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
    public const string ContentGlobalManage = "content.global.manage";
    public const string ContentCompanyManage = "content.company.manage";
    public const string MediaManage = "media.manage";
    public const string UsersManage = "users.manage";
    public const string MenusManage = "menus.manage";
    public const string PermissionsManage = "permissions.manage";
}

public static class ImecePolicies
{
    public const string RequireCompanyAdminOrGlobalContentManager = "RequireCompanyAdminOrGlobalContentManager";
    public const string RequireRegisteredUser = "RequireRegisteredUser";
    public const string RequireCompany = "RequireCompany";
    public const string RequireCompanyAdmin = "RequireCompanyAdmin";
    public const string RequireGlobalAdmin = "RequireGlobalAdmin";
    public const string RequireGlobalContentManager = "RequireGlobalContentManager";
    public const string RequireUsersManage = "RequireUsersManage";
    public const string RequireMenusManage = "RequireMenusManage";
    public const string RequirePermissionsManage = "RequirePermissionsManage";
}
