namespace Application.Common.OrganizationScope;

public static class OrganizationScopeLabelFormatter
{
    public static string Format(
        string companyScope,
        string? companyName,
        string branchScope,
        string? branchName,
        string departmentScope,
        string? departmentName)
    {
        if (string.IsNullOrWhiteSpace(companyScope)
            || companyScope.Equals(OrganizationScopeFieldHelper.All, StringComparison.OrdinalIgnoreCase))
        {
            return "Tümü";
        }

        var parts = new List<string>();
        var companyLabel = companyName?.Trim();
        parts.Add(string.IsNullOrWhiteSpace(companyLabel) ? "Şirket" : companyLabel);

        if (branchScope.Equals(OrganizationScopeFieldHelper.Specific, StringComparison.OrdinalIgnoreCase))
        {
            var branchLabel = branchName?.Trim();
            parts.Add(string.IsNullOrWhiteSpace(branchLabel) ? "Şube" : branchLabel);
        }

        if (departmentScope.Equals(OrganizationScopeFieldHelper.Specific, StringComparison.OrdinalIgnoreCase))
        {
            var departmentLabel = departmentName?.Trim();
            parts.Add(string.IsNullOrWhiteSpace(departmentLabel) ? "Departman" : departmentLabel);
        }

        return string.Join(" › ", parts);
    }
}
