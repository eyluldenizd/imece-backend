using Core.Common.Validation;

namespace Application.DTOs;

public sealed class DishesDto
{
    public int DishId { get; set; }

    public string DishName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class CreateDishesDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Yemek adı boş bırakılamaz.")]
    [Validate(
        ValidationRuleType.MaxLength,
        150,
        ErrorMessage = "Yemek adı en fazla 150 karakter olabilir.")]
    public string DishName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Kategori boş bırakılamaz.")]
    [Validate(
        ValidationRuleType.MaxLength,
        100,
        ErrorMessage = "Kategori en fazla 100 karakter olabilir.")]
    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDishesDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Yemek kimliği sıfırdan büyük olmalıdır.")]
    public int DishId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Yemek adı boş bırakılamaz.")]
    [Validate(
        ValidationRuleType.MaxLength,
        150,
        ErrorMessage = "Yemek adı en fazla 150 karakter olabilir.")]
    public string DishName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Kategori boş bırakılamaz.")]
    [Validate(
        ValidationRuleType.MaxLength,
        100,
        ErrorMessage = "Kategori en fazla 100 karakter olabilir.")]
    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}