using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CorporateAppDto : OrganizationScopeFieldsDto
{
    public long AppId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public int? CorporateAppCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Category { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateCorporateAppDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Başlık zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Başlık en fazla 256 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "URL zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "URL en fazla 1024 karakter olabilir.")]
    public string Url { get; set; } = string.Empty;

    public int? CorporateAppCategoryId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori en fazla 128 karakter olabilir.")]
    public string? Category { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "İkon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCorporateAppDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir uygulama ID değeri gönderilmelidir.")]
    public long AppId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Başlık zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Başlık en fazla 256 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "URL zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "URL en fazla 1024 karakter olabilir.")]
    public string Url { get; set; } = string.Empty;

    public int? CorporateAppCategoryId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori en fazla 128 karakter olabilir.")]
    public string? Category { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "İkon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    public bool IsActive { get; set; }
}
