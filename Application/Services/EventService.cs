using Application.Common.CompanyScope;
using Application.Common.OrganizationScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class EventService
{
    private readonly EventRepository _eventRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyContext _companyContext;
    private readonly OrganizationScopeService _organizationScopeService;

    public EventService(
        EventRepository eventRepository,
        ICurrentUser currentUser,
        ICompanyContext companyContext,
        OrganizationScopeService organizationScopeService)
    {
        _eventRepository = eventRepository;
        _currentUser = currentUser;
        _companyContext = companyContext;
        _organizationScopeService = organizationScopeService;
    }

    public async Task<ServiceResult<IReadOnlyList<EventDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetAllAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
            cancellationToken);

        return ServiceResult<IReadOnlyList<EventDto>>.Success(events.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<EventDto>>> GetUpcomingAsync(
        CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetUpcomingAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
            cancellationToken);

        return ServiceResult<IReadOnlyList<EventDto>>.Success(events.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<EventDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<EventDto>.NotFound($"ID değeri {request.Id} olan etkinlik bulunamadı.");
        }

        CompanyScopeRules.EnsureContentReadAccess(_companyContext, entity.ScopeType, entity.CompanyId);
        return ServiceResult<EventDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateEventDto request,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult<long>.BadRequest(scopeResult.ErrorMessage);
        }

        var entity = new Events
        {
            Title = request.Title,
            Description = request.Description,
            EventType = request.EventType,
            Location = request.Location,
            CoverImageUrl = request.CoverImageUrl,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            IsAllDay = request.IsAllDay,
            CreatedBy = _currentUser.GetRequiredUserId()
        };

        ApplyOrganizationScope(entity, scopeResult.Resolved!);
        CompanyScopeRules.EnsureContentWriteAccess(
            _companyContext,
            _currentUser,
            entity.ScopeType,
            entity.CompanyId);

        var eventId = await _eventRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(eventId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateEventDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound($"ID değeri {request.EventId} olan etkinlik bulunamadı.");
        }

        CompanyScopeRules.EnsureContentWriteAccess(
            _companyContext,
            _currentUser,
            entity.ScopeType,
            entity.CompanyId);

        var scopeResult = await _organizationScopeService.ResolveAsync(request, cancellationToken);
        if (scopeResult.ErrorMessage is not null)
        {
            return ServiceResult.BadRequest(scopeResult.ErrorMessage);
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.EventType = request.EventType;
        entity.Location = request.Location;
        entity.CoverImageUrl = request.CoverImageUrl;
        entity.StartDateTime = request.StartDateTime;
        entity.EndDateTime = request.EndDateTime;
        entity.IsAllDay = request.IsAllDay;
        ApplyOrganizationScope(entity, scopeResult.Resolved!);
        CompanyScopeRules.EnsureContentWriteAccess(
            _companyContext,
            _currentUser,
            entity.ScopeType,
            entity.CompanyId);

        var rowsAffected = await _eventRepository.UpdateAsync(entity, cancellationToken);
        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict("Etkinlik güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound($"ID değeri {request.Id} olan etkinlik bulunamadı.");
        }

        CompanyScopeRules.EnsureContentWriteAccess(
            _companyContext,
            _currentUser,
            entity.ScopeType,
            entity.CompanyId);

        var rowsAffected = await _eventRepository.DeleteAsync(request.Id, cancellationToken);
        if (rowsAffected == 0)
        {
            return ServiceResult.NotFound($"ID değeri {request.Id} olan etkinlik bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static void ApplyOrganizationScope(Events entity, ResolvedOrganizationScope resolved)
    {
        OrganizationScopeService.ApplyToEntity(
            resolved,
            (companyScope, companyId, branchScope, branchId, departmentScope, departmentId) =>
            {
                entity.BranchScope = branchScope;
                entity.BranchId = branchId;
                entity.DepartmentScope = departmentScope;
                entity.DepartmentId = departmentId;

                if (companyScope.Equals(OrganizationScopeFieldHelper.All, StringComparison.OrdinalIgnoreCase))
                {
                    entity.ScopeType = ContentScopeTypes.Global;
                    entity.CompanyId = null;
                    return;
                }

                entity.ScopeType = ContentScopeTypes.Company;
                entity.CompanyId = companyId;
            });
    }

    private static EventDto ToDto(Events entity) => new()
    {
        EventId = entity.EventId,
        CompanyId = entity.CompanyId,
        ScopeType = entity.ScopeType,
        CompanyScope = MapCompanyScope(entity.ScopeType),
        BranchScope = entity.BranchScope,
        BranchId = entity.BranchId,
        DepartmentScope = entity.DepartmentScope,
        DepartmentId = entity.DepartmentId,
        Title = entity.Title,
        Description = entity.Description,
        EventType = entity.EventType,
        Location = entity.Location,
        CoverImageUrl = entity.CoverImageUrl,
        StartDateTime = entity.StartDateTime,
        EndDateTime = entity.EndDateTime,
        IsAllDay = entity.IsAllDay,
        CreatedBy = entity.CreatedBy,
        CreatedAt = entity.CreatedAt
    };

    private static string MapCompanyScope(string scopeType) =>
        scopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase)
            ? OrganizationScopeFieldHelper.All
            : OrganizationScopeFieldHelper.Specific;
}
