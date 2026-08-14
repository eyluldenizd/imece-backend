namespace Infrastructure.Database.Audit;

public interface IAuditLogWriter
{
    Task WriteAsync(
        AuditLogEntry entry,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Kalıcı denetim satırı. HTTP/domain bağımsız; writer yalnız bunu bilir.
/// </summary>
public sealed class AuditLogEntry
{
    public required string Action { get; init; }

    public string? Category { get; init; }

    public string? Outcome { get; init; }

    public string? EntityType { get; init; }

    public string? EntityId { get; init; }

    public int? UserId { get; init; }

    public int? CompanyId { get; init; }

    public string? TraceId { get; init; }

    public string? ClientIp { get; init; }

    public string? UserAgent { get; init; }

    public string? ClientApplication { get; init; }

    public string? HttpMethod { get; init; }

    public string? RequestPath { get; init; }

    public int? StatusCode { get; init; }

    public long? DurationMs { get; init; }

    public string? ErrorCode { get; init; }

    public string? ExceptionType { get; init; }

    public string? BeforeJson { get; init; }

    public string? AfterJson { get; init; }

    public string? RequestBodyJson { get; init; }
}

/// <summary>
/// Arka plan kuyruğu. Writer'ı istek thread'inden ayırır.
/// </summary>
public interface IAuditEventQueue
{
    ValueTask EnqueueAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);

    IAsyncEnumerable<AuditLogEntry> DequeueAllAsync(CancellationToken cancellationToken);
}
