using Core.Common.Validation;

namespace Application.DTOs;

public sealed class EventDto
{
    public long EventId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? EventType { get; set; }

    public string? Location { get; set; }

    public string? CoverImageUrl { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public bool IsAllDay { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class CreateEventDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Etkinlik başlığı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        3,
        ErrorMessage = "Etkinlik başlığı en az 3 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Etkinlik başlığı en fazla 255 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        50,
        ErrorMessage = "Etkinlik tipi en fazla 50 karakter olabilir.")]
    public string? EventType { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Konum en fazla 255 karakter olabilir.")]
    public string? Location { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Kapak görseli adresi en fazla 255 karakter olabilir.")]
    public string? CoverImageUrl { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public bool IsAllDay { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir oluşturan kullanıcı ID değeri gönderilmelidir.")]
    public int CreatedBy { get; set; }
}

public sealed class UpdateEventDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir etkinlik ID değeri gönderilmelidir.")]
    public long EventId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Etkinlik başlığı zorunludur.")]
    [Validate(
        ValidationRuleType.MinLength,
        3,
        ErrorMessage = "Etkinlik başlığı en az 3 karakter olmalıdır.")]
    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Etkinlik başlığı en fazla 255 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        50,
        ErrorMessage = "Etkinlik tipi en fazla 50 karakter olabilir.")]
    public string? EventType { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Konum en fazla 255 karakter olabilir.")]
    public string? Location { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        255,
        ErrorMessage = "Kapak görseli adresi en fazla 255 karakter olabilir.")]
    public string? CoverImageUrl { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public bool IsAllDay { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir oluşturan kullanıcı ID değeri gönderilmelidir.")]
    public int CreatedBy { get; set; }
}