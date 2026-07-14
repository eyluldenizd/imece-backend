using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class TodayInHistoryService
{
    private readonly TodayInHistoryRepository _repository;

    public TodayInHistoryService(TodayInHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TodayInHistoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);

        return items.Select(x => new TodayInHistoryDto
        {
            Id = x.Id,
            EventDate = x.EventDate,
            Title = x.Title,
            Description = x.Description,
            ImageUrl = x.ImageUrl,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public Task CreateAsync(
        TodayInHistoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new TodayInHistory
        {
            Id = dto.Id,
            EventDate = dto.EventDate,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedAt = dto.CreatedAt
        };

        return _repository.CreateAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(
        TodayInHistoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new TodayInHistory
        {
            Id = dto.Id,
            EventDate = dto.EventDate,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedAt = dto.CreatedAt
        };

        return _repository.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }
}