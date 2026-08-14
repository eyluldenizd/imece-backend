namespace Infrastructure.Repositories.Queries;

public static class AuditLogQueries
{
    private const string SelectColumns = """
        SELECT
            a.audit_id,
            a.occurred_at,
            a.action,
            a.category,
            a.outcome,
            a.entity_type,
            a.entity_id,
            a.user_id,
            u.full_name AS user_name,
            a.company_id,
            c.company_name AS company_name,
            a.trace_id,
            a.client_ip,
            a.user_agent,
            a.client_application,
            a.http_method,
            a.request_path,
            a.status_code,
            a.duration_ms,
            a.error_code,
            a.exception_type,
            a.before_json,
            a.after_json,
            a.request_body_json
        FROM audit_log AS a
        LEFT JOIN users AS u ON u.user_id = a.user_id
        LEFT JOIN companies AS c ON c.company_id = a.company_id
        """;

    public const string FilterPredicate = """
        WHERE
            (@Search IS NULL OR (
                a.action LIKE @SearchPattern
                OR a.entity_type LIKE @SearchPattern
                OR a.entity_id LIKE @SearchPattern
                OR a.request_path LIKE @SearchPattern
                OR a.trace_id LIKE @SearchPattern
                OR a.error_code LIKE @SearchPattern
                OR a.client_ip LIKE @SearchPattern
                OR u.full_name LIKE @SearchPattern
            ))
            AND (@DateFrom IS NULL OR a.occurred_at >= @DateFrom)
            AND (@DateToExclusive IS NULL OR a.occurred_at < @DateToExclusive)
            AND (@UserName IS NULL OR u.full_name = @UserName)
            AND (@Category IS NULL OR a.category = @Category)
            AND (@Outcome IS NULL OR a.outcome = @Outcome)
            AND (@EntityType IS NULL OR a.entity_type = @EntityType)
            AND (@ClientIp IS NULL OR a.client_ip LIKE @ClientIpPattern)
        """;

    public const string CountFiltered = $"""
        SELECT COUNT(1)
        FROM audit_log AS a
        LEFT JOIN users AS u ON u.user_id = a.user_id
        {FilterPredicate};
        """;

    /// <summary>OrderBy clause is appended by the repository from a whitelist.</summary>
    public const string GetPagedPrefix = $"""
        {SelectColumns}
        {FilterPredicate}
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE a.audit_id = @AuditId;
        """;

    public const string DistinctCategories = """
        SELECT DISTINCT category AS value
        FROM audit_log
        WHERE category IS NOT NULL AND LTRIM(RTRIM(category)) <> ''
        ORDER BY value ASC;
        """;

    public const string DistinctEntityTypes = """
        SELECT DISTINCT entity_type AS value
        FROM audit_log
        WHERE entity_type IS NOT NULL AND LTRIM(RTRIM(entity_type)) <> ''
        ORDER BY value ASC;
        """;

    public const string DistinctUserNames = """
        SELECT DISTINCT u.full_name AS value
        FROM audit_log AS a
        INNER JOIN users AS u ON u.user_id = a.user_id
        WHERE u.full_name IS NOT NULL AND LTRIM(RTRIM(u.full_name)) <> ''
        ORDER BY value ASC;
        """;
}
