using Core.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password) =>
        _hasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password)
        == PasswordVerificationResult.Success;
}
