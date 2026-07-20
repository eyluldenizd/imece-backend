using Application.Common.Codes;
using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class DishCategoryService
{
    private readonly DishCategoryRepository _dishCategoryRepository;
    private readonly IEntityCodeGenerator _codeGenerator;

    public DishCategoryService(
        DishCategoryRepository dishCategoryRepository,
        IEntityCodeGenerator codeGenerator)
    {
        _dishCategoryRepository = dishCategoryRepository;
        _codeGenerator = codeGenerator;
    }

    public async Task<ServiceResult<IReadOnlyList<DishCategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _dishCategoryRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<DishCategoryDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<DishCategoryDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dishCategoryRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<DishCategoryDto>.NotFound("Yemek kategorisi bulunamadı.");
        }

        return ServiceResult<DishCategoryDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateDishCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var code = await _codeGenerator.AllocateAsync(
            request.Code,
            request.Name,
            "CAT",
            async (candidate, ct) =>
            {
                var existing = await _dishCategoryRepository.GetByCodeAsync(candidate, ct);
                return existing is not null;
            },
            maxLength: 64,
            cancellationToken);

        var entity = new DishCategories
        {
            Name = request.Name.Trim(),
            Code = code,
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        var id = await _dishCategoryRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateDishCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dishCategoryRepository.GetByIdAsync(request.DishCategoryId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Yemek kategorisi bulunamadı.");
        }

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? entity.Code
            : await _codeGenerator.AllocateAsync(
                request.Code,
                request.Name,
                "CAT",
                async (candidate, ct) =>
                {
                    var existing = await _dishCategoryRepository.GetByCodeAsync(candidate, ct);
                    return existing is not null && existing.DishCategoryId != request.DishCategoryId;
                },
                maxLength: 64,
                cancellationToken);

        entity.Name = request.Name.Trim();
        entity.Code = code;
        entity.Description = request.Description?.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _dishCategoryRepository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dishCategoryRepository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Yemek kategorisi bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static DishCategoryDto ToDto(DishCategories entity) => new()
    {
        DishCategoryId = entity.DishCategoryId,
        Name = entity.Name,
        Code = entity.Code,
        Description = entity.Description,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
