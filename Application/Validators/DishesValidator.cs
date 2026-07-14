using Application.DTOs;

namespace Application.Validators;

public static class DishesValidator
{
    private const int DishNameMaxLength = 150;
    private const int CategoryMaxLength = 100;

    public static void ValidateCreate(DishesDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidateDishName(dto.DishName);
        ValidateCategory(dto.Category);
    }

    public static void ValidateUpdate(DishesDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.DishId <= 0)
        {
            throw new ArgumentException(
                "Yemek kimliği sıfırdan büyük olmalıdır.",
                nameof(dto.DishId));
        }

        ValidateDishName(dto.DishName);
        ValidateCategory(dto.Category);
    }

    private static void ValidateDishName(string dishName)
    {
        if (string.IsNullOrWhiteSpace(dishName))
        {
            throw new ArgumentException(
                "Yemek adı boş bırakılamaz.",
                nameof(dishName));
        }

        if (dishName.Trim().Length > DishNameMaxLength)
        {
            throw new ArgumentException(
                $"Yemek adı en fazla {DishNameMaxLength} karakter olabilir.",
                nameof(dishName));
        }
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException(
                "Kategori boş bırakılamaz.",
                nameof(category));
        }

        if (category.Trim().Length > CategoryMaxLength)
        {
            throw new ArgumentException(
                $"Kategori en fazla {CategoryMaxLength} karakter olabilir.",
                nameof(category));
        }
    }
}