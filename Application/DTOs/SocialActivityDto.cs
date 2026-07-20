using Core.Common.Validation;

namespace Application.DTOs;

public sealed class SocialActivityDto : OrganizationScopeFieldsDto
{
    public long SocialActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;
}

public sealed class CreateSocialActivityDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Başlık zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Başlık en fazla 512 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Aktivite türü zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Aktivite türü en fazla 64 karakter olabilir.")]
    public string ActivityType { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum en fazla 256 karakter olabilir.")]
    public string? Location { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Görsel URL en fazla 1024 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    public string Status { get; set; } = "Draft";
}

public sealed class UpdateSocialActivityDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir aktivite ID değeri gönderilmelidir.")]
    public long SocialActivityId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Başlık zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Başlık en fazla 512 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Aktivite türü zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Aktivite türü en fazla 64 karakter olabilir.")]
    public string ActivityType { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum en fazla 256 karakter olabilir.")]
    public string? Location { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Görsel URL en fazla 1024 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    public string Status { get; set; } = "Draft";
    public bool IsActive { get; set; }
}
