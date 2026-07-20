using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CommunicationChannelTypeService
{
    private readonly CommunicationChannelTypeRepository _repository;

    public CommunicationChannelTypeService(CommunicationChannelTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<CommunicationChannelTypeDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CommunicationChannelTypeDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<CommunicationChannelTypeDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<CommunicationChannelTypeDto>.NotFound("İletişim kanalı türü bulunamadı.");
        }

        return ServiceResult<CommunicationChannelTypeDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateCommunicationChannelTypeDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByCodeAsync(request.Code.Trim(), cancellationToken);
        if (existing is not null)
        {
            return ServiceResult<int>.Conflict("Bu tür kodu zaten kullanılıyor.");
        }

        var entity = new CommunicationChannelTypes
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            IconUrl = NormalizeOptional(request.IconUrl),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var id = await _repository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateCommunicationChannelTypeDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.CommunicationChannelTypeId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("İletişim kanalı türü bulunamadı.");
        }

        var existing = await _repository.GetByCodeAsync(request.Code.Trim(), cancellationToken);
        if (existing is not null && existing.CommunicationChannelTypeId != request.CommunicationChannelTypeId)
        {
            return ServiceResult.Conflict("Bu tür kodu zaten kullanılıyor.");
        }

        entity.Name = request.Name.Trim();
        entity.Code = request.Code.Trim();
        entity.IconUrl = NormalizeOptional(request.IconUrl);
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
            return ServiceResult.NotFound("İletişim kanalı türü bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CommunicationChannelTypeDto ToDto(CommunicationChannelTypes entity) => new()
    {
        CommunicationChannelTypeId = entity.CommunicationChannelTypeId,
        Name = entity.Name,
        Code = entity.Code,
        IconUrl = entity.IconUrl,
        IsActive = entity.IsActive,
        SortOrder = entity.SortOrder,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
