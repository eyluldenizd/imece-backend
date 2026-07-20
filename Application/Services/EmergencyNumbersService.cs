using Application.Common.OrganizationScope;
using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class EmergencyNumberService
{
    private readonly EmergencyNumberRepository _repository;
    private readonly OrganizationScopeService _organizationScopeService;

    public EmergencyNumberService(
        EmergencyNumberRepository repository,
        OrganizationScopeService organizationScopeService)
    {
        _repository = repository;
        _organizationScopeService = organizationScopeService;
    }

    public async Task<List<EmergencyNumberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(ToDto).ToList();
    }

    public async Task<EmergencyNumberDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task CreateAsync(EmergencyNumberDto dto, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var entity = new EmergencyNumbers
        {
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            Category = dto.Category,
            Description = dto.Description,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.CreateAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(EmergencyNumberDto dto, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var entity = new EmergencyNumbers
        {
            EmergencyNumberId = dto.EmergencyNumberId,
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            Category = dto.Category,
            Description = dto.Description,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

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

    private static EmergencyNumberDto ToDto(EmergencyNumbers entity) => new()
    {
        EmergencyNumberId = entity.EmergencyNumberId,
        Name = entity.Name,
        PhoneNumber = entity.PhoneNumber,
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
        DepartmentId = entity.DepartmentId
    };
}
