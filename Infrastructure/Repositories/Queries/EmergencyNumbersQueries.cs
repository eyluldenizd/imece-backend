using Infrastructure.Repositories.Queries;

namespace Infrastructure.Queries;

public static class EmergencyNumberQueries
{
    /// <summary>
    /// Prefer snake_case result names matching [DbColumn]; PascalCase aliases also work once
    /// SqlDataAccess maps both property and DbColumn names.
    /// </summary>
    private const string SelectColumns = $"""
        SELECT
            t.emergency_number_id,
            t.name,
            t.phone_number,
            t.emergency_number_category_id,
            c.name AS category_name,
            t.category,
            t.description,
            t.is_active,
            t.display_order,
            t.company_scope,
            t.company_id,
            t.branch_scope,
            t.branch_id,
            t.department_scope,
            t.department_id,
            {OrganizationScopeSql.ListNameColumns},
            t.created_at,
            t.updated_at
        FROM emergency_numbers AS t
        LEFT JOIN emergency_number_categories AS c
            ON c.emergency_number_category_id = t.emergency_number_category_id
        {OrganizationScopeSql.ListJoins}
        """;

    public const string GetAll = SelectColumns + $" WHERE {OrganizationScopeSql.ListFilter} ORDER BY t.display_order ASC, t.name ASC;";

    public const string GetById = SelectColumns + " WHERE t.emergency_number_id = @EmergencyNumberId;";

    public const string Create = """
        INSERT INTO emergency_numbers
        (
            name, phone_number, emergency_number_category_id, category, description, is_active, display_order,
            company_scope, company_id, branch_scope, branch_id, department_scope, department_id,
            created_at
        )
        VALUES
        (
            @Name, @PhoneNumber, @EmergencyNumberCategoryId, @Category, @Description, @IsActive, @DisplayOrder,
            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,
            GETDATE()
        );
        """;

    public const string Update = """
        UPDATE emergency_numbers
        SET
            name = @Name,
            phone_number = @PhoneNumber,
            emergency_number_category_id = @EmergencyNumberCategoryId,
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
