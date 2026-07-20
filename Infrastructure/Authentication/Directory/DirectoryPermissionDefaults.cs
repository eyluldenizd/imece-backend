using Core.Authorization;

namespace Infrastructure.Authentication.Directory;

/// <summary>
/// Yalnızca veritabanından izin gelmediğinde kullanılan yedek eşleme.
/// SqlDirectoryUserService birincil kaynak olarak role_permissions kullanır.
/// </summary>
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
