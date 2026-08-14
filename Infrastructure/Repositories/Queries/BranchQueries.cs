namespace Infrastructure.Repositories.Queries;

public static class BranchQueries
{
    private const string SelectColumns = """
        SELECT
            b.branch_id AS BranchId,
            b.company_id AS CompanyId,
            c.company_name AS CompanyName,
            b.branch_code AS BranchCode,
            b.branch_name AS BranchName,
            b.description AS Description,
            b.address AS Address,
            b.latitude AS Latitude,
            b.longitude AS Longitude,
            b.is_active AS IsActive,
            b.created_at AS CreatedAt,
            b.updated_at AS UpdatedAt
        FROM branches AS b
        LEFT JOIN companies AS c ON c.company_id = b.company_id
        """;

    private const string EntitySelectColumns = """
        SELECT
            branch_id,
            company_id,
            branch_code,
            branch_name,
            description,
            address,
            latitude,
            longitude,
            is_active,
            created_at,
            updated_at
        FROM branches
        """;

    public const string GetAll = $"""
        {SelectColumns}
        ORDER BY b.branch_name ASC;
        """;

    public const string GetActive = $"""
        {SelectColumns}
        WHERE b.is_active = 1
        ORDER BY b.branch_name ASC;
        """;

    public const string GetByCompanyId = $"""
        {SelectColumns}
        WHERE b.company_id = @CompanyId
          AND b.is_active = 1
        ORDER BY b.branch_name ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE b.branch_id = @BranchId;
        """;

    public const string GetEntityById = $"""
        {EntitySelectColumns}
        WHERE branch_id = @BranchId;
        """;

    public const string ExistsByCodeInCompany = """
        SELECT COUNT(1)
        FROM branches
        WHERE company_id = @CompanyId
          AND branch_code = @BranchCode
          AND (@ExcludeBranchId IS NULL OR branch_id <> @ExcludeBranchId);
        """;

    public const string Create = """
        INSERT INTO branches
        (
            company_id,
            branch_code,
            branch_name,
            description,
            address,
            latitude,
            longitude,
            is_active,
            created_at,
            updated_at
        )
        OUTPUT INSERTED.branch_id
        VALUES
        (
            @CompanyId,
            @BranchCode,
            @BranchName,
            @Description,
            @Address,
            @Latitude,
            @Longitude,
            @IsActive,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
        """;

    public const string Update = """
        UPDATE branches
        SET
            company_id = @CompanyId,
            branch_code = @BranchCode,
            branch_name = @BranchName,
            description = @Description,
            address = @Address,
            latitude = @Latitude,
            longitude = @Longitude,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE branch_id = @BranchId;
        """;

    public const string SoftDelete = """
        UPDATE branches
        SET
            is_active = 0,
            updated_at = SYSUTCDATETIME()
        WHERE branch_id = @BranchId
          AND is_active = 1;
        """;

    public const string Delete = "DELETE FROM branches WHERE branch_id = @BranchId;";
}
