namespace Infrastructure.Queries;



public static class EmergencyNumberQueries

{

    private const string SelectColumns = """

        SELECT

            emergency_number_id,

            name,

            phone_number,

            category,

            description,

            is_active,

            display_order,

            company_scope,

            company_id,

            branch_scope,

            branch_id,

            department_scope,

            department_id,

            created_at,

            updated_at

        FROM emergency_numbers

        """;



    public const string GetAll = SelectColumns + " ORDER BY display_order ASC, name ASC;";



    public const string GetById = SelectColumns + " WHERE emergency_number_id = @EmergencyNumberId;";



    public const string Create = """

        INSERT INTO emergency_numbers

        (

            name, phone_number, category, description, is_active, display_order,

            company_scope, company_id, branch_scope, branch_id, department_scope, department_id,

            created_at

        )

        VALUES

        (

            @Name, @PhoneNumber, @Category, @Description, @IsActive, @DisplayOrder,

            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,

            GETDATE()

        );

        """;



    public const string Update = """

        UPDATE emergency_numbers

        SET

            name = @Name,

            phone_number = @PhoneNumber,

            category = @Category,

            description = @Description,

            is_active = @IsActive,

            display_order = @DisplayOrder,

            company_scope = @CompanyScope,

            company_id = @CompanyId,

            branch_scope = @BranchScope,

            branch_id = @BranchId,

            department_scope = @DepartmentScope,

            department_id = @DepartmentId,

            updated_at = GETDATE()

        WHERE emergency_number_id = @EmergencyNumberId;

        """;



    public const string SoftDelete = """

        UPDATE emergency_numbers

        SET is_active = 0, updated_at = GETDATE()

        WHERE emergency_number_id = @EmergencyNumberId;

        """;

}


