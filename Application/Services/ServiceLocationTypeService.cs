using Application.Common.CompanyScope;
using Application.Common.ListQuery;
using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ServiceLocationTypeService
{
    private readonly ServiceLocationTypeRepository _repository;
    private readonly OrganizationScopeService _organizationScopeService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public ServiceLocationTypeService(
        ServiceLocationTypeRepository repository,
        OrganizationScopeService organizationScopeService,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _organizationScopeService = organizationScopeService;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceLocationTypeDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(
            _companyContext,
            _currentUser,
            query?.CompanyId);
        var list = await _repository.GetAllAsync(filter, cancellationToken);
        var dtos = list.Select(ToDto).ToList();
        return ServiceResult<IReadOnlyList<ServiceLocationTypeDto>>.Success(
            AdminListQueryProfiles.ApplyToServiceLocationTypes(dtos, query));
    }

    public async Task<ServiceResult<ServiceLocationTypeDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<ServiceLocationTypeDto>.NotFound("Servis konum türü bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeReadAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        return ServiceResult<ServiceLocationTypeDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateServiceLocationTypeDto request,
        CancellationToken cancellationToken = default)
    {
        NormalizeIncomingScope(request);

        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult<int>.BadRequest(scopeResult.ErrorMessage);
        }

        var name = request.Name.Trim();
        var existing = await _repository.GetByNameInCompanyAsync(
            name,
            scopeResult.Resolved!.CompanyId,
            cancellationToken);
        if (existing is not null)
        {
            return ServiceResult<int>.Conflict("Bu tür adı zaten kullanılıyor.");
        }

        var entity = new ServiceLocationTypes
        {
            Name = name,
            Description = NormalizeOptional(request.Description),
            IconUrl = NormalizeOptional(request.IconUrl),
            ColorKey = NormalizeOptional(request.ColorKey),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
        ApplyScope(entity, scopeResult.Resolved!);

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateServiceLocationTypeDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.ServiceLocationTypeId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Servis konum türü bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        NormalizeIncomingScope(request);

        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult.BadRequest(scopeResult.ErrorMessage);
        }

        var name = request.Name.Trim();
        var existing = await _repository.GetByNameInCompanyAsync(
            name,
            scopeResult.Resolved!.CompanyId,
            cancellationToken);
        if (existing is not null && existing.ServiceLocationTypeId != request.ServiceLocationTypeId)
        {
            return ServiceResult.Conflict("Bu tür adı zaten kullanılıyor.");
        }

        entity.Name = name;
        entity.Description = NormalizeOptional(request.Description);
        entity.IconUrl = NormalizeOptional(request.IconUrl);
        entity.ColorKey = NormalizeOptional(request.ColorKey);
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Servis konum türü bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, entity.CompanyScope, entity.CompanyId);

        var rows = await _repository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Servis konum türü bulunamadı.");
        }

        return ServiceResult.NoContent();
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

    private static void ApplyScope(ServiceLocationTypes entity, ResolvedOrganizationScope resolved)
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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ServiceLocationTypeDto ToDto(ServiceLocationTypes entity) => new()
    {
        ServiceLocationTypeId = entity.ServiceLocationTypeId,
        Name = entity.Name,
        Description = entity.Description,
        IconUrl = entity.IconUrl,
        ColorKey = entity.ColorKey,
        IsActive = entity.IsActive,
        SortOrder = entity.SortOrder,
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
        DepartmentName = entity.DepartmentName
    };
}
