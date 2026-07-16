using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class EmergencyNumberService
{
    private readonly EmergencyNumberRepository _repository;

    public EmergencyNumberService(
        EmergencyNumberRepository repository)
    {
        _repository = repository;
    }


    public async Task<List<EmergencyNumberDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);

        return entities.Select(x => new EmergencyNumberDto
        {
            EmergencyNumberId = x.EmergencyNumberId,
            Name = x.Name,
            PhoneNumber = x.PhoneNumber,
            Category = x.Category,
            Description = x.Description,
            IsActive = x.IsActive,
            DisplayOrder = x.DisplayOrder,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }


    public async Task<EmergencyNumberDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (entity == null)
            return null;

        return new EmergencyNumberDto
        {
            EmergencyNumberId = entity.EmergencyNumberId,
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            Category = entity.Category,
            Description = entity.Description,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }


    public Task CreateAsync(
        EmergencyNumberDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new EmergencyNumbers
        {
            Name = dto.Name,
            PhoneNumber = dto.PhoneNumber,
            Category = dto.Category,
            Description = dto.Description,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        return _repository.CreateAsync(
            entity,
            cancellationToken);
    }


    public Task UpdateAsync(
        EmergencyNumberDto dto,
        CancellationToken cancellationToken = default)
    {
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