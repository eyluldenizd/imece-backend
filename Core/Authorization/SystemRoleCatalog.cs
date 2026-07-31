namespace Core.Authorization;

/// <summary>
/// Multi-tenant ortamda izin verilen sabit sistem rolleri.
/// </summary>
public static class SystemRoleCatalog
{
    public sealed record SystemRoleDefinition(
        string Code,
        string Description,
        IReadOnlyList<string> Permissions);

    private static readonly SystemRoleDefinition[] Definitions =
    [
        new(
            Roles.GlobalAdmin,
            "Platform yöneticisi — tüm şirketler, global içerik ve sistem ayarları",
            SystemPermissionCatalog.All.Select(p => p.Code).ToArray()),
        new(
            Roles.CompanyAdmin,
            "Şirket yöneticisi — erişilebilir şirketlerde içerik, kullanıcı, menü ve rol atamaları",
            [
                Permissions.AdminPanelAccess,
                Permissions.UsersView,
                Permissions.UsersManage,
                Permissions.RolesView,
                Permissions.PermissionsView,
                Permissions.OrganizationView,
                Permissions.OrganizationManage,
                Permissions.ContentView,
                Permissions.ContentCompanyManage,
                Permissions.MediaView,
                Permissions.MediaManage,
                Permissions.MenusView,
                Permissions.MenusManage,
                Permissions.ServicesView,
                Permissions.ServicesManage,
                Permissions.ReservationsView,
                Permissions.ReservationsManage,
                Permissions.ReportsView
            ]),
        new(
            Roles.HrManager,
            "İK yöneticisi — şirket kullanıcıları ve organizasyon yönetimi",
            [
                Permissions.AdminPanelAccess,
                Permissions.UsersView,
                Permissions.UsersManage,
                Permissions.OrganizationView,
                Permissions.OrganizationManage
            ]),
        new(
            Roles.MenuManager,
            "Menü sorumlusu — yemek menüsü ve menü modülleri",
            [
                Permissions.AdminPanelAccess,
                Permissions.MenusView,
                Permissions.MenusManage,
                Permissions.ContentView
            ]),
        new(
            Roles.Editor,
            "İçerik editörü — duyuru, etkinlik, kampanya ve medya içerikleri",
            [
                Permissions.AdminPanelAccess,
                Permissions.ContentView,
                Permissions.ContentCompanyManage,
                Permissions.MediaView,
                Permissions.MediaManage
            ]),
        new(
            Roles.User,
            "Çalışan — yalnızca portal erişimi, admin paneli yok",
            [
                Permissions.ContentView,
                Permissions.MediaView,
                Permissions.MenusView,
                Permissions.ServicesView,
                Permissions.ReservationsView
            ])
    ];

    public static IReadOnlyList<SystemRoleDefinition> All => Definitions;

    public static bool IsSystemRole(string? roleName) =>
        !string.IsNullOrWhiteSpace(roleName)
        && Definitions.Any(definition =>
            string.Equals(definition.Code, roleName.Trim(), StringComparison.OrdinalIgnoreCase));

    public static SystemRoleDefinition? Find(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        return Definitions.FirstOrDefault(definition =>
            string.Equals(definition.Code, roleName.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
