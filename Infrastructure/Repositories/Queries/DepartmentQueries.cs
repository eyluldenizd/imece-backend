namespace Infrastructure.Repositories.Queries;

public static class DepartmentQueries
{
    private const string SelectColumns = """
        SELECT
            d.department_id AS DepartmentId,
            d.branch_id AS BranchId,
            b.company_id AS CompanyId,
            d.parent_department_id AS ParentDepartmentId,
            d.department_code AS DepartmentCode,
            d.department_name AS DepartmentName,
            d.description AS Description,
            d.is_active AS IsActive,
            d.created_at AS CreatedAt,
            d.updated_at AS UpdatedAt
        FROM departments AS d
        LEFT JOIN branches AS b ON b.branch_id = d.branch_id
        """;

    public const string GetAll = $"""
        {SelectColumns}
        ORDER BY d.department_name ASC;
        """;

    public const string GetActive = $"""
        {SelectColumns}
        WHERE d.is_active = 1
        ORDER BY d.department_name ASC;
        """;

    public const string GetByBranchId = $"""
        {SelectColumns}
        WHERE d.branch_id = @BranchId
        ORDER BY d.department_name ASC;
        """;

    public const string GetByCompanyId = $"""
        {SelectColumns}
        WHERE b.company_id = @CompanyId
        ORDER BY d.department_name ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE d.department_id = @DepartmentId;
        """;

    public const string ExistsByCodeInBranch = """
        SELECT COUNT(1)
        FROM departments
        WHERE branch_id = @BranchId
          AND department_code = @DepartmentCode
          AND (@ExcludeDepartmentId IS NULL OR department_id <> @ExcludeDepartmentId);
        """;

    public const string Create = """
        INSERT INTO departments
        (
            branch_id,
            parent_department_id,
            department_code,
            department_name,
            description,
            is_active,
            created_at,
            updated_at
        )
        OUTPUT INSERTED.department_id
        VALUES
        (
            @BranchId,
            @ParentDepartmentId,
            @DepartmentCode,
            @DepartmentName,
            @Description,
            @IsActive,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
        """;

    public const string Update = """
        UPDATE departments
        SET
            branch_id = @BranchId,
            parent_department_id = @ParentDepartmentId,
            department_code = @DepartmentCode,
            department_name = @DepartmentName,
            description = @Description,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE department_id = @DepartmentId;
        """;

    public const string SoftDelete = """
        UPDATE departments
        SET
            is_active = 0,
            updated_at = SYSUTCDATETIME()
        WHERE department_id = @DepartmentId
          AND is_active = 1;
        """;
}
