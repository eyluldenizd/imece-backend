namespace Infrastructure.Repositories.Queries;

public static class WeeklyMenuQueries
{
    private const string SelectColumns = """
        SELECT
            menu_id AS MenuId,
            company_id AS CompanyId,
            menu_code AS MenuCode,
            year AS Year,
            month AS Month,
            week_of_month AS WeekOfMonth,
            period_start_date AS PeriodStartDate,
            period_end_date AS PeriodEndDate,
            title AS Title,
            is_published AS IsPublished,
            published_at AS PublishedAt,
            is_active AS IsActive,
            created_by AS CreatedBy,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM weekly_menus
        """;

    public const string GetAll = $"""
        {SelectColumns}
        WHERE is_active = 1
          AND {CompanyScopeSql.CompanyOnlyListFilter}
        ORDER BY year DESC, month DESC, week_of_month DESC, menu_id DESC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE menu_id = @MenuId AND is_active = 1;
        """;

    public const string GetByCompanyAndCode = $"""
        {SelectColumns}
        WHERE company_id = @CompanyId
          AND menu_code = @MenuCode
          AND is_active = 1;
        """;

    public const string Create = """
        INSERT INTO weekly_menus (
            company_id,
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
}
