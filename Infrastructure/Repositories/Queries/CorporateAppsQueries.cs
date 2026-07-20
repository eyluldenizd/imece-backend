namespace Infrastructure.Repositories.Queries;

public static class CorporateAppsQueries
{
    private const string SelectColumns = """
        SELECT
            a.app_id AS AppId,
            a.title AS Title,
            a.description AS Description,
            a.url AS Url,
            a.corporate_app_category_id AS CorporateAppCategoryId,
            c.name AS CategoryName,
            a.category AS Category,
            a.icon_url AS IconUrl,
            a.is_active AS IsActive,
            a.company_scope AS CompanyScope,
            a.company_id AS CompanyId,
            a.branch_scope AS BranchScope,
            a.branch_id AS BranchId,
            a.department_scope AS DepartmentScope,
            a.department_id AS DepartmentId
        FROM corporate_apps AS a
        LEFT JOIN corporate_app_categories AS c
            ON c.corporate_app_category_id = a.corporate_app_category_id
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY a.title ASC;";

    public const string GetById = $"{SelectColumns} WHERE a.app_id = @AppId;";

    public const string Create = """
        INSERT INTO corporate_apps
        (
            title, description, url, corporate_app_category_id, category, icon_url, is_active,
            company_scope, company_id, branch_scope, branch_id,
            department_scope, department_id
        )
        OUTPUT INSERTED.app_id
        VALUES
        (
            @Title, @Description, @Url, @CorporateAppCategoryId, @Category, @IconUrl, @IsActive,
            @CompanyScope, @CompanyId, @BranchScope, @BranchId,
            @DepartmentScope, @DepartmentId
        );
        """;

    public const string Update = """
        UPDATE corporate_apps
        SET
            title = @Title,
            description = @Description,
            url = @Url,
            corporate_app_category_id = @CorporateAppCategoryId,
            category = @Category,
            icon_url = @IconUrl,
            is_active = @IsActive,
            company_scope = @CompanyScope,
            company_id = @CompanyId,
            branch_scope = @BranchScope,
            branch_id = @BranchId,
            department_scope = @DepartmentScope,
            department_id = @DepartmentId
        WHERE app_id = @AppId;
        """;

    public const string SoftDelete = """
        UPDATE corporate_apps
        SET is_active = 0
        WHERE app_id = @AppId;
        """;
}
