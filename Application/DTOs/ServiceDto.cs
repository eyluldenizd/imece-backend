using Core.Common.Validation;

namespace Application.DTOs;

public sealed class ServiceDto
{
    public long ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateServiceDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Hizmet adı zorunludur.")]
    [Validate(ValidationRuleType.MinLength, 3, ErrorMessage = "Hizmet adı en az 3 karakter olmalıdır.")]
    [Validate(ValidationRuleType.MaxLength, 255, ErrorMessage = "Hizmet adı en fazla 255 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 100, ErrorMessage = "Simge adı en fazla 100 karakter olabilir.")]
    public string? Icon { get; set; }
}

public sealed class UpdateServiceDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Geçerli bir hizmet ID değeri gönderilmelidir.")]
    public long ServiceId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Hizmet adı zorunludur.")]
    [Validate(ValidationRuleType.MinLength, 3, ErrorMessage = "Hizmet adı en az 3 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
}
