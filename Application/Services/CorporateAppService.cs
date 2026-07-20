using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Common;
using Core.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CorporateAppService
{
    private readonly CorporateAppsRepository _repository;
    private readonly CorporateAppCategoryRepository _categoryRepository;
    private readonly OrganizationScopeService _organizationScopeService;

    public CorporateAppService(
        CorporateAppsRepository repository,
        CorporateAppCategoryRepository categoryRepository,
        OrganizationScopeService organizationScopeService)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _organizationScopeService = organizationScopeService;
    }

    public async Task<ServiceResult<IReadOnlyList<CorporateAppDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CorporateAppDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<CorporateAppDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<CorporateAppDto>.NotFound("Kurumsal uygulama bulunamadı.");
        }

        return ServiceResult<CorporateAppDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateCorporateAppDto request,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult<long>.BadRequest(scopeResult.ErrorMessage);
        }

        var categoryResult = await ResolveCategoryAsync(
            request.CorporateAppCategoryId,
            request.Category,
            cancellationToken);
        if (categoryResult.ErrorMessage is not null)
        {
            return ServiceResult<long>.BadRequest(categoryResult.ErrorMessage);
        }

        var entity = new CorporateApps
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Url = request.Url.Trim(),
            CorporateAppCategoryId = request.CorporateAppCategoryId,
            Category = categoryResult.Name,
            IconUrl = NormalizeOptional(request.IconUrl),
            IsActive = request.IsActive
        };

        ApplyScope(entity, scopeResult.Resolved!);

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateCorporateAppDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.AppId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Kurumsal uygulama bulunamadı.");
        }

        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult.BadRequest(scopeResult.ErrorMessage);
        }

        var categoryResult = await ResolveCategoryAsync(
            request.CorporateAppCategoryId,
            request.Category,
            cancellationToken);
        if (categoryResult.ErrorMessage is not null)
        {
            return ServiceResult.BadRequest(categoryResult.ErrorMessage);
        }

        entity.Title = request.Title.Trim();
        entity.Description = request.Description;
        entity.Url = request.Url.Trim();
        entity.CorporateAppCategoryId = request.CorporateAppCategoryId;
        entity.Category = categoryResult.Name;
        entity.IconUrl = NormalizeOptional(request.IconUrl);
        entity.IsActive = request.IsActive;
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository.SoftDeleteAsync(request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Kurumsal uygulama bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private async Task<(string? Name, string? ErrorMessage)> ResolveCategoryAsync(
        int? corporateAppCategoryId,
        string? category,
        CancellationToken cancellationToken)
    {
        if (corporateAppCategoryId.HasValue)
        {
            var appCategory = await _categoryRepository.GetByIdAsync(
                corporateAppCategoryId.Value,
                cancellationToken);

            if (appCategory is null)
            {
                return (null, "Kurumsal uygulama kategorisi bulunamadı.");
            }

            return (appCategory.Name, null);
        }

        return (NormalizeOptional(category), null);
    }

    private static void ApplyScope(CorporateApps entity, ResolvedOrganizationScope resolved)
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

    private static CorporateAppDto ToDto(CorporateApps entity) => new()
    {
        AppId = entity.AppId,
        Title = entity.Title,
        Description = entity.Description,
        Url = entity.Url,
        CorporateAppCategoryId = entity.CorporateAppCategoryId,
        CategoryName = entity.CategoryName,
        Category = entity.Category,
        IconUrl = entity.IconUrl,
        IsActive = entity.IsActive,
        CompanyScope = entity.CompanyScope,
        CompanyId = entity.CompanyId,
        BranchScope = entity.BranchScope,
        BranchId = entity.BranchId,
        DepartmentScope = entity.DepartmentScope,
        DepartmentId = entity.DepartmentId
    };
}
