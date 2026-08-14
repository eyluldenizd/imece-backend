using Application.Common.CompanyScope;
using Application.Common.ListQuery;
using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ServiceRouteService
{
    private readonly ServiceRouteRepository _repository;
    private readonly ServiceRouteStopRepository _stopRepository;
    private readonly ServiceLocationRepository _locationRepository;
    private readonly OrganizationScopeService _organizationScopeService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public ServiceRouteService(
        ServiceRouteRepository repository,
        ServiceRouteStopRepository stopRepository,
        ServiceLocationRepository locationRepository,
        OrganizationScopeService organizationScopeService,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _stopRepository = stopRepository;
        _locationRepository = locationRepository;
        _organizationScopeService = organizationScopeService;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceRouteDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(
            _companyContext,
            _currentUser,
            query?.CompanyId);
        var entities = await _repository.GetAllAsync(filter, cancellationToken);
        var result = new List<ServiceRouteDto>();

        foreach (var entity in entities)
        {
            result.Add(await ToDtoAsync(entity, cancellationToken));
        }

        return ServiceResult<IReadOnlyList<ServiceRouteDto>>.Success(
            AdminListQueryProfiles.ApplyToServiceRoutes(result, query));
    }

    public async Task<ServiceResult<ServiceRouteDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<ServiceRouteDto>.NotFound("Servis güzergahı bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeReadAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        return ServiceResult<ServiceRouteDto>.Success(await ToDtoAsync(entity, cancellationToken));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateServiceRouteDto request,
        CancellationToken cancellationToken = default)
    {
        var buildResult = await BuildEntityAsync(request, cancellationToken);
        if (buildResult.Error is not null)
        {
            return buildResult.Error;
        }

        var stopsResult = ValidateStops(request.Stops);
        if (stopsResult is not null)
        {
            return ServiceResult<long>.BadRequest(stopsResult);
        }

        var entity = buildResult.Entity!;
        var routeId = await _repository.CreateAsync(entity, cancellationToken);
        await ReplaceStopsAsync(routeId, request.Stops, cancellationToken);

        return ServiceResult<long>.Created(routeId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateServiceRouteDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(request.ServiceRouteId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound("Servis güzergahı bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, existing.CompanyScope, existing.CompanyId);

        var createLike = new CreateServiceRouteDto
        {
            CompanyScope = request.CompanyScope,
            CompanyId = request.CompanyId,
            BranchScope = request.BranchScope,
            BranchId = request.BranchId,
            DepartmentScope = request.DepartmentScope,
            DepartmentId = request.DepartmentId,
            RouteName = request.RouteName,
            DepartureLocation = request.DepartureLocation,
            ArrivalLocation = request.ArrivalLocation,
            DepartureLocationId = request.DepartureLocationId,
            ArrivalLocationId = request.ArrivalLocationId,
            RouteDescription = request.RouteDescription,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            Stops = request.Stops
        };

        var buildResult = await BuildEntityAsync(createLike, cancellationToken);
        if (buildResult.Error is not null)
        {
            return ServiceResult.BadRequest(buildResult.Error.Message!);
        }

        var stopsResult = ValidateStops(request.Stops);
        if (stopsResult is not null)
        {
            return ServiceResult.BadRequest(stopsResult);
        }

        var entity = buildResult.Entity!;
        entity.ServiceRouteId = request.ServiceRouteId;

        await _repository.UpdateAsync(entity, cancellationToken);
        await ReplaceStopsAsync(request.ServiceRouteId, request.Stops, cancellationToken);

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound("Servis güzergahı bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, existing.CompanyScope, existing.CompanyId);
        await _repository.DeleteAsync(request.Id, cancellationToken);
        return ServiceResult.NoContent();
    }

    private async Task<(ServiceRoutes? Entity, ServiceResult<long>? Error)> BuildEntityAsync(
        CreateServiceRouteDto request,
        CancellationToken cancellationToken)
    {
        NormalizeIncomingScope(request);

        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return (null, ServiceResult<long>.BadRequest(scopeResult.ErrorMessage));
        }

        if (string.IsNullOrWhiteSpace(request.RouteName))
        {
            return (null, ServiceResult<long>.BadRequest("Güzergah adı zorunludur."));
        }

        if (request.DepartureLocationId.HasValue
            && request.ArrivalLocationId.HasValue
            && request.DepartureLocationId == request.ArrivalLocationId)
        {
            return (null, ServiceResult<long>.BadRequest("Kalkış ve varış konumu aynı olamaz."));
        }

        string departureLocation = request.DepartureLocation?.Trim() ?? string.Empty;
        string arrivalLocation = request.ArrivalLocation?.Trim() ?? string.Empty;
        var resolvedCompanyId = scopeResult.Resolved!.CompanyId;

        if (request.DepartureLocationId.HasValue)
        {
            var location = await _locationRepository.GetByIdAsync(
                request.DepartureLocationId.Value,
                cancellationToken);

            if (location is null)
            {
                return (null, ServiceResult<long>.BadRequest("Geçersiz kalkış konumu ID değeri."));
            }

            var locationError = EnsureLocationMatchesRouteCompany(location, resolvedCompanyId);
            if (locationError is not null)
            {
                return (null, ServiceResult<long>.BadRequest(locationError));
            }

            departureLocation = string.IsNullOrWhiteSpace(departureLocation) ? location.Name : departureLocation;
        }

        if (request.ArrivalLocationId.HasValue)
        {
            var location = await _locationRepository.GetByIdAsync(
                request.ArrivalLocationId.Value,
                cancellationToken);

            if (location is null)
            {
                return (null, ServiceResult<long>.BadRequest("Geçersiz varış konumu ID değeri."));
            }

            var locationError = EnsureLocationMatchesRouteCompany(location, resolvedCompanyId);
            if (locationError is not null)
            {
                return (null, ServiceResult<long>.BadRequest(locationError));
            }

            arrivalLocation = string.IsNullOrWhiteSpace(arrivalLocation) ? location.Name : arrivalLocation;
        }

        if (string.IsNullOrWhiteSpace(departureLocation) || string.IsNullOrWhiteSpace(arrivalLocation))
        {
            return (null, ServiceResult<long>.BadRequest(
                "Kalkış/varış konumu metni veya locationId zorunludur."));
        }

        try
        {
            var entity = new ServiceRoutes
            {
                RouteName = request.RouteName.Trim(),
                DepartureLocation = departureLocation,
                ArrivalLocation = arrivalLocation,
                DepartureLocationId = request.DepartureLocationId,
                ArrivalLocationId = request.ArrivalLocationId,
                RouteDescription = request.RouteDescription,
                DepartureTime = ParseRouteTime(request.DepartureTime),
                ArrivalTime = ParseRouteTime(request.ArrivalTime),
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder
            };
            ApplyScope(entity, scopeResult.Resolved!);
            return (entity, null);
        }
        catch (FormatException ex)
        {
            return (null, ServiceResult<long>.BadRequest(ex.Message));
        }
    }

    private string? EnsureLocationMatchesRouteCompany(ServiceLocations location, int? companyId)
    {
        CompanyScopeRules.EnsureOrganizationScopeReadAccess(_companyContext, location.CompanyScope, location.CompanyId);
        if (companyId.HasValue
            && location.CompanyId.HasValue
            && location.CompanyId.Value != companyId.Value)
        {
            return "Seçilen konumlar güzergah şirketi ile aynı olmalıdır.";
        }

        return null;
    }

    private static void NormalizeIncomingScope(OrganizationScopeFieldsDto dto)
    {
        if (string.Equals(dto.CompanyScope, "Multiple", StringComparison.OrdinalIgnoreCase))
        {
            dto.CompanyScope = OrganizationScopeFieldHelper.All;
            dto.CompanyId = null;
            dto.BranchScope = OrganizationScopeFieldHelper.All;
            dto.BranchId = null;
            dto.DepartmentScope = OrganizationScopeFieldHelper.All;
            dto.DepartmentId = null;
        }
    }

    private static void ApplyScope(ServiceRoutes entity, ResolvedOrganizationScope resolved)
    {
        OrganizationScopeService.ApplyToEntity(
            resolved,
            (companyScope, companyId, branchScope, branchId, departmentScope, departmentId) =>
            {
                entity.CompanyScope = companyScope;
                entity.CompanyId = companyId;
                entity.BranchScope = branchScope;
                entity.BranchId = branchId;
                entity.DepartmentScope = departmentScope;
                entity.DepartmentId = departmentId;
            });
    }

    private static string? ValidateStops(IReadOnlyList<ServiceRouteStopInputDto>? stops)
    {
        if (stops is null || stops.Count == 0)
        {
            return null;
        }

        var orders = stops.Select(s => s.StopOrder).ToList();
        if (orders.Distinct().Count() != orders.Count)
        {
            return "Durak sıra numaraları benzersiz olmalıdır.";
        }

        var sorted = orders.OrderBy(o => o).ToList();
        for (var i = 0; i < sorted.Count; i++)
        {
            if (sorted[i] != i + 1)
            {
                return "Durak sıra numaraları 1'den başlayarak ardışık olmalıdır.";
            }
        }

        return null;
    }

    private async Task ReplaceStopsAsync(
        long routeId,
        IReadOnlyList<ServiceRouteStopInputDto>? stops,
        CancellationToken cancellationToken)
    {
        await _stopRepository.DeleteByRouteIdAsync(routeId, cancellationToken);

        if (stops is null)
        {
            return;
        }

        foreach (var stop in stops.OrderBy(s => s.StopOrder))
        {
            await _stopRepository.CreateAsync(
                new ServiceRouteStops
                {
                    ServiceRouteId = routeId,
                    ServiceLocationId = stop.ServiceLocationId,
                    StopOrder = stop.StopOrder,
                    ArrivalTime = ParseRouteTime(stop.ArrivalTime),
                    DepartureTime = ParseRouteTime(stop.DepartureTime),
                    Notes = stop.Notes,
                    IsActive = stop.IsActive
                },
                cancellationToken);
        }
    }

    private async Task<ServiceRouteDto> ToDtoAsync(
        ServiceRoutes entity,
        CancellationToken cancellationToken)
    {
        var stops = await _stopRepository.GetByRouteIdAsync(entity.ServiceRouteId, cancellationToken);

        return new ServiceRouteDto
        {
            ServiceRouteId = entity.ServiceRouteId,
            RouteName = entity.RouteName,
            DepartureLocation = entity.DepartureLocation,
            ArrivalLocation = entity.ArrivalLocation,
            DepartureLocationId = entity.DepartureLocationId,
            ArrivalLocationId = entity.ArrivalLocationId,
            RouteDescription = entity.RouteDescription,
            DepartureTime = FormatRouteTime(entity.DepartureTime),
            ArrivalTime = FormatRouteTime(entity.ArrivalTime),
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CompanyScope = entity.CompanyScope,
            CompanyId = entity.CompanyId,
            BranchScope = entity.BranchScope,
            BranchId = entity.BranchId,
            DepartmentScope = entity.DepartmentScope,
            DepartmentId = entity.DepartmentId,
            CompanyName = entity.CompanyName,
            BranchName = entity.BranchName,
            DepartmentName = entity.DepartmentName,
            Stops = stops.Select(s => new ServiceRouteStopDto
            {
                ServiceRouteStopId = s.ServiceRouteStopId,
                ServiceRouteId = s.ServiceRouteId,
                ServiceLocationId = s.ServiceLocationId,
                StopOrder = s.StopOrder,
                ArrivalTime = FormatRouteTime(s.ArrivalTime),
                DepartureTime = FormatRouteTime(s.DepartureTime),
                Notes = s.Notes,
                IsActive = s.IsActive
            }).ToList()
        };
    }

    private static string? FormatRouteTime(TimeSpan? time) =>
        time.HasValue
            ? $"{time.Value.Hours:D2}:{time.Value.Minutes:D2}:{time.Value.Seconds:D2}"
            : null;

    private static TimeSpan? ParseRouteTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return TimeSpan.TryParse(value, out var parsed)
            ? parsed
            : throw new FormatException($"Geçersiz saat formatı: {value}. HH:mm:ss bekleniyor.");
    }
}
