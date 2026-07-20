using Core.Common.Validation;

namespace Application.DTOs;

public sealed class BranchDto
{
    public int BranchId { get; set; }

    public int? CompanyId { get; set; }

    public string BranchCode { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateBranchDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Şirket ID zorunludur.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Şube kodu en fazla 64 karakter olabilir.")]
    public string? BranchCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Şube adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Şube adı en fazla 256 karakter olabilir.")]
    public string BranchName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateBranchDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şube ID değeri gönderilmelidir.")]
    public int BranchId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Şirket ID zorunludur.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Şube kodu en fazla 64 karakter olabilir.")]
    public string? BranchCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Şube adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Şube adı en fazla 256 karakter olabilir.")]
    public string BranchName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }
}
