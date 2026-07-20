using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CommunicationChannelTypeDto
{
    public int CommunicationChannelTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCommunicationChannelTypeDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Tür adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Tür adı en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.Required, ErrorMessage = "Tür kodu zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Tür kodu en fazla 64 karakter olabilir.")]
    public string Code { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "İkon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCommunicationChannelTypeDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir tür ID değeri gönderilmelidir.")]
    public int CommunicationChannelTypeId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Tür adı zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Tür adı en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.Required, ErrorMessage = "Tür kodu zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Tür kodu en fazla 64 karakter olabilir.")]
    public string Code { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "İkon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
