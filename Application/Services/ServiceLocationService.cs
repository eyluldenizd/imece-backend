using Application.Common.CompanyScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ServiceLocationService
{
    private readonly ServiceLocationRepository _repository;
    private readonly ServiceLocationTypeRepository _typeRepository;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public ServiceLocationService(
        ServiceLocationRepository repository,
        ServiceLocationTypeRepository typeRepository,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _typeRepository = typeRepository;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceLocationDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var list = await _repository.GetAllAsync(filter, cancellationToken);
        return ServiceResult<IReadOnlyList<ServiceLocationDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<ServiceLocationDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<ServiceLocationDto>.NotFound("Servis konumu bulunamadı.");
        }

        EnsureAccess(entity);
        return ServiceResult<ServiceLocationDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateServiceLocationDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(request.CompanyId.Value);
        }

        var typeResult = await ResolveLocationTypeAsync(
            request.ServiceLocationTypeId,
            request.LocationType,
            cancellationToken);
        if (typeResult.ErrorMessage is not null)
        {
            return ServiceResult<long>.BadRequest(typeResult.ErrorMessage);
        }

        var entity = new ServiceLocations
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            Name = request.Name.Trim(),
            ServiceLocationTypeId = request.ServiceLocationTypeId,
            LocationType = typeResult.Name!,
            Address = NormalizeOptional(request.Address),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsActive = true
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateServiceLocationDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.ServiceLocationId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Servis konumu bulunamadı.");
        }

        if (request.CompanyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(request.CompanyId.Value);
        }

        var typeResult = await ResolveLocationTypeAsync(
            request.ServiceLocationTypeId,
            request.LocationType,
            cancellationToken);
        if (typeResult.ErrorMessage is not null)
        {
            return ServiceResult.BadRequest(typeResult.ErrorMessage);
        }

        entity.CompanyId = request.CompanyId;
        entity.BranchId = request.BranchId;
        entity.Name = request.Name.Trim();
        entity.ServiceLocationTypeId = request.ServiceLocationTypeId;
        entity.LocationType = typeResult.Name!;
        entity.Address = NormalizeOptional(request.Address);
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.IsActive = request.IsActive;

        await _repository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Servis konumu bulunamadı.");
        }

        EnsureAccess(entity);

        var rows = await _repository.SoftDeleteAsync(request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Servis konumu bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private async Task<(string? Name, string? ErrorMessage)> ResolveLocationTypeAsync(
        int? serviceLocationTypeId,
        string? locationType,
        CancellationToken cancellationToken)
    {
        if (serviceLocationTypeId.HasValue)
        {
            var locationTypeEntity = await _typeRepository.GetByIdAsync(
                serviceLocationTypeId.Value,
                cancellationToken);

            if (locationTypeEntity is null)
            {
                return (null, "Servis konum türü bulunamadı.");
            }

            return (locationTypeEntity.Name, null);
        }

        if (string.IsNullOrWhiteSpace(locationType))
        {
            return (null, "Konum türü zorunludur.");
        }

        return (locationType.Trim(), null);
    }

    private void EnsureAccess(ServiceLocations entity)
    {
        if (entity.CompanyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(entity.CompanyId.Value);
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ServiceLocationDto ToDto(ServiceLocations entity) => new()
    {
        ServiceLocationId = entity.ServiceLocationId,
        CompanyId = entity.CompanyId,
        BranchId = entity.BranchId,
        Name = entity.Name,
        ServiceLocationTypeId = entity.ServiceLocationTypeId,
        TypeName = entity.TypeName,
        LocationType = entity.LocationType,
        Address = entity.Address,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
