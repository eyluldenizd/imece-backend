using Application.Common.CompanyScope;
using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class EmergencyNumberService
{
    private readonly EmergencyNumberRepository _repository;
    private readonly EmergencyNumberCategoryRepository _categoryRepository;
    private readonly OrganizationScopeService _organizationScopeService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public EmergencyNumberService(
        EmergencyNumberRepository repository,
        EmergencyNumberCategoryRepository categoryRepository,
        OrganizationScopeService organizationScopeService,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _organizationScopeService = organizationScopeService;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<List<EmergencyNumberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var entities = await _repository.GetAllAsync(filter, cancellationToken);
        return entities.Select(ToDto).ToList();
    }

    public async Task<EmergencyNumberDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            CompanyScopeRules.EnsureOrganizationScopeReadAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        }
        return entity is null ? null : ToDto(entity);
    }

    public async Task CreateAsync(EmergencyNumberDto dto, CancellationToken cancellationToken = default)
    {
        NormalizeIncomingScope(dto);

        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var categoryResult = await ResolveCategoryAsync(
            dto.EmergencyNumberCategoryId,
            dto.Category,
            cancellationToken);
        if (categoryResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(categoryResult.ErrorMessage);
        }

        var entity = new EmergencyNumbers
        {
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            EmergencyNumberCategoryId = dto.EmergencyNumberCategoryId,
            Category = categoryResult.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.CreateAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(EmergencyNumberDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(dto.EmergencyNumberId, cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException("Acil numara kaydı bulunamadı.");
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, existing.CompanyScope, existing.CompanyId);
        NormalizeIncomingScope(dto);
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var categoryResult = await ResolveCategoryAsync(
            dto.EmergencyNumberCategoryId,
            dto.Category,
            cancellationToken);
        if (categoryResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(categoryResult.ErrorMessage);
        }

        var entity = new EmergencyNumbers
        {
            EmergencyNumberId = dto.EmergencyNumberId,
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            EmergencyNumberCategoryId = dto.EmergencyNumberCategoryId,
            Category = categoryResult.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.UpdateAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, entity.CompanyScope, entity.CompanyId);
        await _repository.DeleteAsync(id, cancellationToken);
    }

    private static void NormalizeIncomingScope(EmergencyNumberDto dto)
    {
        // Admin multi-company payload uses "Multiple"; org resolver only accepts All|Specific.
        if (string.Equals(dto.CompanyScope, "Multiple", StringComparison.OrdinalIgnoreCase))
        {
            dto.CompanyScope = OrganizationScopeFieldHelper.All;
            dto.CompanyId = null;
            dto.BranchScope = OrganizationScopeFieldHelper.All;
            dto.BranchId = null;
            dto.DepartmentScope = OrganizationScopeFieldHelper.All;
            dto.DepartmentId = null;
        }

        if (dto.EmergencyNumberCategoryId is <= 0)
        {
            dto.EmergencyNumberCategoryId = null;
        }
    }

    private async Task<(string? Name, string? ErrorMessage)> ResolveCategoryAsync(
        int? emergencyNumberCategoryId,
        string? category,
        CancellationToken cancellationToken)
    {
        if (emergencyNumberCategoryId is > 0)
        {
            var numberCategory = await _categoryRepository.GetByIdAsync(
                emergencyNumberCategoryId.Value,
                cancellationToken);

            if (numberCategory is null)
            {
                return (null, "Acil numara kategorisi bulunamadı.");
            }

            return (numberCategory.Name, null);
        }

        return (NormalizeOptional(category), null);
    }

    private static void ApplyScope(EmergencyNumbers entity, ResolvedOrganizationScope resolved)
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

    private static EmergencyNumberDto ToDto(EmergencyNumbers entity) => new()
    {
        EmergencyNumberId = entity.EmergencyNumberId,
        Name = entity.Name,
        PhoneNumber = entity.PhoneNumber,
        EmergencyNumberCategoryId = entity.EmergencyNumberCategoryId,
        CategoryName = entity.CategoryName,
        Category = entity.Category,
        Description = entity.Description,
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
        DepartmentName = entity.DepartmentName
    };
}
