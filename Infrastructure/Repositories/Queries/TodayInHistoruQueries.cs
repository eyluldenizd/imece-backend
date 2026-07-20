namespace Infrastructure.Queries;

public static class TodayInHistoryQueries
{
    private const string SelectColumns = """
        SELECT
            id,
            event_date,
            title,
            description,
            image_url,
            company_scope,
            company_id,
            branch_scope,
            branch_id,
            department_scope,
            department_id,
            created_at
        FROM today_in_history
        """;

    public const string GetAll = SelectColumns + " ORDER BY event_date DESC;";

    public const string Create = """
        INSERT INTO today_in_history
        (
            event_date, title, description, image_url,
            company_scope, company_id, branch_scope, branch_id, department_scope, department_id,
            created_at
        )
        VALUES
        (
            @EventDate, @Title, @Description, @ImageUrl,
            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,
            GETDATE()
        );
        """;

    public const string Update = """
        UPDATE today_in_history
        SET
            event_date = @EventDate,
            title = @Title,
            description = @Description,
            image_url = @ImageUrl,
            company_scope = @CompanyScope,
            company_id = @CompanyId,
            branch_scope = @BranchScope,
            branch_id = @BranchId,
            department_scope = @DepartmentScope,
            department_id = @DepartmentId
        WHERE id = @Id;
        """;

    public const string Delete = """
        DELETE FROM today_in_history
        WHERE id = @Id;
        """;
}
