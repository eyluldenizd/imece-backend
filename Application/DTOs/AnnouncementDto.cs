using Core.Common.Validation;

namespace Application.DTOs;

public sealed class AnnouncementDto
{
    public long AnnouncementId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public int AuthorUserId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime PublishStart { get; set; }

    public DateTime? PublishEnd { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateAnnouncementDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Duyuru başlığı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        3,
        ErrorMessage = "Duyuru başlığı en az 3 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        200,
        ErrorMessage = "Duyuru başlığı en fazla 200 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Duyuru içeriği zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        4000,
        ErrorMessage = "Duyuru içeriği en fazla 4000 karakter olabilir.")]
    public string Content { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        500,
        ErrorMessage = "Kapak görseli adresi en fazla 500 karakter olabilir.")]
    public string? CoverImageUrl { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir yazar kullanıcı ID değeri gönderilmelidir.")]
    public int AuthorUserId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime PublishStart { get; set; }

    public DateTime? PublishEnd { get; set; }
}

public sealed class UpdateAnnouncementDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir duyuru ID değeri gönderilmelidir.")]
    public long AnnouncementId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Duyuru başlığı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        3,
        ErrorMessage = "Duyuru başlığı en az 3 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        200,
        ErrorMessage = "Duyuru başlığı en fazla 200 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Duyuru içeriği zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        4000,
        ErrorMessage = "Duyuru içeriği en fazla 4000 karakter olabilir.")]
    public string Content { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        500,
        ErrorMessage = "Kapak görseli adresi en fazla 500 karakter olabilir.")]
    public string? CoverImageUrl { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir yazar kullanıcı ID değeri gönderilmelidir.")]
    public int AuthorUserId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime PublishStart { get; set; }

    public DateTime? PublishEnd { get; set; }
}