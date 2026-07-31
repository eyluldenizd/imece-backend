namespace Core.Authorization;

public static class Roles
{
    public const string User = "user";
    public const string Editor = "editor";
    public const string HrManager = "hr_manager";
    public const string MenuManager = "menu_manager";
    public const string CompanyAdmin = "company_admin";
    public const string GlobalAdmin = "global_admin";
}

public static class Permissions
{
    public const string AdminPanelAccess = "admin.panel.access";

    public const string UsersView = "users.view";
    public const string UsersManage = "users.manage";

    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";

    public const string PermissionsView = "permissions.view";
    public const string PermissionsManage = "permissions.manage";

    public const string OrganizationView = "organization.view";
    public const string OrganizationManage = "organization.manage";

    public const string ContentView = "content.view";
    public const string ContentGlobalManage = "content.global.manage";
    public const string ContentCompanyManage = "content.company.manage";

    public const string MediaView = "media.view";
    public const string MediaManage = "media.manage";

    public const string MenusView = "menus.view";
    public const string MenusManage = "menus.manage";

    public const string ServicesView = "services.view";
    public const string ServicesManage = "services.manage";

    public const string ReservationsView = "reservations.view";
    public const string ReservationsManage = "reservations.manage";

    public const string ReportsView = "reports.view";
}

/// <summary>
/// Organizasyon (şirket) erişim kapsamı. Global: tüm şirketler;
/// Assigned: yalnız <c>user_company_access</c> kayıtları.
/// </summary>
public enum OrganizationAccessScope
{
    Assigned = 0,
    Global = 1
}

public static class OrganizationScopeCodes
{
    public const string Global = "global";
    public const string Assigned = "assigned";

    public static string ToCode(OrganizationAccessScope scope) =>
        scope == OrganizationAccessScope.Global ? Global : Assigned;

    public static OrganizationAccessScope FromHasGlobalAccess(bool hasGlobalAccess) =>
        hasGlobalAccess ? OrganizationAccessScope.Global : OrganizationAccessScope.Assigned;
}

public static class ImecePolicies
{
    public const string RequireRegisteredUser = "RequireRegisteredUser";
    public const string RequireCompany = "RequireCompany";

    public const string RequireAdminPanelAccess = "RequireAdminPanelAccess";

    public const string RequireUsersView = "RequireUsersView";
    public const string RequireUsersManage = "RequireUsersManage";

    public const string RequireRolesView = "RequireRolesView";
    public const string RequireRolesManage = "RequireRolesManage";

    public const string RequirePermissionsView = "RequirePermissionsView";
    public const string RequirePermissionsManage = "RequirePermissionsManage";

    public const string RequireOrganizationView = "RequireOrganizationView";
    public const string RequireOrganizationManage = "RequireOrganizationManage";

    public const string RequireContentView = "RequireContentView";
    public const string RequireContentCompanyManage = "RequireContentCompanyManage";
    public const string RequireContentGlobalManage = "RequireContentGlobalManage";

    public const string RequireMediaView = "RequireMediaView";
    public const string RequireMediaManage = "RequireMediaManage";

    public const string RequireMenusView = "RequireMenusView";
    public const string RequireMenusManage = "RequireMenusManage";

    public const string RequireServicesView = "RequireServicesView";
    public const string RequireServicesManage = "RequireServicesManage";

    public const string RequireReservationsView = "RequireReservationsView";
    public const string RequireReservationsManage = "RequireReservationsManage";

    public const string RequireReportsView = "RequireReportsView";

    // Legacy aliases — kept only so older attribute strings compile during migration.
    // Controllers must not use these; prefer the permission policies above.
    [Obsolete("Use RequireContentCompanyManage or module permission policies.")]
    public const string RequireCompanyAdmin = "RequireCompanyAdmin";

    [Obsolete("Use RequireOrganizationManage or HasGlobalOrganizationAccess.")]
    public const string RequireGlobalAdmin = "RequireGlobalAdmin";

    [Obsolete("Use RequireContentGlobalManage.")]
    public const string RequireGlobalContentManager = "RequireContentGlobalManage";

    [Obsolete("Use RequireContentCompanyManage.")]
    public const string RequireCompanyAdminOrGlobalContentManager = "RequireContentCompanyManage";
}
