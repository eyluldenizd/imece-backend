using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class DishesService
{
    private readonly DishesRepository _dishesRepository;
    private readonly DishCategoryRepository _dishCategoryRepository;

    public DishesService(
        DishesRepository dishesRepository,
        DishCategoryRepository dishCategoryRepository)
    {
        _dishesRepository = dishesRepository;
        _dishCategoryRepository = dishCategoryRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<DishesDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var dishes = await _dishesRepository.GetAllAsync(cancellationToken);
        var categories = await _dishCategoryRepository.GetAllAsync(cancellationToken);
        var categoryMap = categories.ToDictionary(category => category.DishCategoryId);

        return ServiceResult<IReadOnlyList<DishesDto>>.Success(
            dishes.Select(dish => ToDto(dish, categoryMap)).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<DishesDto>>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var dishes = await _dishesRepository.GetActiveAsync(cancellationToken);
        var categories = await _dishCategoryRepository.GetActiveAsync(cancellationToken);
        var categoryMap = categories.ToDictionary(category => category.DishCategoryId);

        return ServiceResult<IReadOnlyList<DishesDto>>.Success(
            dishes.Select(dish => ToDto(dish, categoryMap)).ToList());
    }

    public async Task<ServiceResult<DishesDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var dish = await _dishesRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (dish is null)
        {
            return ServiceResult<DishesDto>.NotFound("Yemek bulunamadı.");
        }

        var category = dish.DishCategoryId.HasValue
            ? await _dishCategoryRepository.GetByIdAsync(dish.DishCategoryId.Value, cancellationToken)
            : null;

        return ServiceResult<DishesDto>.Success(ToDto(dish, category));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateDishesDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dishCategoryRepository.GetByIdAsync(request.DishCategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            return ServiceResult<int>.BadRequest("Geçersiz yemek kategorisi.");
        }

        var entity = new Dishes
        {
            DishName = request.DishName.Trim(),
            DishCategoryId = request.DishCategoryId,
            Category = category.Name,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = request.IsActive
        };

        var id = await _dishesRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateDishesDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dishesRepository.GetByIdAsync(request.DishId, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound("Yemek bulunamadı.");
        }

        var category = await _dishCategoryRepository.GetByIdAsync(request.DishCategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            return ServiceResult.BadRequest("Geçersiz yemek kategorisi.");
        }

        entity.DishName = request.DishName.Trim();
        entity.DishCategoryId = request.DishCategoryId;
        entity.Category = category.Name;
        entity.Description = request.Description?.Trim();
        entity.ImageUrl = request.ImageUrl?.Trim();
        entity.IsActive = request.IsActive;

        await _dishesRepository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dishesRepository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound("Yemek bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static DishesDto ToDto(
        Dishes dish,
        IReadOnlyDictionary<int, DishCategories> categoryMap)
    {
        categoryMap.TryGetValue(dish.DishCategoryId ?? 0, out var category);
        return ToDto(dish, category);
    }

    private static DishesDto ToDto(Dishes dish, DishCategories? category) => new()
    {
        DishId = dish.DishId,
        DishName = dish.DishName,
        DishCategoryId = dish.DishCategoryId,
        DishCategoryName = category?.Name,
        Category = dish.Category,
        Description = dish.Description,
        ImageUrl = dish.ImageUrl,
        IsActive = dish.IsActive,
        CreatedAt = dish.CreatedAt,
        UpdatedAt = dish.UpdatedAt
    };
}
