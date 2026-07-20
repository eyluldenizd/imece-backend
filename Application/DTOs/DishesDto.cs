using Core.Common.Validation;

namespace Application.DTOs;

public sealed class DishesDto
{
    public int DishId { get; set; }

    public string DishName { get; set; } = string.Empty;

    public int? DishCategoryId { get; set; }

    public string? DishCategoryName { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateDishesDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Yemek adı boş bırakılamaz.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Yemek adı en fazla 256 karakter olabilir.")]
    public string DishName { get; set; } = string.Empty;

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kategorisi seçilmelidir.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Açıklama en fazla 1024 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Görsel URL en fazla 1024 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDishesDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kimliği sıfırdan büyük olmalıdır.")]
    public int DishId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Yemek adı boş bırakılamaz.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Yemek adı en fazla 256 karakter olabilir.")]
    public string DishName { get; set; } = string.Empty;

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kategorisi seçilmelidir.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Açıklama en fazla 1024 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Görsel URL en fazla 1024 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}
