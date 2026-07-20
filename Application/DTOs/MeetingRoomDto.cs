using Core.Common.Validation;

namespace Application.DTOs;

public sealed class MeetingRoomDto
{
    public int MeetingRoomId { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public int Capacity { get; set; }
    public string? LocationDescription { get; set; }
    public string? Features { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateMeetingRoomDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }

    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Oda adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Oda adı en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Oda kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kat bilgisi en fazla 64 karakter olabilir.")]
    public string? Floor { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Kapasite en az 1 olmalıdır.")]
    public int Capacity { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Konum açıklaması en fazla 512 karakter olabilir.")]
    public string? LocationDescription { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Özellikler en fazla 512 karakter olabilir.")]
    public string? Features { get; set; }
}

public sealed class UpdateMeetingRoomDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir oda ID değeri gönderilmelidir.")]
    public int MeetingRoomId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }

    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Oda adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Oda adı en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Oda kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kat bilgisi en fazla 64 karakter olabilir.")]
    public string? Floor { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Kapasite en az 1 olmalıdır.")]
    public int Capacity { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Konum açıklaması en fazla 512 karakter olabilir.")]
    public string? LocationDescription { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Özellikler en fazla 512 karakter olabilir.")]
    public string? Features { get; set; }

    public bool IsActive { get; set; }
}
