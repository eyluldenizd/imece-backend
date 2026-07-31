using Core.Common.Validation;

namespace Application.DTOs;

/// <summary>
/// Oturum açmış kullanıcının zenginleştirilmiş profil özeti.
/// Auth /me context'inden ayrıdır; users tablosu + org join'lerinden gelir.
/// </summary>
public sealed class CurrentUserProfileDto
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Phone { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsActive { get; set; }

    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public int? BranchId { get; set; }

    public string? BranchName { get; set; }

    public int? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateOnly? HireDate { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? PasswordChangedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyCollection<string> Roles { get; set; } = [];

    public IReadOnlyCollection<string> Permissions { get; set; } = [];

    public IReadOnlyCollection<CurrentUserCompanyResponse> Companies { get; set; } = [];
}

public sealed class ChangeMyPasswordDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Mevcut şifre zorunludur.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Yeni şifre zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        12,
        ErrorMessage = "Yeni şifre en az 12 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        128,
        ErrorMessage = "Yeni şifre en fazla 128 karakter olabilir.")]
    public string NewPassword { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Yeni şifre tekrarı zorunludur.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
