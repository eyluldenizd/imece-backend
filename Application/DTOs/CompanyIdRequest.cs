using Core.Common.Validation;

namespace Application.DTOs;

public sealed class CompanyIdRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şirket ID değeri gönderilmelidir.")]
    public int CompanyId { get; set; }
}
