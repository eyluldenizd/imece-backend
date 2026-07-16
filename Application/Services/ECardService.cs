using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ECardService
{
    private readonly ECardRepository _repository;

    public ECardService(
        ECardRepository repository)
    {
        _repository = repository;
    }


    public async Task<List<ECardDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(
            cancellationToken);

        return entities.Select(x => new ECardDto
        {
            ECardId = x.ECardId,
            Title = x.Title,
            Description = x.Description,
            CardType = x.CardType,
            ImageUrl = x.ImageUrl,
            RedirectUrl = x.RedirectUrl,
            IsActive = x.IsActive,
            DisplayOrder = x.DisplayOrder,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt

        }).ToList();
    }


    public async Task<ECardDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (entity == null)
        {
            return null;
        }


        return new ECardDto
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
            UpdatedAt = entity.UpdatedAt
        };
    }


    public Task CreateAsync(
        ECardDto dto,
        CancellationToken cancellationToken = default)
    {
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


        return _repository.CreateAsync(
            entity,
            cancellationToken);
    }


    public Task UpdateAsync(
        ECardDto dto,
        CancellationToken cancellationToken = default)
    {
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


        return _repository.UpdateAsync(
            entity,
            cancellationToken);
    }


    public Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(
            id,
            cancellationToken);
    }
}