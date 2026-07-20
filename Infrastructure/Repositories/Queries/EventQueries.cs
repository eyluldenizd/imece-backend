namespace Infrastructure.Repositories.Queries;



public static class EventQueries

{

    private const string SelectColumns = """

        SELECT

            event_id,

            company_id,

            scope_type,

            branch_scope,

            branch_id,

            department_scope,

            department_id,

            title,

            description,

            event_type,

            location,

            cover_image_url,

            start_datetime,

            end_datetime,

            is_all_day,

            created_by,

            created_at

        FROM events

        """;



    public const string GetAll = $"""

        {SelectColumns}

        WHERE {CompanyScopeSql.ListFilter}

        ORDER BY start_datetime ASC;

        """;



    public const string GetUpcoming = $"""

        {SelectColumns}

        WHERE end_datetime >= SYSDATETIME()

          AND {CompanyScopeSql.ListFilter}

        ORDER BY start_datetime ASC;

        """;



    public const string GetById = $"""

        {SelectColumns}

        WHERE event_id = @EventId;

        """;



    public const string Create = """

        INSERT INTO events

        (

            company_id,

            scope_type,

            branch_scope,

            branch_id,

            department_scope,

            department_id,

            title,

            description,

            event_type,

            location,

            cover_image_url,

            start_datetime,

            end_datetime,

            is_all_day,

            created_by

        )

        OUTPUT INSERTED.event_id

        VALUES

        (

            @CompanyId,

            @ScopeType,

            @BranchScope,

            @BranchId,

            @DepartmentScope,

            @DepartmentId,

            @Title,

            @Description,

            @EventType,

            @Location,

            @CoverImageUrl,

            @StartDateTime,

            @EndDateTime,

            @IsAllDay,

            @CreatedBy

        );

        """;



    public const string Update = """

        UPDATE events

        SET

            company_id = @CompanyId,

            scope_type = @ScopeType,

            branch_scope = @BranchScope,

            branch_id = @BranchId,

            department_scope = @DepartmentScope,

            department_id = @DepartmentId,

            title = @Title,

            description = @Description,

            event_type = @EventType,

            location = @Location,

            cover_image_url = @CoverImageUrl,

            start_datetime = @StartDateTime,

            end_datetime = @EndDateTime,

            is_all_day = @IsAllDay

        WHERE event_id = @EventId;

        """;



    public const string Delete = """

        DELETE FROM events

        WHERE event_id = @EventId;

        """;

}


