namespace Core.Authorization;

/// <summary>
/// Merkezi permission eşleştirme. <c>module.manage</c>, aynı modülün
/// <c>module.view</c> gereksinimini karşılar. Controller'larda tekrarlanmaz.
/// </summary>
public static class PermissionSatisfaction
{
    private static readonly Dictionary<string, string[]> ViewImpliedBy = new(StringComparer.OrdinalIgnoreCase)
    {
        [Permissions.UsersView] = [Permissions.UsersManage],
        [Permissions.RolesView] = [Permissions.RolesManage, Permissions.PermissionsManage],
        [Permissions.PermissionsView] = [Permissions.PermissionsManage, Permissions.RolesManage],
        [Permissions.OrganizationView] = [Permissions.OrganizationManage],
        [Permissions.ContentView] =
        [
            Permissions.ContentCompanyManage,
            Permissions.ContentGlobalManage
        ],
        [Permissions.MediaView] = [Permissions.MediaManage],
        [Permissions.MenusView] = [Permissions.MenusManage],
        [Permissions.ServicesView] = [Permissions.ServicesManage],
        [Permissions.ReservationsView] = [Permissions.ReservationsManage]
    };

    /// <summary>
    /// Kullanıcının sahip olduğu permission setinin istenen kodu karşılayıp karşılamadığı.
    /// </summary>
    public static bool Satisfies(
        IEnumerable<string>? grantedPermissions,
        string requiredPermission)
    {
        if (string.IsNullOrWhiteSpace(requiredPermission))
        {
            return false;
        }

        var granted = grantedPermissions as ICollection<string>
            ?? grantedPermissions?.ToArray()
            ?? Array.Empty<string>();

        if (granted.Count == 0)
        {
            return false;
        }

        if (granted.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!ViewImpliedBy.TryGetValue(requiredPermission.Trim(), out var implying))
        {
            return false;
        }

        return implying.Any(code =>
            granted.Contains(code, StringComparer.OrdinalIgnoreCase));
    }

    public static bool SatisfiesAny(
        IEnumerable<string>? grantedPermissions,
        params string[] requiredPermissions) =>
        requiredPermissions.Any(required => Satisfies(grantedPermissions, required));
}
