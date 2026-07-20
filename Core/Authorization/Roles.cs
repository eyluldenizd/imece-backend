namespace Core.Authorization;

public static class Roles
{
    public const string User = "user";
    public const string CompanyAdmin = "company_admin";
    public const string GlobalAdmin = "global_admin";
    public const string Editor = "editor";
}

public static class Permissions
{
    public const string AdminPanelAccess = "admin.panel.access";
    public const string ContentGlobalManage = "content.global.manage";
    public const string ContentCompanyManage = "content.company.manage";
    public const string MediaManage = "media.manage";
    public const string UsersManage = "users.manage";
}

public static class ImecePolicies
{
    public const string RequireCompanyAdminOrGlobalContentManager = "RequireCompanyAdminOrGlobalContentManager";
    public const string RequireRegisteredUser = "RequireRegisteredUser";
    public const string RequireCompany = "RequireCompany";
    public const string RequireCompanyAdmin = "RequireCompanyAdmin";
    public const string RequireGlobalAdmin = "RequireGlobalAdmin";
    public const string RequireGlobalContentManager = "RequireGlobalContentManager";
}
