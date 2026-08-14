using Application.Common.CompanyScope;
using Application.Exceptions;
using Core.Authorization;

namespace Imece.Authorization.Tests;

public sealed class CompanyListScopeRulesTests
{
    [Fact]
    public void Global_Admin_Without_Requested_Company_Gets_Unrestricted_Filter()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: true, accessible: [1, 2]);
        var currentUser = new StubCurrentUser(memberships: []);

        var filter = CompanyScopeRules.ResolveListCompanyFilter(companyContext, currentUser);

        Assert.Null(filter.CompanyId);
        Assert.Null(filter.AccessibleCompanyIdsCsv);
    }

    [Fact]
    public void Assigned_Multi_Company_Without_Request_Uses_Accessible_Csv()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: false, accessible: [10, 20]);
        var currentUser = new StubCurrentUser(memberships: [10, 20]);

        var filter = CompanyScopeRules.ResolveListCompanyFilter(companyContext, currentUser);

        Assert.Null(filter.CompanyId);
        Assert.Equal("10,20", filter.AccessibleCompanyIdsCsv);
    }

    [Fact]
    public void Assigned_Single_Company_Uses_CompanyId()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: false, accessible: [7]);
        var currentUser = new StubCurrentUser(memberships: [7]);

        var filter = CompanyScopeRules.ResolveListCompanyFilter(companyContext, currentUser);

        Assert.Equal(7, filter.CompanyId);
        Assert.Null(filter.AccessibleCompanyIdsCsv);
    }

    [Fact]
    public void Requested_Accessible_Company_Narrows_Filter()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: false, accessible: [1, 2]);
        var currentUser = new StubCurrentUser(memberships: [1, 2]);

        var filter = CompanyScopeRules.ResolveListCompanyFilter(companyContext, currentUser, requestedCompanyId: 2);

        Assert.Equal(2, filter.CompanyId);
        Assert.Null(filter.AccessibleCompanyIdsCsv);
    }

    [Fact]
    public void Requested_Unauthorized_Company_Throws_Forbidden()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: false, accessible: [1]);
        var currentUser = new StubCurrentUser(memberships: [1]);

        Assert.Throws<ForbiddenException>(() =>
            CompanyScopeRules.ResolveListCompanyFilter(companyContext, currentUser, requestedCompanyId: 99));
    }

    [Fact]
    public void Organization_All_Scope_Is_Readable_Without_Company()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: false, accessible: [1]);
        CompanyScopeRules.EnsureOrganizationScopeReadAccess(companyContext, "All", null);
    }

    [Fact]
    public void Organization_Specific_Scope_Requires_Company_Access()
    {
        var companyContext = new StubCompanyContext(isGlobalAdmin: false, accessible: [1]);
        Assert.Throws<ForbiddenException>(() =>
            CompanyScopeRules.EnsureOrganizationScopeReadAccess(companyContext, "Specific", 99));
    }

    [Fact]
    public void Scoped_Query_Sources_Contain_Company_Filters()
    {
        var repoRoot = FindRepoRoot();
        var files = new Dictionary<string, string[]>
        {
            ["CampaignsQueries.cs"] = ["OrganizationScopeSql.ListFilter"],
            ["SocialActivityQueries.cs"] = ["OrganizationScopeSql.ListFilter"],
            ["CorporateAppsQueries.cs"] = ["OrganizationScopeSql.ListFilterAliasA"],
            ["ServicesQueries.cs"] = ["OrganizationScopeSql.ListFilter"],
            ["CommunicationChannelsQueries.cs"] = ["OrganizationScopeSql.ListFilter"],
            ["ECardQueries.cs"] = ["OrganizationScopeSql.ListFilterUnqualified"],
            ["EmergencyNumbersQueries.cs"] = ["OrganizationScopeSql.ListFilter"],
            ["TodayInHistoruQueries.cs"] = ["OrganizationScopeSql.ListFilterUnqualified"],
            ["ReservationQueries.cs"] = ["ISNULL(r.company_id, mr.company_id)", "@AccessibleCompanyIds"],
            ["MediaFileQueries.cs"] = ["CompanyScopeSql.MediaFileListFilter"],
            ["WeeklyMenuEntryQueries.cs"] = ["CompanyScopeSql.BranchCompanyListFilter"],
            ["ServieRouteQueries.cs"] = ["OrganizationScopeSql.ListFilter"],
            ["DashboardQueries.cs"] =
            [
                "OrganizationScopeSql.ListFilter",
                "CompanyScopeSql.ServiceRouteListFilter",
                "OrganizationScopeSql.DashboardFilterAliasC"
            ]
        };

        var queriesDir = Path.Combine(repoRoot, "Infrastructure", "Repositories", "Queries");
        Assert.True(Directory.Exists(queriesDir), queriesDir);

        foreach (var (fileName, needles) in files)
        {
            var path = Path.Combine(queriesDir, fileName);
            Assert.True(File.Exists(path), path);
            var text = File.ReadAllText(path);
            foreach (var needle in needles)
            {
                Assert.Contains(needle, text, StringComparison.Ordinal);
            }
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ImeceWebAPI", "ImeceWebAPI.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Repository root not found.");
    }

    private sealed class StubCompanyContext : ICompanyContext
    {
        private readonly HashSet<int> _accessible;

        public StubCompanyContext(bool isGlobalAdmin, IEnumerable<int> accessible)
        {
            IsGlobalAdmin = isGlobalAdmin;
            _accessible = accessible.ToHashSet();
            CurrentCompanyId = _accessible.Count == 1 ? _accessible.First() : null;
        }

        public int? CurrentCompanyId { get; }
        public int? CompanyId => CurrentCompanyId;
        public string? CompanyName => null;
        public bool HasCompany => CurrentCompanyId.HasValue;
        public bool IsGlobalAdmin { get; }

        public bool CanAccessCompany(int companyId) =>
            IsGlobalAdmin || _accessible.Contains(companyId);

        public void EnsureCanAccessCompany(int companyId)
        {
            if (!CanAccessCompany(companyId))
            {
                throw new ForbiddenException("Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
            }
        }

        public int GetRequiredCompanyId() =>
            CurrentCompanyId ?? throw new ForbiddenException("Company required.");
    }

    private sealed class StubCurrentUser : ICurrentUser
    {
        public StubCurrentUser(IEnumerable<int> memberships)
        {
            CompanyMemberships = memberships
                .Select(id => new CompanyMembership
                {
                    CompanyId = id,
                    CompanyName = $"C{id}"
                })
                .ToArray();
        }

        public bool IsAuthenticated => true;
        public bool IsRegistered => true;
        public bool IsActive => true;
        public int? UserId => 1;
        public string? ExternalId => "ext";
        public string? Username => "user";
        public string? Email => "a@b.c";
        public string? DisplayName => "Test";
        public string? IdentityProvider => "test";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public IReadOnlyCollection<CompanyMembership> CompanyMemberships { get; }
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;
        public int GetRequiredUserId() => 1;
    }
}
