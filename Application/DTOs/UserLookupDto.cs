using Core.Common.Validation;

namespace Application.DTOs;

public sealed class UserLookupDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
