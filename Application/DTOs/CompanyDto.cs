using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CompanyDto
{
    public int CompanyId { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCompanyDto
{
    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Şirket kodu en fazla 64 karakter olabilir.")]
    public string? CompanyCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Şirket adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Şirket adı en fazla 256 karakter olabilir.")]
    public string CompanyName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "Açıklama en fazla 1024 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "Logo adresi en fazla 1024 karakter olabilir.")]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCompanyDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Şirket kodu en fazla 64 karakter olabilir.")]
    public string? CompanyCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Şirket adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Şirket adı en fazla 256 karakter olabilir.")]
    public string CompanyName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "Açıklama en fazla 1024 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "Logo adresi en fazla 1024 karakter olabilir.")]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; }
}
