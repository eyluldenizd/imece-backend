namespace Application.DTOs;

public sealed class AuditLogDto
{
    public long AuditId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Outcome { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? TraceId { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public string? ClientApplication { get; set; }
    public string? HttpMethod { get; set; }
    public string? RequestPath { get; set; }
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
    public string? ErrorCode { get; set; }
    public string? ExceptionType { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? RequestBodyJson { get; set; }
}

public sealed class AuditLogListQueryDto
{
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? UserName { get; set; }
    public string? Category { get; set; }
    public string? Outcome { get; set; }
    public string? EntityType { get; set; }
    public string? ClientIp { get; set; }

    /// <summary>occurredAt | userName | action</summary>
    public string? SortBy { get; set; }

    /// <summary>asc | desc</summary>
    public string? SortDir { get; set; }

    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

public sealed class AuditLogFilterOptionsDto
{
    public IReadOnlyList<string> Users { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EntityTypes { get; init; } = Array.Empty<string>();
}
