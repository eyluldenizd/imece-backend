using Application.Common.CompanyScope;
using Application.Common.ListQuery;
using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Core.Entities;
using Infrastructure.Repositories;
using ServiceEntity = Core.Entities.Services;

namespace Application.Services;

public sealed class ServicesService
{
    private readonly ServicesRepository _servicesRepository;
    private readonly OrganizationScopeService _organizationScopeService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public ServicesService(
        ServicesRepository servicesRepository,
        OrganizationScopeService organizationScopeService,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _servicesRepository = servicesRepository;
        _organizationScopeService = organizationScopeService;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser, query?.CompanyId);
        var list = await _servicesRepository.GetAllAsync(filter, cancellationToken);
        var dtos = list.Select(ToDto).ToList();
        return ServiceResult<IReadOnlyList<ServiceDto>>.Success(
            AdminListQueryProfiles.ApplyToServices(dtos, query));
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var list = await _servicesRepository.GetActiveAsync(filter, cancellationToken);
        return ServiceResult<IReadOnlyList<ServiceDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<ServiceDto>> GetByIdAsync(IdRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _servicesRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return ServiceResult<ServiceDto>.NotFound("Hizmet bulunamadı.");

        CompanyScopeRules.EnsureOrganizationScopeReadAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        return ServiceResult<ServiceDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(CreateServiceDto request, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult<long>.BadRequest(scopeResult.ErrorMessage);
        }

        var entity = new ServiceEntity
        {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            IsActive = true
        };

        ApplyScope(entity, scopeResult.Resolved!);

        var id = await _servicesRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(UpdateServiceDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _servicesRepository.GetByIdAsync(request.ServiceId, cancellationToken);
        if (entity is null)
            return ServiceResult.NotFound("Hizmet bulunamadı.");

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult.BadRequest(scopeResult.ErrorMessage);
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Icon = request.Icon;
        entity.IsActive = request.IsActive;
        ApplyScope(entity, scopeResult.Resolved!);

        await _servicesRepository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(IdRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _servicesRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return ServiceResult.NotFound("Hizmet bulunamadı.");

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        var rows = await _servicesRepository.SoftDeleteAsync(request.Id, cancellationToken);
        if (rows == 0)
            return ServiceResult.NotFound("Hizmet bulunamadı.");

        return ServiceResult.NoContent();
    }

    private static void ApplyScope(ServiceEntity entity, ResolvedOrganizationScope resolved)
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

    private static ServiceDto ToDto(ServiceEntity entity) => new()
    {
        ServiceId = entity.ServiceId,
        Name = entity.Name,
        Description = entity.Description,
        Icon = entity.Icon,
        IsActive = entity.IsActive,
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
