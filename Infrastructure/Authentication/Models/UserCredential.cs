namespace Infrastructure.Authentication.Models;

public sealed class UserCredential
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string AzureObjectId { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutEnd { get; set; }
}
