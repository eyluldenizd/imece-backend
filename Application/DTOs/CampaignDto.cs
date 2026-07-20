using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CampaignDto : OrganizationScopeFieldsDto
{
    public long CampaignId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? TargetUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
}

public sealed class CreateCampaignDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Kampanya başlığı zorunludur.")]
    [Validate(ValidationRuleType.MinLength, 3, ErrorMessage = "Kampanya başlığı en az 3 karakter olmalıdır.")]
    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Kampanya başlığı en fazla 255 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Görsel adresi en fazla 255 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Hedef link adresi en fazla 255 karakter olabilir.")]
    public string? TargetUrl { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class UpdateCampaignDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir kampanya ID değeri gönderilmelidir.")]
    public long CampaignId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Kampanya başlığı zorunludur.")]
    [Validate(ValidationRuleType.MinLength, 3, ErrorMessage = "Kampanya başlığı en az 3 karakter olmalıdır.")]
    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Kampanya başlığı en fazla 255 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Görsel adresi en fazla 255 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Hedef link adresi en fazla 255 karakter olabilir.")]
    public string? TargetUrl { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
