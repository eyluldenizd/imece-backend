namespace Core.Auditing;

/// <summary>
/// Denetim olay kategorileri. Tek pipeline, farklı semantik.
/// </summary>
public static class AuditCategories
{
    public const string Request = "Request";
    public const string Mutation = "Mutation";
    public const string Error = "Error";
    public const string Security = "Security";
    public const string Sql = "Sql";
}

public static class AuditOutcomes
{
    public const string Success = "Success";
    public const string Failure = "Failure";
    public const string Denied = "Denied";
}

/// <summary>
/// Merkezî denetim olayı. Tüm otomatik kancalar bu modele yazar.
/// </summary>
public sealed class AuditEvent
{
    public required string Action { get; init; }

    public string Category { get; init; } = AuditCategories.Mutation;

    public string? Outcome { get; init; }

    public string? EntityType { get; init; }

    public string? EntityId { get; init; }

    public string? HttpMethod { get; init; }

    public string? RequestPath { get; init; }

    public int? StatusCode { get; init; }

    public long? DurationMs { get; init; }

    public string? ErrorCode { get; init; }

    public string? ExceptionType { get; init; }

    public object? Before { get; init; }

    public object? After { get; init; }

    public object? RequestBody { get; init; }
}

public interface IAuditRequestContext
{
    string? TraceId { get; }

    string? ClientIp { get; }

    string? UserAgent { get; }

    string? ClientApplication { get; }

    string? HttpMethod { get; }

    string? RequestPath { get; }
}

public interface IAuditValueSanitizer
{
    object? Sanitize(object? value);
}

/// <summary>
/// Uygulama kodunun kullandığı tek denetim API'si.
/// Yazım senkron veya arka plan kuyruğu üzerinden yapılır (options).
/// </summary>
public interface IAuditService
{
    Task WriteAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default);

    Task WriteAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? before = null,
        object? after = null,
        CancellationToken cancellationToken = default);
}
