using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class CorporateAppCategoryService
{
    private readonly CorporateAppCategoryRepository _repository;

    public CorporateAppCategoryService(CorporateAppCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<CorporateAppCategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<CorporateAppCategoryDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<CorporateAppCategoryDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<CorporateAppCategoryDto>.NotFound("Kurumsal uygulama kategorisi bulunamadı.");
        }

        return ServiceResult<CorporateAppCategoryDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateCorporateAppCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var existing = await _repository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            return ServiceResult<int>.Conflict("Bu kategori adı zaten kullanılıyor.");
        }

        var entity = new CorporateAppCategories
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
        UpdateCorporateAppCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.CorporateAppCategoryId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Kurumsal uygulama kategorisi bulunamadı.");
        }

        var name = request.Name.Trim();
        var existing = await _repository.GetByNameAsync(name, cancellationToken);
        if (existing is not null && existing.CorporateAppCategoryId != request.CorporateAppCategoryId)
        {
            return ServiceResult.Conflict("Bu kategori adı zaten kullanılıyor.");
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
            return ServiceResult.NotFound("Kurumsal uygulama kategorisi bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CorporateAppCategoryDto ToDto(CorporateAppCategories entity) => new()
    {
        CorporateAppCategoryId = entity.CorporateAppCategoryId,
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
