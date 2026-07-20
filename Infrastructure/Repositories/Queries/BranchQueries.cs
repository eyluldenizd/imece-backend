namespace Infrastructure.Repositories.Queries;

public static class BranchQueries
{
    private const string SelectColumns = """
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
        ORDER BY branch_name ASC;
        """;

    public const string GetActive = $"""
        {SelectColumns}
        WHERE is_active = 1
        ORDER BY branch_name ASC;
        """;

    public const string GetByCompanyId = $"""
        {SelectColumns}
        WHERE company_id = @CompanyId
        ORDER BY branch_name ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
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
}
