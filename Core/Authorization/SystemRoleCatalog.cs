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
            [
                Permissions.AdminPanelAccess,
                Permissions.ContentGlobalManage,
                Permissions.ContentCompanyManage,
                Permissions.MediaManage,
                Permissions.UsersManage,
                Permissions.MenusManage,
                Permissions.PermissionsManage
            ]),
        new(
            Roles.CompanyAdmin,
            "Şirket yöneticisi — kendi şirketinde içerik, kullanıcı, menü ve rol atamaları",
            [
                Permissions.AdminPanelAccess,
                Permissions.ContentCompanyManage,
                Permissions.MediaManage,
                Permissions.UsersManage,
                Permissions.MenusManage,
                Permissions.PermissionsManage
            ]),
        new(
            Roles.HrManager,
            "İK yöneticisi — şirket kullanıcıları ve organizasyon yönetimi",
            [
                Permissions.AdminPanelAccess,
                Permissions.UsersManage
            ]),
        new(
            Roles.MenuManager,
            "Menü sorumlusu — yemek menüsü ve menü modülleri",
            [
                Permissions.AdminPanelAccess,
                Permissions.MenusManage
            ]),
        new(
            Roles.Editor,
            "İçerik editörü — duyuru, etkinlik, kampanya ve medya içerikleri",
            [
                Permissions.AdminPanelAccess,
                Permissions.ContentCompanyManage,
                Permissions.MediaManage
            ]),
        new(
            Roles.User,
            "Çalışan — yalnızca portal erişimi, admin paneli yok",
            [])
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
