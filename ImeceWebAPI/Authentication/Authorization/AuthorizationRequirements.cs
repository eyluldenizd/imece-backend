using Microsoft.AspNetCore.Authorization;

namespace ImeceWebAPI.Authentication.Authorization;

/// <summary>Uygulamaya kayıtlı ve etkin kullanıcı gerektirir.</summary>
public sealed class RegisteredUserRequirement : IAuthorizationRequirement;

/// <summary>Bir şirkete bağlı (ve kayıtlı/etkin) kullanıcı gerektirir.</summary>
public sealed class CompanyRequirement : IAuthorizationRequirement;

/// <summary>Belirtilen rollerden en az birine sahip kullanıcı gerektirir.</summary>
public sealed class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }

    public IReadOnlyCollection<string> AllowedRoles { get; }
}

/// <summary>Belirtilen izinlerden en az birine sahip kullanıcı gerektirir.</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(params string[] permissions)
    {
        if (permissions is null || permissions.Length == 0)
        {
            throw new ArgumentException("En az bir permission gereklidir.", nameof(permissions));
        }

        Permissions = permissions;
    }

    public IReadOnlyCollection<string> Permissions { get; }

    /// <summary>Geriye uyumluluk: ilk permission.</summary>
    public string Permission => Permissions.First();
}

