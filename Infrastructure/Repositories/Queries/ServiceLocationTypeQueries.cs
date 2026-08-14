namespace Infrastructure.Repositories.Queries;

public static class ServiceLocationTypeQueries
{
    private const string SelectColumns = $"""
        SELECT
            t.service_location_type_id AS ServiceLocationTypeId,
            t.name AS Name,
            t.description AS Description,
            t.icon_url AS IconUrl,
            t.color_key AS ColorKey,
            t.sort_order AS SortOrder,
            t.is_active AS IsActive,
            {OrganizationScopeSql.SelectColumns},
            {OrganizationScopeSql.ListNameColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_location_types AS t
        {OrganizationScopeSql.ListJoins}
        """;

    public static readonly string GetAll = $"{SelectColumns} WHERE {OrganizationScopeSql.ListFilter} ORDER BY t.sort_order ASC, t.name ASC;";

    public const string GetById = $"""
        SELECT
            t.service_location_type_id AS ServiceLocationTypeId,
            t.name AS Name,
            t.description AS Description,
            t.icon_url AS IconUrl,
            t.color_key AS ColorKey,
            t.sort_order AS SortOrder,
            t.is_active AS IsActive,
            {OrganizationScopeSql.SelectColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_location_types AS t
        WHERE t.service_location_type_id = @ServiceLocationTypeId;
        """;

    public const string GetByNameInCompany = """
        SELECT
            t.service_location_type_id AS ServiceLocationTypeId,
            t.name AS Name,
            t.description AS Description,
            t.icon_url AS IconUrl,
            t.color_key AS ColorKey,
            t.sort_order AS SortOrder,
            t.is_active AS IsActive,
            t.company_scope AS CompanyScope,
            t.company_id AS CompanyId,
            t.branch_scope AS BranchScope,
            t.branch_id AS BranchId,
            t.department_scope AS DepartmentScope,
            t.department_id AS DepartmentId,
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM service_location_types AS t
        WHERE t.name = @Name
          AND (
                (@CompanyId IS NULL AND t.company_id IS NULL)
             OR t.company_id = @CompanyId
          );
        """;

    public const string Create = """
        INSERT INTO service_location_types
            (company_scope, company_id, branch_scope, branch_id, department_scope, department_id,
             name, description, icon_url, color_key, sort_order, is_active)
        OUTPUT INSERTED.service_location_type_id
        VALUES
            (@CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,
             @Name, @Description, @IconUrl, @ColorKey, @SortOrder, @IsActive);
        """;

    public const string Update = """
        UPDATE service_location_types
        SET company_scope = @CompanyScope,
            company_id = @CompanyId,
            branch_scope = @BranchScope,
            branch_id = @BranchId,
            department_scope = @DepartmentScope,
            department_id = @DepartmentId,
            name = @Name,
            description = @Description,
            icon_url = @IconUrl,
            color_key = @ColorKey,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE service_location_type_id = @ServiceLocationTypeId;
        """;

    public const string SoftDelete = """
        UPDATE service_location_types
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE service_location_type_id = @ServiceLocationTypeId;
        """;

    public const string Delete = "DELETE FROM service_location_types WHERE service_location_type_id = @ServiceLocationTypeId;";
}
