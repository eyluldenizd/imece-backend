using Application.Common.OrganizationScope;
using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ECardService
{
    private readonly ECardRepository _repository;
    private readonly OrganizationScopeService _organizationScopeService;

    public ECardService(
        ECardRepository repository,
        OrganizationScopeService organizationScopeService)
    {
        _repository = repository;
        _organizationScopeService = organizationScopeService;
    }

    public async Task<List<ECardDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(ToDto).ToList();
    }

    public async Task<ECardDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task CreateAsync(ECardDto dto, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var entity = new ECards
        {
            Title = dto.Title,
            Description = dto.Description,
            CardType = dto.CardType,
            ImageUrl = dto.ImageUrl,
            RedirectUrl = dto.RedirectUrl,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.CreateAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(ECardDto dto, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var entity = new ECards
        {
            ECardId = dto.ECardId,
            Title = dto.Title,
            Description = dto.Description,
            CardType = dto.CardType,
            ImageUrl = dto.ImageUrl,
            RedirectUrl = dto.RedirectUrl,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    private static void ApplyScope(ECards entity, ResolvedOrganizationScope resolved)
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

    private static ECardDto ToDto(ECards entity) => new()
    {
        ECardId = entity.ECardId,
        Title = entity.Title,
        Description = entity.Description,
        CardType = entity.CardType,
        ImageUrl = entity.ImageUrl,
        RedirectUrl = entity.RedirectUrl,
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
