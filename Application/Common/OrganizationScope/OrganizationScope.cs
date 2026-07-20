namespace Application.Common.OrganizationScope;

public enum OrganizationScopeLevel
{
    All,
    Specific
}

/// <summary>
/// Hierarchical organization targeting: company → branch → department.
/// </summary>
public sealed class OrganizationScope
{
    public OrganizationScopeLevel CompanyScope { get; set; } = OrganizationScopeLevel.All;

    public int? CompanyId { get; set; }

    public OrganizationScopeLevel BranchScope { get; set; } = OrganizationScopeLevel.All;

    public int? BranchId { get; set; }

    public OrganizationScopeLevel DepartmentScope { get; set; } = OrganizationScopeLevel.All;

    public int? DepartmentId { get; set; }
}

public sealed record ResolvedOrganizationScope(
    OrganizationScopeLevel CompanyScope,
    int? CompanyId,
    OrganizationScopeLevel BranchScope,
    int? BranchId,
    OrganizationScopeLevel DepartmentScope,
    int? DepartmentId);
