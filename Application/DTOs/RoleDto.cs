using Core.Common.Validation;

namespace Application.DTOs;

public sealed class RoleDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyList<string> PermissionCodes { get; set; } = [];
}

public sealed class RoleListItemDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed class CreateRoleDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Rol adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Rol adı en fazla 64 karakter olabilir.")]
    public string RoleName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Açıklama en fazla 256 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRoleDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir rol ID değeri gönderilmelidir.")]
    public int RoleId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Rol adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Rol adı en fazla 64 karakter olabilir.")]
    public string RoleName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Açıklama en fazla 256 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed class UpdateRolePermissionsDto
{
    public int[] PermissionIds { get; set; } = [];
}

public sealed class PermissionDto
{
    public int PermissionId { get; set; }

    public string PermissionCode { get; set; } = string.Empty;

    public string? Description { get; set; }
}
