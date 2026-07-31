using Application.Common.CompanyScope;
using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class TodayInHistoryService
{
    private readonly TodayInHistoryRepository _repository;
    private readonly OrganizationScopeService _organizationScopeService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public TodayInHistoryService(
        TodayInHistoryRepository repository,
        OrganizationScopeService organizationScopeService,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _organizationScopeService = organizationScopeService;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<List<TodayInHistoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var items = await _repository.GetAllAsync(filter, cancellationToken);
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
        var existing = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        CompanyScopeRules.EnsureOrganizationScopeWriteAccess(_companyContext, existing.CompanyScope, existing.CompanyId);
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
