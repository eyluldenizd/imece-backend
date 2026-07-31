namespace Core.Authorization;

/// <summary>
/// Seed edilen sabit sistem yetkileri. Kodları auth policy / seed contract'ıdır.
/// Admin UI rastgele yeni permission code oluşturmaz; catalog genişletilir.
/// </summary>
public static class SystemPermissionCatalog
{
    public sealed record SystemPermissionDefinition(string Code, string Description);

    private static readonly SystemPermissionDefinition[] Definitions =
    [
        new(Permissions.AdminPanelAccess, "Admin panel erişimi"),
        new(Permissions.UsersView, "Kullanıcıları görüntüleme"),
        new(Permissions.UsersManage, "Kullanıcı yönetimi"),
        new(Permissions.RolesView, "Rolleri görüntüleme"),
        new(Permissions.RolesManage, "Rol yönetimi"),
        new(Permissions.PermissionsView, "Yetkileri görüntüleme"),
        new(Permissions.PermissionsManage, "Yetki ve rol atama yönetimi"),
        new(Permissions.OrganizationView, "Organizasyon (şirket/şube/birim) görüntüleme"),
        new(Permissions.OrganizationManage, "Organizasyon ve şirket erişimi yönetimi"),
        new(Permissions.ContentView, "İçerik görüntüleme"),
        new(Permissions.ContentGlobalManage, "Global içerik yönetimi"),
        new(Permissions.ContentCompanyManage, "Şirket içerik yönetimi"),
        new(Permissions.MediaView, "Medya görüntüleme"),
        new(Permissions.MediaManage, "Medya yönetimi"),
        new(Permissions.MenusView, "Menü görüntüleme"),
        new(Permissions.MenusManage, "Menü yönetimi"),
        new(Permissions.ServicesView, "Servisleri görüntüleme"),
        new(Permissions.ServicesManage, "Servis yönetimi"),
        new(Permissions.ReservationsView, "Rezervasyonları görüntüleme"),
        new(Permissions.ReservationsManage, "Rezervasyon yönetimi"),
        new(Permissions.ReportsView, "Raporları görüntüleme")
    ];

    public static IReadOnlyList<SystemPermissionDefinition> All => Definitions;

    public static bool IsSystemPermission(string? permissionCode) =>
        !string.IsNullOrWhiteSpace(permissionCode)
        && Definitions.Any(definition =>
            string.Equals(definition.Code, permissionCode.Trim(), StringComparison.OrdinalIgnoreCase));

    public static SystemPermissionDefinition? Find(string? permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return null;
        }

        return Definitions.FirstOrDefault(definition =>
            string.Equals(definition.Code, permissionCode.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
