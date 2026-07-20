using Core.Authentication;
using Core.Authorization;
using Core.Directory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication.Directory;

/// <summary>
/// Development ortamı için yapılandırma tabanlı dizin. Profiller
/// <see cref="DirectoryOptions.Profiles"/> içinde ExternalId ile anahtarlanır.
/// </summary>
public sealed class DevelopmentDirectoryUserService : IDirectoryUserService
{
    private readonly DirectoryOptions _options;

    public DevelopmentDirectoryUserService(IOptions<DirectoryOptions> options)
    {
        _options = options.Value;
    }

    public Task<ApplicationUser?> FindByExternalIdentityAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Profiles.TryGetValue(identity.ExternalId, out var profile))
        {
            return Task.FromResult<ApplicationUser?>(null);
        }

        var roles = (IReadOnlyCollection<string>)profile.Roles.ToArray();
        var permissions = DirectoryPermissionDefaults.Apply(roles, profile.Permissions);

        return Task.FromResult<ApplicationUser?>(new ApplicationUser
        {
            Identity = identity,
            UserId = profile.UserId,
            IsActive = profile.IsActive,
            CompanyId = profile.CompanyId,
            CompanyName = profile.CompanyName,
            Roles = roles,
            Permissions = permissions,
            CompanyMemberships = BuildMemberships(profile)
        });
    }

    private static IReadOnlyCollection<CompanyMembership> BuildMemberships(
        DirectoryProfileOptions profile)
    {
        if (profile.CompanyRoles.Count > 0)
        {
            return profile.CompanyRoles
                .Select(role => new CompanyMembership
                {
                    CompanyId = role.CompanyId,
                    CompanyName = role.CompanyName,
                    Roles = role.Roles.ToArray(),
                    Permissions = DirectoryPermissionDefaults.Apply(
                        role.Roles,
                        role.Permissions)
                })
                .ToArray();
        }

        if (profile.CompanyId is int companyId)
        {
            return
            [
                new CompanyMembership
                {
                    CompanyId = companyId,
                    CompanyName = profile.CompanyName,
                    Roles = profile.Roles.ToArray(),
                    Permissions = DirectoryPermissionDefaults.Apply(
                        profile.Roles,
                        profile.Permissions)
                }
            ];
        }

        return [];
    }
}

internal static class DirectoryPermissionDefaults
{
    public static IReadOnlyCollection<string> Apply(
        IEnumerable<string> roles,
        IEnumerable<string> explicitPermissions)
    {
        var permissions = new HashSet<string>(
            explicitPermissions,
            StringComparer.OrdinalIgnoreCase);

        var roleSet = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(Roles.GlobalAdmin) || roleSet.Contains(Roles.CompanyAdmin))
        {
            permissions.Add(Permissions.ContentCompanyManage);
        }

        if (roleSet.Contains(Roles.GlobalAdmin))
        {
            permissions.Add(Permissions.ContentGlobalManage);
        }

        return permissions.ToArray();
    }
}
