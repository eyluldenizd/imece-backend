using Application.DTOs;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class UpcomingEventService
{
    private readonly UpcomingEventsRepository _repository;

    public UpcomingEventService(UpcomingEventsRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<UpcomingEventDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var dtos = list.Select(x => new UpcomingEventDto
        {
            EventId = x.EventId,
            Title = x.Title,
            Description = x.Description,
            EventDate = x.EventDate,
            Location = x.Location
        }).ToList();

        return ServiceResult<IReadOnlyList<UpcomingEventDto>>.Success(dtos);
    }
}
