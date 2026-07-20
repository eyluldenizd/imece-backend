using Core.Common.Validation;

namespace Application.DTOs;

public sealed class DepartmentDto
{
    public int DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public int? CompanyId { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? DepartmentCode { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateDepartmentDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Şube ID zorunludur.")]
    public int BranchId { get; set; }

    public int? ParentDepartmentId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Departman kodu en fazla 64 karakter olabilir.")]
    public string? DepartmentCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Departman adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Departman adı en fazla 256 karakter olabilir.")]
    public string DepartmentName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDepartmentDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir departman ID değeri gönderilmelidir.")]
    public int DepartmentId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Şube ID zorunludur.")]
    public int BranchId { get; set; }

    public int? ParentDepartmentId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Departman kodu en fazla 64 karakter olabilir.")]
    public string? DepartmentCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Departman adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Departman adı en fazla 256 karakter olabilir.")]
    public string DepartmentName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Açıklama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
