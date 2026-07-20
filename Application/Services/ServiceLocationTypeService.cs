using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ServiceLocationTypeService
{
    private readonly ServiceLocationTypeRepository _repository;

    public ServiceLocationTypeService(ServiceLocationTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceLocationTypeDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<ServiceLocationTypeDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<ServiceLocationTypeDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<ServiceLocationTypeDto>.NotFound("Servis konum türü bulunamadı.");
        }

        return ServiceResult<ServiceLocationTypeDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateServiceLocationTypeDto request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var existing = await _repository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            return ServiceResult<int>.Conflict("Bu tür adı zaten kullanılıyor.");
        }

        var entity = new ServiceLocationTypes
        {
            Name = name,
            Description = NormalizeOptional(request.Description),
            IconUrl = NormalizeOptional(request.IconUrl),
            ColorKey = NormalizeOptional(request.ColorKey),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateServiceLocationTypeDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.ServiceLocationTypeId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Servis konum türü bulunamadı.");
        }

        var name = request.Name.Trim();
        var existing = await _repository.GetByNameAsync(name, cancellationToken);
        if (existing is not null && existing.ServiceLocationTypeId != request.ServiceLocationTypeId)
        {
            return ServiceResult.Conflict("Bu tür adı zaten kullanılıyor.");
        }

        entity.Name = name;
        entity.Description = NormalizeOptional(request.Description);
        entity.IconUrl = NormalizeOptional(request.IconUrl);
        entity.ColorKey = NormalizeOptional(request.ColorKey);
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _repository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Servis konum türü bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ServiceLocationTypeDto ToDto(ServiceLocationTypes entity) => new()
    {
        ServiceLocationTypeId = entity.ServiceLocationTypeId,
        Name = entity.Name,
        Description = entity.Description,
        IconUrl = entity.IconUrl,
        ColorKey = entity.ColorKey,
        IsActive = entity.IsActive,
        SortOrder = entity.SortOrder,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
