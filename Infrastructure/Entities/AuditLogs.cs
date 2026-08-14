using Infrastructure.Data;

namespace Infrastructure.Entities;

/// <summary>
/// Read model for dbo.audit_log (+ optional user/company display joins).
/// </summary>
public sealed class AuditLogs
{
    [DbManager.DbColumn("audit_id")]
    public long AuditId { get; set; }

    [DbManager.DbColumn("occurred_at")]
    public DateTime OccurredAt { get; set; }

    [DbManager.DbColumn("action")]
    public string Action { get; set; } = string.Empty;

    [DbManager.DbColumn("category")]
    public string? Category { get; set; }

    [DbManager.DbColumn("outcome")]
    public string? Outcome { get; set; }

    [DbManager.DbColumn("entity_type")]
    public string? EntityType { get; set; }

    [DbManager.DbColumn("entity_id")]
    public string? EntityId { get; set; }

    [DbManager.DbColumn("user_id")]
    public int? UserId { get; set; }

    [DbManager.DbColumn("user_name")]
    public string? UserName { get; set; }

    [DbManager.DbColumn("company_id")]
    public int? CompanyId { get; set; }

    [DbManager.DbColumn("company_name")]
    public string? CompanyName { get; set; }

    [DbManager.DbColumn("trace_id")]
    public string? TraceId { get; set; }

    [DbManager.DbColumn("client_ip")]
    public string? ClientIp { get; set; }

    [DbManager.DbColumn("user_agent")]
    public string? UserAgent { get; set; }

    [DbManager.DbColumn("client_application")]
    public string? ClientApplication { get; set; }

    [DbManager.DbColumn("http_method")]
    public string? HttpMethod { get; set; }

    [DbManager.DbColumn("request_path")]
    public string? RequestPath { get; set; }

    [DbManager.DbColumn("status_code")]
    public int? StatusCode { get; set; }

    [DbManager.DbColumn("duration_ms")]
    public long? DurationMs { get; set; }

    [DbManager.DbColumn("error_code")]
    public string? ErrorCode { get; set; }

    [DbManager.DbColumn("exception_type")]
    public string? ExceptionType { get; set; }

    [DbManager.DbColumn("before_json")]
    public string? BeforeJson { get; set; }

    [DbManager.DbColumn("after_json")]
    public string? AfterJson { get; set; }

    [DbManager.DbColumn("request_body_json")]
    public string? RequestBodyJson { get; set; }
}
