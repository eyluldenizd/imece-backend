namespace Infrastructure.Database.Audit;

public interface IAuditLogWriter
{
    Task WriteAsync(
        AuditLogEntry entry,
        CancellationToken cancellationToken = default);
}

public sealed class AuditLogEntry
{
    public required string Action { get; init; }

    public string? EntityType { get; init; }

    public string? EntityId { get; init; }

    public int? UserId { get; init; }

    public int? CompanyId { get; init; }

    public string? TraceId { get; init; }

    public string? ClientIp { get; init; }

    public string? UserAgent { get; init; }

    public string? ClientApplication { get; init; }

    public string? BeforeJson { get; init; }

    public string? AfterJson { get; init; }
}
