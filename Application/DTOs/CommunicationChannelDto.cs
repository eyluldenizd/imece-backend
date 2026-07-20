using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CommunicationChannelDto : OrganizationScopeFieldsDto
{
    public long ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? CommunicationChannelTypeId { get; set; }
    public string? TypeIconUrl { get; set; }
    public string AddressUrl { get; set; } = string.Empty;
    public string? DepartmentInCharge { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCommunicationChannelDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Kanal adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Kanal adı en fazla 256 karakter olabilir.")]
    public string ChannelName { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kanal türü en fazla 64 karakter olabilir.")]
    public string? Type { get; set; }

    public int? CommunicationChannelTypeId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Adres URL zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Adres URL en fazla 1024 karakter olabilir.")]
    public string AddressUrl { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Sorumlu departman en fazla 256 karakter olabilir.")]
    public string? DepartmentInCharge { get; set; }

    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "İkon en fazla 256 karakter olabilir.")]
    public string? Icon { get; set; }

    public int SortOrder { get; set; }
}

public sealed class UpdateCommunicationChannelDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir kanal ID değeri gönderilmelidir.")]
    public long ChannelId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Kanal adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Kanal adı en fazla 256 karakter olabilir.")]
    public string ChannelName { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kanal türü en fazla 64 karakter olabilir.")]
    public string? Type { get; set; }

    public int? CommunicationChannelTypeId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Adres URL zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "Adres URL en fazla 1024 karakter olabilir.")]
    public string AddressUrl { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Sorumlu departman en fazla 256 karakter olabilir.")]
    public string? DepartmentInCharge { get; set; }

    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "İkon en fazla 256 karakter olabilir.")]
    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
