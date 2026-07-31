using Core.Authorization;

namespace Imece.Authorization.Tests;

public sealed class PermissionSatisfactionTests
{
    [Fact]
    public void Manage_Implies_View_For_Same_Module()
    {
        var granted = new[] { Permissions.UsersManage };
        Assert.True(PermissionSatisfaction.Satisfies(granted, Permissions.UsersView));
        Assert.True(PermissionSatisfaction.Satisfies(granted, Permissions.UsersManage));
        Assert.False(PermissionSatisfaction.Satisfies(granted, Permissions.RolesView));
    }

    [Fact]
    public void Content_Manage_Implies_Content_View()
    {
        Assert.True(PermissionSatisfaction.Satisfies(
            [Permissions.ContentCompanyManage],
            Permissions.ContentView));
        Assert.True(PermissionSatisfaction.Satisfies(
            [Permissions.ContentGlobalManage],
            Permissions.ContentView));
    }

    [Fact]
    public void Media_And_Menus_Manage_Imply_View()
    {
        Assert.True(PermissionSatisfaction.Satisfies([Permissions.MediaManage], Permissions.MediaView));
        Assert.True(PermissionSatisfaction.Satisfies([Permissions.MenusManage], Permissions.MenusView));
    }

    [Fact]
    public void Missing_Permission_Is_Rejected()
    {
        Assert.False(PermissionSatisfaction.Satisfies(
            [Permissions.ContentView],
            Permissions.UsersManage));
    }

    [Fact]
    public void SatisfiesAny_Requires_At_Least_One()
    {
        var granted = new[] { Permissions.ServicesManage };
        Assert.True(PermissionSatisfaction.SatisfiesAny(
            granted,
            Permissions.ServicesView,
            Permissions.UsersView));
        Assert.False(PermissionSatisfaction.SatisfiesAny(
            granted,
            Permissions.UsersView,
            Permissions.RolesView));
    }
}

public sealed class PermissionResolutionModelTests
{
    [Fact]
    public void Multiple_Roles_Union_Permissions_Without_Duplicates()
    {
        var roleA = new[] { Permissions.ContentView, Permissions.MediaView };
        var roleB = new[] { Permissions.ContentView, Permissions.ServicesView };

        var effective = roleA
            .Concat(roleB)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(3, effective.Length);
        Assert.Equal(
            new[] { Permissions.ContentView, Permissions.MediaView, Permissions.ServicesView },
            effective);
    }

    [Fact]
    public void Passive_Role_Permissions_Are_Not_Included_When_Filtered()
    {
        var activeRolePermissions = new[] { Permissions.UsersView };
        var passiveRolePermissions = new[] { Permissions.UsersManage };

        // Directory resolver only joins active roles; simulate that filter.
        var effective = activeRolePermissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Contains(Permissions.UsersView, effective);
        Assert.DoesNotContain(Permissions.UsersManage, effective);
    }

    [Fact]
    public void GlobalAdmin_Catalog_Has_All_Permissions()
    {
        var global = SystemRoleCatalog.Find(Roles.GlobalAdmin);
        Assert.NotNull(global);
        Assert.Equal(SystemPermissionCatalog.All.Count, global!.Permissions.Count);
        Assert.All(
            SystemPermissionCatalog.All,
            def => Assert.Contains(def.Code, global.Permissions));
    }

    [Fact]
    public void CompanyAdmin_Does_Not_Get_Global_Content_Or_Roles_Manage()
    {
        var companyAdmin = SystemRoleCatalog.Find(Roles.CompanyAdmin);
        Assert.NotNull(companyAdmin);
        Assert.DoesNotContain(Permissions.ContentGlobalManage, companyAdmin!.Permissions);
        Assert.DoesNotContain(Permissions.RolesManage, companyAdmin.Permissions);
        Assert.Contains(Permissions.ContentCompanyManage, companyAdmin.Permissions);
    }
}

public sealed class OrganizationScopeModelTests
{
    [Fact]
    public void Global_And_Assigned_Codes_Are_Stable()
    {
        Assert.Equal("global", OrganizationScopeCodes.ToCode(OrganizationAccessScope.Global));
        Assert.Equal("assigned", OrganizationScopeCodes.ToCode(OrganizationAccessScope.Assigned));
        Assert.Equal(OrganizationAccessScope.Global, OrganizationScopeCodes.FromHasGlobalAccess(true));
        Assert.Equal(OrganizationAccessScope.Assigned, OrganizationScopeCodes.FromHasGlobalAccess(false));
    }

    [Fact]
    public void ApplicationUser_OrganizationScope_Follows_Flag()
    {
        var global = new ApplicationUser
        {
            Identity = new Core.Authentication.ExternalIdentity
            {
                IdentityProvider = "test",
                ExternalId = "g1"
            },
            HasGlobalOrganizationAccess = true,
            Roles = [Roles.GlobalAdmin],
            Permissions = SystemPermissionCatalog.All.Select(p => p.Code).ToArray(),
            CompanyMemberships = []
        };

        Assert.Equal(OrganizationAccessScope.Global, global.OrganizationScope);
        Assert.Empty(global.CompanyMemberships);
    }
}

public sealed class PolicyInventoryTests
{
    [Fact]
    public void Obsolete_Role_Policy_Names_Alias_To_Permission_Policies()
    {
#pragma warning disable CS0618
        Assert.Equal(
            ImecePolicies.RequireContentCompanyManage,
            ImecePolicies.RequireCompanyAdminOrGlobalContentManager);
        Assert.Equal(
            ImecePolicies.RequireContentGlobalManage,
            ImecePolicies.RequireGlobalContentManager);
#pragma warning restore CS0618
    }

    [Fact]
    public void Catalog_Contains_Media_And_Menus_View()
    {
        Assert.True(SystemPermissionCatalog.IsSystemPermission(Permissions.MediaView));
        Assert.True(SystemPermissionCatalog.IsSystemPermission(Permissions.MenusView));
        Assert.Equal(21, SystemPermissionCatalog.All.Count);
    }

    [Fact]
    public void Catalog_Has_View_And_Manage_For_Each_Module()
    {
        var codes = SystemPermissionCatalog.All.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(Permissions.UsersView, codes);
        Assert.Contains(Permissions.UsersManage, codes);
        Assert.Contains(Permissions.RolesView, codes);
        Assert.Contains(Permissions.RolesManage, codes);
        Assert.Contains(Permissions.PermissionsView, codes);
        Assert.Contains(Permissions.PermissionsManage, codes);
        Assert.Contains(Permissions.OrganizationView, codes);
        Assert.Contains(Permissions.OrganizationManage, codes);
        Assert.Contains(Permissions.ContentView, codes);
        Assert.Contains(Permissions.ContentCompanyManage, codes);
        Assert.Contains(Permissions.ContentGlobalManage, codes);
        Assert.Contains(Permissions.MediaView, codes);
        Assert.Contains(Permissions.MediaManage, codes);
        Assert.Contains(Permissions.MenusView, codes);
        Assert.Contains(Permissions.MenusManage, codes);
        Assert.Contains(Permissions.ServicesView, codes);
        Assert.Contains(Permissions.ServicesManage, codes);
        Assert.Contains(Permissions.ReservationsView, codes);
        Assert.Contains(Permissions.ReservationsManage, codes);
        Assert.Contains(Permissions.ReportsView, codes);
        Assert.Contains(Permissions.AdminPanelAccess, codes);
        Assert.Equal(21, codes.Count);
    }

    [Fact]
    public void Controllers_Do_Not_Use_Role_Name_Authorize_Roles_Attribute()
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "ImeceWebAPI", "Controllers")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ImeceWebAPI", "Controllers"))
        };

        var controllersRoot = candidates.FirstOrDefault(Directory.Exists);
        Assert.True(controllersRoot is not null, "Controllers path missing");

        var hits = Directory.EnumerateFiles(controllersRoot!, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path).Select((line, idx) => (path, line, idx)))
            .Where(x =>
                x.line.Contains("[Authorize(Roles", StringComparison.Ordinal)
                || x.line.Contains("ImecePolicies.RequireCompanyAdmin", StringComparison.Ordinal)
                || x.line.Contains("ImecePolicies.RequireGlobalAdmin", StringComparison.Ordinal)
                || x.line.Contains("RequireCompanyAdminOrGlobalContentManager", StringComparison.Ordinal))
            .Select(x => $"{Path.GetFileName(x.path)}:{x.idx + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(hits.Count == 0, "Role-name policies remain:\n" + string.Join("\n", hits));
    }
}

public sealed class CreateRoleContractTests
{
    [Fact]
    public void CreateRoleDto_Supports_Optional_PermissionIds()
    {
        var dto = new Application.DTOs.CreateRoleDto
        {
            RoleName = "custom_editor",
            Description = "test",
            IsActive = true,
            PermissionIds = [1, 2, 2, 3]
        };

        Assert.NotNull(dto.PermissionIds);
        Assert.Equal(4, dto.PermissionIds!.Length);
        Assert.Equal(3, dto.PermissionIds.Distinct().Count());
    }
}
