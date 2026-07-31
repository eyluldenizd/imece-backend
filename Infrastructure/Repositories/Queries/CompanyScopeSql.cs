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

    /// <summary>Branch.company_id üzerinden filtre (weekly_menu_entries).</summary>
    public const string BranchCompanyListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (@CompanyId IS NOT NULL AND b.company_id = @CompanyId)
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND b.company_id IN (
                    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                    FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                )
            )
        )
        """;

    /// <summary>
    /// service_routes: departure/arrival location company veya null company location.
    /// Location ID yoksa yalnız global (filtre null/null) görür.
    /// </summary>
    public const string ServiceRouteListFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR EXISTS (
                SELECT 1
                FROM service_locations AS sl
                WHERE (
                        sl.service_location_id = service_routes.departure_location_id
                     OR sl.service_location_id = service_routes.arrival_location_id
                )
                AND (
                        (
                            @CompanyId IS NOT NULL
                            AND (sl.company_id = @CompanyId OR sl.company_id IS NULL)
                        )
                     OR (
                            @AccessibleCompanyIds IS NOT NULL
                            AND (
                                sl.company_id IS NULL
                                OR sl.company_id IN (
                                    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                                    FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                                )
                            )
                        )
                )
            )
        )
        """;

    public const string UserMembershipFilter = """
        (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR EXISTS (
                SELECT 1
                FROM user_company_roles AS ucr_scope
                WHERE ucr_scope.user_id = u.user_id
                  AND ucr_scope.is_active = 1
                  AND (
                        (@CompanyId IS NOT NULL AND ucr_scope.company_id = @CompanyId)
                     OR (
                            @AccessibleCompanyIds IS NOT NULL
                            AND ucr_scope.company_id IN (
                                SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                                FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                            )
                        )
                  )
            )
            OR EXISTS (
                SELECT 1
                FROM branches AS b_scope
                WHERE b_scope.branch_id = u.branch_id
                  AND (
                        (@CompanyId IS NOT NULL AND b_scope.company_id = @CompanyId)
                     OR (
                            @AccessibleCompanyIds IS NOT NULL
                            AND b_scope.company_id IN (
                                SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                                FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                            )
                        )
                  )
            )
        )
        """;
}
