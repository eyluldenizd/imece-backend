namespace Infrastructure.Repositories.Queries;



public static class ServicesQueries

{

    private const string SelectColumns = $"""

        SELECT

            t.service_id AS ServiceId,

            t.name AS Name,

            t.description AS Description,

            t.icon AS Icon,

            t.is_active AS IsActive,

            {OrganizationScopeSql.SelectColumns},

            {OrganizationScopeSql.ListNameColumns},

            t.created_at AS CreatedAt,

            t.updated_at AS UpdatedAt

        FROM services AS t

        {OrganizationScopeSql.ListJoins}

        """;



    public const string GetAll = $"{SelectColumns} ORDER BY t.created_at DESC;";

    public const string GetActive = $"{SelectColumns} WHERE t.is_active = 1 ORDER BY t.created_at DESC;";

    public const string GetById = $"""

        SELECT

            t.service_id AS ServiceId,

            t.name AS Name,

            t.description AS Description,

            t.icon AS Icon,

            t.is_active AS IsActive,

            {OrganizationScopeSql.SelectColumns},

            t.created_at AS CreatedAt,

            t.updated_at AS UpdatedAt

        FROM services AS t

        WHERE t.service_id = @ServiceId;

        """;

    public const string Create = """

        INSERT INTO services

        (

            name, description, icon, is_active,

            company_scope, company_id, branch_scope, branch_id, department_scope, department_id

        )

        OUTPUT INSERTED.service_id

        VALUES

        (

            @Name, @Description, @Icon, @IsActive,

            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId

        );

        """;

    public const string Update = """

        UPDATE services

        SET

            name = @Name,

            description = @Description,

            icon = @Icon,

            is_active = @IsActive,

            company_scope = @CompanyScope,

            company_id = @CompanyId,

            branch_scope = @BranchScope,

            branch_id = @BranchId,

            department_scope = @DepartmentScope,

            department_id = @DepartmentId,

            updated_at = SYSDATETIME()

        WHERE service_id = @ServiceId;

        """;

    /// <summary>Soft-delete: route passive; satır silinmez.</summary>

    public const string SoftDelete = "UPDATE services SET is_active = 0, updated_at = SYSDATETIME() WHERE service_id = @ServiceId;";

}


