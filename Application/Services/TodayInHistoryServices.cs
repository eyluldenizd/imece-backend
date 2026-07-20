using Application.Common.OrganizationScope;
using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class TodayInHistoryService
{
    private readonly TodayInHistoryRepository _repository;
    private readonly OrganizationScopeService _organizationScopeService;

    public TodayInHistoryService(
        TodayInHistoryRepository repository,
        OrganizationScopeService organizationScopeService)
    {
        _repository = repository;
        _organizationScopeService = organizationScopeService;
    }

    public async Task<List<TodayInHistoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task CreateAsync(TodayInHistoryDto dto, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var entity = new TodayInHistory
        {
            EventDate = dto.EventDate,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedAt = dto.CreatedAt
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.CreateAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(TodayInHistoryDto dto, CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(dto, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            throw new InvalidOperationException(scopeResult.ErrorMessage);
        }

        var entity = new TodayInHistory
        {
            Id = dto.Id,
            EventDate = dto.EventDate,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedAt = dto.CreatedAt
        };
        ApplyScope(entity, scopeResult.Resolved!);

        await _repository.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    private static void ApplyScope(TodayInHistory entity, ResolvedOrganizationScope resolved)
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

    private static TodayInHistoryDto ToDto(TodayInHistory entity) => new()
    {
        Id = entity.Id,
        EventDate = entity.EventDate,
        Title = entity.Title,
        Description = entity.Description,
        ImageUrl = entity.ImageUrl,
        CreatedAt = entity.CreatedAt,
        CompanyScope = entity.CompanyScope,
        CompanyId = entity.CompanyId,
        BranchScope = entity.BranchScope,
        BranchId = entity.BranchId,
        DepartmentScope = entity.DepartmentScope,
        DepartmentId = entity.DepartmentId
    };
}
