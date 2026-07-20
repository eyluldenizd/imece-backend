namespace Core.Auditing;

public interface IAuditRequestContext
{
    string? TraceId { get; }

    string? ClientIp { get; }

    string? UserAgent { get; }

    string? ClientApplication { get; }
}

public interface IAuditValueSanitizer
{
    object? Sanitize(object? value);
}

public interface IAuditService
{
    Task WriteAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? before = null,
        object? after = null,
        CancellationToken cancellationToken = default);
}
