using System.Text.Json;
using Core.Auditing;
using Core.Authorization;
using Infrastructure.Database.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogWriter _writer;
    private readonly IAuditEventQueue _queue;
    private readonly IAuditValueSanitizer _sanitizer;
    private readonly IOptions<AuditOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogWriter writer,
        IAuditEventQueue queue,
        IAuditValueSanitizer sanitizer,
        IOptions<AuditOptions> options,
        IServiceProvider serviceProvider,
        ILogger<AuditService> logger)
    {
        _writer = writer;
        _queue = queue;
        _sanitizer = sanitizer;
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task WriteAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? before = null,
        object? after = null,
        CancellationToken cancellationToken = default) =>
        WriteAsync(
            new AuditEvent
            {
                Action = action,
                Category = AuditCategories.Mutation,
                EntityType = entityType,
                EntityId = entityId,
                Before = before,
                After = after
            },
            cancellationToken);

    public async Task WriteAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        if (!options.Enabled)
        {
            return;
        }

        try
        {
            var requestContext = _serviceProvider.GetService<IAuditRequestContext>();
            var currentUser = _serviceProvider.GetService<ICurrentUser>();
            var companyContext = _serviceProvider.GetService<ICompanyContext>();

            var entry = new AuditLogEntry
            {
                Action = Truncate(auditEvent.Action, 128) ?? "Unknown",
                Category = Truncate(auditEvent.Category, 32),
                Outcome = Truncate(auditEvent.Outcome, 32),
                EntityType = Truncate(auditEvent.EntityType, 128),
                EntityId = Truncate(auditEvent.EntityId, 128),
                UserId = currentUser?.UserId,
                CompanyId = companyContext?.CompanyId ?? companyContext?.CurrentCompanyId,
                TraceId = Truncate(requestContext?.TraceId, 128),
                ClientIp = Truncate(requestContext?.ClientIp, 64),
                UserAgent = Truncate(requestContext?.UserAgent, 512),
                ClientApplication = Truncate(requestContext?.ClientApplication, 64),
                HttpMethod = Truncate(auditEvent.HttpMethod ?? requestContext?.HttpMethod, 16),
                RequestPath = Truncate(auditEvent.RequestPath ?? requestContext?.RequestPath, 512),
                StatusCode = auditEvent.StatusCode,
                DurationMs = auditEvent.DurationMs,
                ErrorCode = Truncate(auditEvent.ErrorCode, 64),
                ExceptionType = Truncate(auditEvent.ExceptionType, 256),
                BeforeJson = ToJson(_sanitizer.Sanitize(auditEvent.Before)),
                AfterJson = ToJson(_sanitizer.Sanitize(auditEvent.After)),
                RequestBodyJson = ToJson(_sanitizer.Sanitize(auditEvent.RequestBody))
            };

            if (options.UseBackgroundQueue)
            {
                await _queue.EnqueueAsync(entry, cancellationToken);
                return;
            }

            await _writer.WriteAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            if (options.ContentFailureMode == AuditContentFailureMode.FailClosed)
            {
                throw;
            }

            _logger.LogWarning(
                ex,
                "Audit yazımı başarısız (FailOpen). Action={Action} Entity={EntityType}/{EntityId}",
                auditEvent.Action,
                auditEvent.EntityType,
                auditEvent.EntityId);
        }
    }

    private static string? ToJson(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value);

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
