namespace Infrastructure.Repositories.Queries;

public static class DashboardQueries
{
    // users has no company_id — membership is via user_company_roles.
    public const string CountUsers = """
        SELECT COUNT(DISTINCT u.user_id)
        FROM users AS u
        WHERE u.is_active = 1
          AND (
                @CompanyId IS NULL
                AND @AccessibleCompanyIds IS NULL
              OR EXISTS (
                    SELECT 1
                    FROM user_company_roles AS ucr
                    WHERE ucr.user_id = u.user_id
                      AND ucr.is_active = 1
                      AND (
                            (@CompanyId IS NOT NULL AND ucr.company_id = @CompanyId)
                         OR (@AccessibleCompanyIds IS NOT NULL AND ucr.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')))
                          )
                )
              );
        """;

    public const string CountActiveCompanies = """
        SELECT COUNT(1)
        FROM companies AS c
        WHERE c.is_active = 1
          AND (@CompanyId IS NULL OR c.company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR c.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountBranches = """
        SELECT COUNT(1)
        FROM branches AS b
        WHERE b.is_active = 1
          AND (@CompanyId IS NULL OR b.company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR b.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountDepartments = """
        SELECT COUNT(1)
        FROM departments AS d
        INNER JOIN branches AS b ON b.branch_id = d.branch_id
        WHERE d.is_active = 1
          AND (@CompanyId IS NULL OR b.company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR b.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountAnnouncements = """
        SELECT COUNT(1)
        FROM announcements AS a
        WHERE (@CompanyId IS NULL OR a.company_id = @CompanyId OR a.scope_type = N'Global')
          AND (@AccessibleCompanyIds IS NULL OR a.company_id IS NULL OR a.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountUpcomingEvents = """
        SELECT COUNT(1)
        FROM events AS e
        WHERE e.start_datetime >= SYSUTCDATETIME()
          AND (@CompanyId IS NULL OR e.company_id = @CompanyId OR e.scope_type = N'Global')
          AND (@AccessibleCompanyIds IS NULL OR e.company_id IS NULL OR e.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountActiveServices = """
        SELECT COUNT(1)
        FROM services
        WHERE is_active = 1;
        """;

    public const string CountMediaFiles = """
        SELECT COUNT(1)
        FROM media_files AS mf
        WHERE mf.is_active = 1
          AND (@CompanyId IS NULL OR mf.company_id = @CompanyId OR mf.scope_type = N'Global')
          AND (@AccessibleCompanyIds IS NULL OR mf.company_id IS NULL OR mf.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountReservations = """
        SELECT COUNT(1)
        FROM reservations AS r
        WHERE r.status <> 'cancelled'
          AND (@CompanyId IS NULL OR r.company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR r.company_id IS NULL OR r.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string CountPublishedWeeklyMenus = """
        SELECT COUNT(1)
        FROM weekly_menus AS wm
        WHERE wm.is_published = 1
          AND wm.is_active = 1
          AND (@CompanyId IS NULL OR wm.company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR wm.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')));
        """;

    public const string EventsByMonth = """
        SELECT
            YEAR(e.start_datetime) AS [Year],
            MONTH(e.start_datetime) AS [Month],
            COUNT(1) AS [Count]
        FROM events AS e
        WHERE e.start_datetime >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1))
          AND (@CompanyId IS NULL OR e.company_id = @CompanyId OR e.scope_type = N'Global')
          AND (@AccessibleCompanyIds IS NULL OR e.company_id IS NULL OR e.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')))
        GROUP BY YEAR(e.start_datetime), MONTH(e.start_datetime)
        ORDER BY [Year], [Month];
        """;

    public const string UsersByCompany = """
        SELECT
            c.company_id AS CompanyId,
            c.company_name AS CompanyName,
            COUNT(DISTINCT u.user_id) AS [Count]
        FROM companies AS c
        LEFT JOIN user_company_roles AS ucr
            ON ucr.company_id = c.company_id
           AND ucr.is_active = 1
        LEFT JOIN users AS u
            ON u.user_id = ucr.user_id
           AND u.is_active = 1
        WHERE c.is_active = 1
          AND (@CompanyId IS NULL OR c.company_id = @CompanyId)
          AND (@AccessibleCompanyIds IS NULL OR c.company_id IN (SELECT value FROM STRING_SPLIT(@AccessibleCompanyIds, ',')))
        GROUP BY c.company_id, c.company_name
        ORDER BY c.company_name;
        """;
}
