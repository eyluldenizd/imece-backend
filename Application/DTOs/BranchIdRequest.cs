using Core.Common.Validation;

namespace Application.DTOs;

public sealed class BranchIdRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şube ID değeri gönderilmelidir.")]
    public int BranchId { get; set; }
}
