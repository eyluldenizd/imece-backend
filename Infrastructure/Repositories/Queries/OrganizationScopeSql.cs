namespace Infrastructure.Repositories.Queries;

public static class OrganizationScopeSql
{
    /// <summary>
    /// Scope columns from the primary alias <c>t</c>. Must be qualified when ListJoins are used
    /// (companies/branches/departments also expose company_id / branch_id / department_id).
    /// </summary>
    public const string SelectColumns = """
        t.company_scope AS CompanyScope,
        t.company_id AS CompanyId,
        t.branch_scope AS BranchScope,
        t.branch_id AS BranchId,
        t.department_scope AS DepartmentScope,
        t.department_id AS DepartmentId
        """;

    public const string ListJoins = """
        LEFT JOIN companies AS co ON co.company_id = t.company_id
        LEFT JOIN branches AS b ON b.branch_id = t.branch_id
        LEFT JOIN departments AS d ON d.department_id = t.department_id
        """;

    public const string ListNameColumns = """
        co.company_name AS CompanyName,
        b.branch_name AS BranchName,
        d.department_name AS DepartmentName
        """;
}
