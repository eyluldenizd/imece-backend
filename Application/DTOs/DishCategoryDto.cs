using Core.Common.Validation;

namespace Application.DTOs;

public sealed class DishCategoryDto
{
    public int DishCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateDishCategoryDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori adı en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kategori kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDishCategoryDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Kategori kimliği sıfırdan büyük olmalıdır.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori adı en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kategori kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
