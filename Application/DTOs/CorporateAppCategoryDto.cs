using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CorporateAppCategoryDto
{
    public int CorporateAppCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ColorKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCorporateAppCategoryDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori adı en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "İkon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Renk anahtarı en fazla 32 karakter olabilir.")]
    public string? ColorKey { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCorporateAppCategoryDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir kategori ID değeri gönderilmelidir.")]
    public int CorporateAppCategoryId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori adı en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "İkon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Renk anahtarı en fazla 32 karakter olabilir.")]
    public string? ColorKey { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
