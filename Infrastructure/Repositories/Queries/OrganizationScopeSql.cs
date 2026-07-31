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

    /// <summary>
    /// Org-scope list filter for alias <c>t</c> (All rows + accessible company rows).
    /// </summary>
    public const string ListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    t.company_scope = N'All'
                    OR t.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    t.company_scope = N'All'
                    OR t.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    /// <summary>Org-scope list filter for alias <c>a</c> (corporate_apps).</summary>
    public const string ListFilterAliasA = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    a.company_scope = N'All'
                    OR a.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    a.company_scope = N'All'
                    OR a.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    /// <summary>Org-scope list filter without table alias (e_cards, emergency_numbers, …).</summary>
    public const string ListFilterUnqualified = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    company_scope = N'All'
                    OR company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    company_scope = N'All'
                    OR company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    /// <summary>Dashboard counts using alias <c>c</c> / <c>sa</c> / <c>en</c> / <c>ec</c> / <c>tih</c>.</summary>
    public const string DashboardFilterAliasC = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    c.company_scope = N'All'
                    OR c.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    c.company_scope = N'All'
                    OR c.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    public const string DashboardFilterAliasSa = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    sa.company_scope = N'All'
                    OR sa.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    sa.company_scope = N'All'
                    OR sa.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    public const string DashboardFilterAliasEn = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    en.company_scope = N'All'
                    OR en.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    en.company_scope = N'All'
                    OR en.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    public const string DashboardFilterAliasEc = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    ec.company_scope = N'All'
                    OR ec.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    ec.company_scope = N'All'
                    OR ec.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;

    public const string DashboardFilterAliasTih = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (
                @CompanyId IS NOT NULL
                AND (
                    tih.company_scope = N'All'
                    OR tih.company_id = @CompanyId
                )
            )
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    tih.company_scope = N'All'
                    OR tih.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                )
            )
        )
        """;
}
