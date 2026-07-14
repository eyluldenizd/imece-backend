using Application.DTOs;

using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class EventService
{
    private readonly EventRepository _eventRepository;

    public EventService(
        EventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<EventDto>>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetAllAsync(
            cancellationToken);

        IReadOnlyList<EventDto> response = events
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<EventDto>>
            .Success(response);
    }

    public async Task<ServiceResult<IReadOnlyList<EventDto>>>
        GetUpcomingAsync(
            CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetUpcomingAsync(
            cancellationToken);

        IReadOnlyList<EventDto> response = events
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<EventDto>>
            .Success(response);
    }

    public async Task<ServiceResult<EventDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult<EventDto>.NotFound(
                $"ID değeri {request.Id} olan etkinlik bulunamadı.");
        }

        return ServiceResult<EventDto>.Success(
            ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateEventDto request,
        CancellationToken cancellationToken = default)
    {
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
            CreatedBy = request.CreatedBy
        };

        var eventId = await _eventRepository.CreateAsync(
            entity,
            cancellationToken);

        return ServiceResult<long>.Created(eventId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateEventDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _eventRepository.GetByIdAsync(
            request.EventId,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.EventId} olan etkinlik bulunamadı.");
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.EventType = request.EventType;
        entity.Location = request.Location;
        entity.CoverImageUrl = request.CoverImageUrl;
        entity.StartDateTime = request.StartDateTime;
        entity.EndDateTime = request.EndDateTime;
        entity.IsAllDay = request.IsAllDay;
        entity.CreatedBy = request.CreatedBy;

        var rowsAffected = await _eventRepository.UpdateAsync(
            entity,
            cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Etkinlik güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _eventRepository.DeleteAsync(
            request.Id,
            cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan etkinlik bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static EventDto ToDto(
        Events entity)
    {
        return new EventDto
        {
            EventId = entity.EventId,
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
    }
}