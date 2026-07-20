namespace Infrastructure.Repositories.Queries;

public static class CompanyQueries
{
    private const string SelectColumns = """
        SELECT
            company_id,
            company_code,
            company_name,
            description,
            logo_url,
            is_active,
            created_at,
            updated_at
        FROM companies
        """;

    public const string GetAll = $"""
        {SelectColumns}
        ORDER BY company_name ASC;
        """;

    public const string GetActive = $"""
        {SelectColumns}
        WHERE is_active = 1
        ORDER BY company_name ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE company_id = @CompanyId;
        """;

    public const string GetCompanyNameById = """
        SELECT TOP 1 company_name
        FROM companies
        WHERE company_id = @CompanyId;
        """;

    public const string ExistsByCode = """
        SELECT COUNT(1)
        FROM companies
        WHERE company_code = @CompanyCode
          AND (@ExcludeCompanyId IS NULL OR company_id <> @ExcludeCompanyId);
        """;

    public const string Create = """
        INSERT INTO companies
        (
            company_code,
            company_name,
            description,
            logo_url,
            is_active,
            created_at,
            updated_at
        )
        OUTPUT INSERTED.company_id
        VALUES
        (
            @CompanyCode,
            @CompanyName,
            @Description,
            @LogoUrl,
            @IsActive,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        );
        """;

    public const string Update = """
        UPDATE companies
        SET
            company_code = @CompanyCode,
            company_name = @CompanyName,
            description = @Description,
            logo_url = @LogoUrl,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE company_id = @CompanyId;
        """;

    public const string SoftDelete = """
        UPDATE companies
        SET
            is_active = 0,
            updated_at = SYSUTCDATETIME()
        WHERE company_id = @CompanyId
          AND is_active = 1;
        """;
}
