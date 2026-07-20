using Core.Common.Validation;

namespace Application.DTOs;

public sealed class ReservationDto
{
    public long ReservationId { get; set; }
    public int? CompanyId { get; set; }
    public int? MeetingRoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int? OrganizerUserId { get; set; }
    public int? RequesterUserId { get; set; }
    public string? RequesterName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateReservationDto
{
    public int? CompanyId { get; set; }
    public int? MeetingRoomId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Oda adı en fazla 128 karakter olabilir.")]
    public string? RoomName { get; set; }

    public int? OrganizerUserId { get; set; }
    public int? RequesterUserId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Talep eden adı en fazla 256 karakter olabilir.")]
    public string? RequesterName { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Rezervasyon başlığı zorunludur.")]
    [Validate(ValidationRuleType.MinLength, 3, ErrorMessage = "Rezervasyon başlığı en az 3 karakter olmalıdır.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Rezervasyon başlığı en fazla 256 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Başlangıç zamanı zorunludur.")]
    public DateTime StartTime { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Bitiş zamanı zorunludur.")]
    public DateTime EndTime { get; set; }
}

public sealed class UpdateReservationDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir rezervasyon ID değeri gönderilmelidir.")]
    public long ReservationId { get; set; }

    public int? CompanyId { get; set; }
    public int? MeetingRoomId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Oda adı en fazla 128 karakter olabilir.")]
    public string? RoomName { get; set; }

    public int? OrganizerUserId { get; set; }
    public int? RequesterUserId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Talep eden adı en fazla 256 karakter olabilir.")]
    public string? RequesterName { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Rezervasyon başlığı zorunludur.")]
    [Validate(ValidationRuleType.MinLength, 3, ErrorMessage = "Rezervasyon başlığı en az 3 karakter olmalıdır.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Rezervasyon başlığı en fazla 256 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Başlangıç zamanı zorunludur.")]
    public DateTime StartTime { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Bitiş zamanı zorunludur.")]
    public DateTime EndTime { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Rezervasyon durumu zorunludur.")]
    public string Status { get; set; } = string.Empty;
}
