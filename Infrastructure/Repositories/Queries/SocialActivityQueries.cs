namespace Infrastructure.Repositories.Queries;



public static class SocialActivityQueries

{

    private const string BaseSelect = """

        SELECT

            t.social_activity_id,

            t.title,

            t.description,

            t.activity_type,

            t.location,

            t.start_at,

            t.end_at,

            t.image_url,

            t.company_scope,

            t.company_id,

            t.branch_scope,

            t.branch_id,

            t.department_scope,

            t.department_id,

            t.status,

            t.is_active,

            t.created_by,

            t.created_at,

            t.updated_at,

            co.company_name,
            b.branch_name,
            d.department_name
        FROM social_activities AS t
        LEFT JOIN companies AS co ON co.company_id = t.company_id
        LEFT JOIN branches AS b ON b.branch_id = t.branch_id
        LEFT JOIN departments AS d ON d.department_id = t.department_id
        """;



    public const string GetAll = BaseSelect + " ORDER BY t.start_at DESC;";



    public const string GetById = BaseSelect + " WHERE t.social_activity_id = @SocialActivityId;";



    public const string Create = """

        INSERT INTO social_activities

        (

            title, description, activity_type, location, start_at, end_at, image_url,

            company_scope, company_id, branch_scope, branch_id, department_scope, department_id,

            status, is_active, created_by

        )

        OUTPUT INSERTED.social_activity_id

        VALUES

        (

            @Title, @Description, @ActivityType, @Location, @StartAt, @EndAt, @ImageUrl,

            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,

            @Status, @IsActive, @CreatedBy

        );

        """;



    public const string Update = """

        UPDATE social_activities

        SET

            title = @Title,

            description = @Description,

            activity_type = @ActivityType,

            location = @Location,

            start_at = @StartAt,

            end_at = @EndAt,

            image_url = @ImageUrl,

            company_scope = @CompanyScope,

            company_id = @CompanyId,

            branch_scope = @BranchScope,

            branch_id = @BranchId,

            department_scope = @DepartmentScope,

            department_id = @DepartmentId,

            status = @Status,

            is_active = @IsActive,

            updated_at = SYSUTCDATETIME()

        WHERE social_activity_id = @SocialActivityId;

        """;



    public const string SoftDelete = """

        UPDATE social_activities

        SET is_active = 0, updated_at = SYSUTCDATETIME()

        WHERE social_activity_id = @SocialActivityId;

        """;

}


