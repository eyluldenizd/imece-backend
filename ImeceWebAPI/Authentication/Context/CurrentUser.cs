using Application.Exceptions;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// <see cref="ICurrentUser"/>'ın scoped implementasyonu. Yalnızca çözümlenmiş
/// <see cref="ApplicationUser"/>'ı okur; hiçbir provider tipine bağlı değildir.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly ImeceUserContext _context;

    public CurrentUser(ImeceUserContext context)
    {
        _context = context;
    }

    private ApplicationUser? User => _context.User;

    public bool IsAuthenticated => User is not null;

    public bool IsRegistered => User?.IsRegistered ?? false;

    public bool IsActive => User?.IsActive ?? false;

    public int? UserId => User?.UserId;

    public string? ExternalId => User?.Identity.ExternalId;

    public string? Username => User?.Identity.Username;

    public string? Email => User?.Identity.Email;

    public string? DisplayName => User?.Identity.DisplayName;

    public string? IdentityProvider => User?.Identity.IdentityProvider;

    public IReadOnlyCollection<string> Roles => User?.Roles ?? [];

    public IReadOnlyCollection<string> Permissions => User?.Permissions ?? [];

    public IReadOnlyCollection<CompanyMembership> CompanyMemberships =>
        User?.CompanyMemberships ?? [];

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    public int GetRequiredUserId() =>
        UserId
        ?? throw new ForbiddenException(
            "Bu işlem için uygulamaya kayıtlı bir kullanıcı kimliği gereklidir.");
}
