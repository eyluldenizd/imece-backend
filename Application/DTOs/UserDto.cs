using Core.Common.Validation;

namespace Application.DTOs;

public sealed class UserDto
{
    public int UserId { get; set; }

    public string AzureObjectId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Title { get; set; }

    public int? DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public int RoleId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? Phone { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateUserDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Kullanıcı adı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        128,
        ErrorMessage = "Kullanıcı adı en fazla 128 karakter olabilir.")]
    public string Username { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        128,
        ErrorMessage = "Azure nesne ID en fazla 128 karakter olabilir.")]
    public string? AzureObjectId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "E-posta alanı zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "E-posta en fazla 255 karakter olabilir.")]
    public string Email { get; set; } = string.Empty;

    public string? TemporaryPassword { get; set; }

    /// <summary>
    /// true ise password_changed_at null bırakılır (ilk girişte şifre değiştirme zorunlu).
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Şirket ID zorunludur.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Ad soyad alanı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        3,
        ErrorMessage = "Ad soyad en az 3 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Ad soyad en fazla 255 karakter olabilir.")]
    public string FullName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Unvan en fazla 255 karakter olabilir.")]
    public string? Title { get; set; }

    public int? DepartmentId { get; set; }

    public int? BranchId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir rol ID değeri gönderilmelidir.")]
    public int RoleId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateOnly? HireDate { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        30,
        ErrorMessage = "Telefon numarası en fazla 30 karakter olabilir.")]
    public string? Phone { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        500,
        ErrorMessage = "Fotoğraf adresi en fazla 500 karakter olabilir.")]
    public string? PhotoUrl { get; set; }
}

public sealed class UpdateUserDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir kullanıcı ID değeri gönderilmelidir.")]
    public int UserId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Ad soyad alanı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        3,
        ErrorMessage = "Ad soyad en az 3 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Ad soyad en fazla 255 karakter olabilir.")]
    public string FullName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Unvan en fazla 255 karakter olabilir.")]
    public string? Title { get; set; }

    public int? DepartmentId { get; set; }

    public int? BranchId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir rol ID değeri gönderilmelidir.")]
    public int RoleId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateOnly? HireDate { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        30,
        ErrorMessage = "Telefon numarası en fazla 30 karakter olabilir.")]
    public string? Phone { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        500,
        ErrorMessage = "Fotoğraf adresi en fazla 500 karakter olabilir.")]
    public string? PhotoUrl { get; set; }

    [Validate(
        ValidationRuleType.MinLength,
        12,
        ErrorMessage = "Yeni şifre en az 12 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        128,
        ErrorMessage = "Yeni şifre en fazla 128 karakter olabilir.")]
    public string? NewPassword { get; set; }

    public bool IsActive { get; set; }
}
