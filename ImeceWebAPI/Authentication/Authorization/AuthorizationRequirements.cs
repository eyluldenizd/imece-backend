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

/// <summary>Belirtilen izne sahip kullanıcı gerektirir (feature-permission policy).</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}

/// <summary>Şirket yöneticisi veya global içerik yöneticisi izni gerektirir.</summary>
public sealed class CompanyAdminOrGlobalContentManagerRequirement : IAuthorizationRequirement;
