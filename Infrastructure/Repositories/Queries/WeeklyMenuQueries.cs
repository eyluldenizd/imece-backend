namespace Infrastructure.Repositories.Queries;

public static class WeeklyMenuQueries
{
    private const string SelectColumns = """
        SELECT
            wm.menu_id AS MenuId,
            wm.company_id AS CompanyId,
            wm.branch_id AS BranchId,
            co.company_name AS CompanyName,
            b.branch_name AS BranchName,
            wm.menu_code AS MenuCode,
            wm.year AS Year,
            wm.month AS Month,
            wm.week_of_month AS WeekOfMonth,
            wm.period_start_date AS PeriodStartDate,
            wm.period_end_date AS PeriodEndDate,
            wm.title AS Title,
            wm.is_published AS IsPublished,
            wm.published_at AS PublishedAt,
            wm.is_active AS IsActive,
            wm.created_by AS CreatedBy,
            wm.created_at AS CreatedAt,
            wm.updated_at AS UpdatedAt
        FROM weekly_menus AS wm
        LEFT JOIN companies AS co ON co.company_id = wm.company_id
        LEFT JOIN branches AS b ON b.branch_id = wm.branch_id
        """;

    public const string GetAll = """
        SELECT
            wm.menu_id AS MenuId,
            wm.company_id AS CompanyId,
            wm.branch_id AS BranchId,
            co.company_name AS CompanyName,
            b.branch_name AS BranchName,
            wm.menu_code AS MenuCode,
            wm.year AS Year,
            wm.month AS Month,
            wm.week_of_month AS WeekOfMonth,
            wm.period_start_date AS PeriodStartDate,
            wm.period_end_date AS PeriodEndDate,
            wm.title AS Title,
            wm.is_published AS IsPublished,
            wm.published_at AS PublishedAt,
            wm.is_active AS IsActive,
            wm.created_by AS CreatedBy,
            wm.created_at AS CreatedAt,
            wm.updated_at AS UpdatedAt
        FROM weekly_menus AS wm
        LEFT JOIN companies AS co ON co.company_id = wm.company_id
        LEFT JOIN branches AS b ON b.branch_id = wm.branch_id
        WHERE wm.is_active = 1
          AND (
            (@CompanyId IS NULL AND @AccessibleCompanyIds IS NULL)
            OR (@CompanyId IS NOT NULL AND wm.company_id = @CompanyId)
            OR (
                @AccessibleCompanyIds IS NOT NULL
                AND wm.company_id IN (
                    SELECT TRY_CAST(LTRIM(RTRIM(value)) AS INT)
                    FROM STRING_SPLIT(@AccessibleCompanyIds, ',')
                )
            )
          )
        ORDER BY wm.year DESC, wm.month DESC, wm.week_of_month DESC, wm.menu_id DESC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE wm.menu_id = @MenuId AND wm.is_active = 1;
        """;

    public const string GetByCompanyBranchAndCode = $"""
        {SelectColumns}
        WHERE wm.company_id = @CompanyId
          AND wm.branch_id = @BranchId
          AND wm.menu_code = @MenuCode
          AND wm.is_active = 1;
        """;

    public const string Create = """
        INSERT INTO weekly_menus (
            company_id,
            branch_id,
            menu_code,
            year,
            month,
            week_of_month,
            period_start_date,
            period_end_date,
            title,
            is_published,
            created_by,
            is_active
        )
        OUTPUT INSERTED.menu_id
        VALUES (
            @CompanyId,
            @BranchId,
            @MenuCode,
            @Year,
            @Month,
            @WeekOfMonth,
            @PeriodStartDate,
            @PeriodEndDate,
            @Title,
            0,
            @CreatedBy,
            1
        );
        """;

    public const string Update = """
        UPDATE weekly_menus
        SET title = @Title,
            company_id = @CompanyId,
            branch_id = @BranchId,
            updated_at = SYSDATETIME()
        WHERE menu_id = @MenuId AND is_active = 1;
        """;

    public const string Publish = """
        UPDATE weekly_menus
        SET is_published = 1,
            published_at = SYSDATETIME(),
            updated_at = SYSDATETIME()
        WHERE menu_id = @MenuId AND is_active = 1;
        """;

    public const string Unpublish = """
        UPDATE weekly_menus
        SET is_published = 0,
            published_at = NULL,
            updated_at = SYSDATETIME()
        WHERE menu_id = @MenuId AND is_active = 1;
        """;

    public const string SoftDelete = """
        UPDATE weekly_menus
        SET is_active = 0, updated_at = SYSDATETIME()
        WHERE menu_id = @MenuId;
        """;

    public const string Delete = "DELETE FROM weekly_menus WHERE menu_id = @MenuId;";
}
