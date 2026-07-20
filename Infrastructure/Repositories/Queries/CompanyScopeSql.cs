namespace Infrastructure.Repositories.Queries;

internal static class CompanyScopeSql
{
    public const string ListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (@CompanyId IS NOT NULL AND (company_id = @CompanyId OR scope_type = N'Global'))
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                    OR scope_type = N'Global'
                )
            )
        )
        """;

    public const string FolderListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (@CompanyId IS NOT NULL AND mf.company_id = @CompanyId)
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND mf.company_id IN (
                    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                    FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                )
            )
        )
        """;

    public const string MediaFileListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (@CompanyId IS NOT NULL AND (mf.company_id = @CompanyId OR mf.scope_type = N'Global'))
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND (
                    mf.company_id IN (
                        SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                        FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                    )
                    OR mf.scope_type = N'Global'
                )
            )
        )
        """;

    public const string CompanyOnlyListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (@CompanyId IS NOT NULL AND company_id = @CompanyId)
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND company_id IN (
                    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                    FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                )
            )
        )
        """;
}
