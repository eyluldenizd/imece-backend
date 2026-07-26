namespace Infrastructure.Repositories.Queries;

public static class ContentCompanyTargetQueries
{
    public const string GetByContent = """
        SELECT company_id AS CompanyId
        FROM content_company_targets
        WHERE content_type = @ContentType AND content_id = @ContentId
        ORDER BY company_id;
        """;

    public const string GetByContentType = """
        SELECT content_id AS ContentId, company_id AS CompanyId
        FROM content_company_targets
        WHERE content_type = @ContentType;
        """;

    public const string DeleteByContent = """
        DELETE FROM content_company_targets
        WHERE content_type = @ContentType AND content_id = @ContentId;
        """;

    public const string Insert = """
        INSERT INTO content_company_targets (content_type, content_id, company_id)
        VALUES (@ContentType, @ContentId, @CompanyId);
        """;
}
