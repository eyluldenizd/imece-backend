namespace Application.Common.OrganizationScope;

public static class OrganizationScopeFieldHelper
{
    public const string All = "All";
    public const string Specific = "Specific";

    public static OrganizationScopeLevel ParseLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Equals(All, StringComparison.OrdinalIgnoreCase))
        {
            return OrganizationScopeLevel.All;
        }

        if (value.Equals(Specific, StringComparison.OrdinalIgnoreCase))
        {
            return OrganizationScopeLevel.Specific;
        }

        throw new InvalidOperationException(
            "Geçersiz kapsam değeri. All veya Specific olmalıdır.");
    }

    public static string FormatLevel(OrganizationScopeLevel level) =>
        level == OrganizationScopeLevel.All ? All : Specific;

    public static OrganizationScope ToScope(
        string? companyScope,
        int? companyId,
        string? branchScope,
        int? branchId,
        string? departmentScope,
        int? departmentId) =>
        new()
        {
            CompanyScope = ParseLevel(companyScope),
            CompanyId = companyId,
            BranchScope = ParseLevel(branchScope),
            BranchId = branchId,
            DepartmentScope = ParseLevel(departmentScope),
            DepartmentId = departmentId
        };

    public static void ApplyResolved(
        ResolvedOrganizationScope resolved,
        Action<string, int?, string, int?, string, int?> apply)
    {
        apply(
            FormatLevel(resolved.CompanyScope),
            resolved.CompanyId,
            FormatLevel(resolved.BranchScope),
            resolved.BranchId,
            FormatLevel(resolved.DepartmentScope),
            resolved.DepartmentId);
    }
}
