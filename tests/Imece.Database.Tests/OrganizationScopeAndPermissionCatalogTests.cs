using Core.Authorization;

namespace Imece.Database.Tests;

public sealed class OrganizationScopeAndPermissionCatalogTests
{
    [Fact]
    public void OrganizationScope_FromHasGlobalAccess_MapsCorrectly()
    {
        Assert.Equal(OrganizationAccessScope.Global, OrganizationScopeCodes.FromHasGlobalAccess(true));
        Assert.Equal(OrganizationAccessScope.Assigned, OrganizationScopeCodes.FromHasGlobalAccess(false));
        Assert.Equal("global", OrganizationScopeCodes.ToCode(OrganizationAccessScope.Global));
        Assert.Equal("assigned", OrganizationScopeCodes.ToCode(OrganizationAccessScope.Assigned));
    }

    [Fact]
    public void SystemPermissionCatalog_ContainsModuleLevelPermissions()
    {
        Assert.True(SystemPermissionCatalog.IsSystemPermission(Permissions.UsersView));
        Assert.True(SystemPermissionCatalog.IsSystemPermission(Permissions.UsersManage));
        Assert.True(SystemPermissionCatalog.IsSystemPermission(Permissions.RolesView));
        Assert.True(SystemPermissionCatalog.IsSystemPermission(Permissions.OrganizationManage));
        Assert.False(SystemPermissionCatalog.IsSystemPermission("users.create"));
    }

    [Fact]
    public void GlobalAdmin_HasAllCatalogPermissions()
    {
        var global = SystemRoleCatalog.Find(Roles.GlobalAdmin);
        Assert.NotNull(global);
        Assert.Equal(SystemPermissionCatalog.All.Count, global!.Permissions.Count);
    }

    [Fact]
    public void ApplicationUser_OrganizationScope_ReflectsGlobalFlag()
    {
        var globalUser = new ApplicationUser
        {
            Identity = new Core.Authentication.ExternalIdentity
            {
                IdentityProvider = "test",
                ExternalId = "1"
            },
            HasGlobalOrganizationAccess = true,
            Roles = [Roles.GlobalAdmin]
        };

        Assert.Equal(OrganizationAccessScope.Global, globalUser.OrganizationScope);

        var assignedUser = new ApplicationUser
        {
            Identity = new Core.Authentication.ExternalIdentity
            {
                IdentityProvider = "test",
                ExternalId = "2"
            },
            HasGlobalOrganizationAccess = false,
            Roles = [Roles.Editor]
        };

        Assert.Equal(OrganizationAccessScope.Assigned, assignedUser.OrganizationScope);
    }
}
