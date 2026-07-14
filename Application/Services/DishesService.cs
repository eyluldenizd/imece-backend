using Application.DTOs;
using Application.Exceptions;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class DishesService
{
    private readonly DishesRepository _dishesRepository;

    public DishesService(DishesRepository dishesRepository)
    {
        _dishesRepository = dishesRepository;
    }

    public async Task<List<DishesDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var dishes = await _dishesRepository.GetAllAsync(cancellationToken);

        return dishes
            .Select(ToDto)
            .ToList();
    }

    public async Task<List<DishesDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var dishes = await _dishesRepository.GetAllAsync(cancellationToken);

        return dishes
            .Select(ToDto)
            .ToList();
    }

    public async Task<DishesDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var dish = await _dishesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Dishes), id);

        return ToDto(dish);
    }

    //public async Task<int> CreateAsync(
    //    DishesDto dto,
    //    CancellationToken cancellationToken = default)
    //{
    //    DishesValidator.ValidateCreate(dto);

    //    var dish = new Dishes
    //    {
    //        DishName = dto.DishName.Trim(),
    //        Category = dto.Category.Trim(),
    //        IsActive = dto.IsActive
    //    };

    //    var dish = await _dishesRepository.GetAllAsync(
    //        cancellationToken);

    //    retq
    //}

    //public async Task UpdateAsync(
    //    DishesDto dto,
    //    CancellationToken cancellationToken = default)
    //{
    //    DishesValidator.ValidateUpdate(dto);

    //    var existingDish = await _dishesRepository.GetByIdAsync(
    //        dto.DishId,
    //        cancellationToken)
    //        ?? throw new NotFoundException(nameof(Dishes), dto.DishId);

    //    existingDish.DishName = dto.DishName.Trim();
    //    existingDish.Category = dto.Category.Trim();
    //    existingDish.IsActive = dto.IsActive;

    //    var rowsAffected = await _dishesRepository.UpdateAsync(
    //        existingDish,
    //        cancellationToken);

    //    if (rowsAffected == 0)
    //    {
    //        throw new NotFoundException(
    //            nameof(Dishes),
    //            dto.DishId);
    //    }
    //}

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Yemek kimliği sıfırdan büyük olmalıdır.",
                nameof(id));
        }

        var rowsAffected = await _dishesRepository.DeleteAsync(
            id,
            cancellationToken);

        if (rowsAffected == 0)
        {
            throw new NotFoundException(nameof(Dishes), id);
        }
    }

    private static DishesDto ToDto(Dishes dish)
    {
        return new DishesDto
        {
            DishId = dish.DishId,
            DishName = dish.DishName,
            Category = dish.Category,
            IsActive = dish.IsActive,
            CreatedAt = dish.CreatedAt
        };
    }
}