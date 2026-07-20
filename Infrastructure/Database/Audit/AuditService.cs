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
    private readonly IAuditValueSanitizer _sanitizer;
    private readonly IOptions<AuditOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogWriter writer,
        IAuditValueSanitizer sanitizer,
        IOptions<AuditOptions> options,
        IServiceProvider serviceProvider,
        ILogger<AuditService> logger)
    {
        _writer = writer;
        _sanitizer = sanitizer;
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task WriteAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? before = null,
        object? after = null,
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
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = currentUser?.UserId,
                CompanyId = companyContext?.CompanyId ?? companyContext?.CurrentCompanyId,
                TraceId = requestContext?.TraceId,
                ClientIp = requestContext?.ClientIp,
                UserAgent = requestContext?.UserAgent,
                ClientApplication = requestContext?.ClientApplication,
                BeforeJson = ToJson(_sanitizer.Sanitize(before)),
                AfterJson = ToJson(_sanitizer.Sanitize(after))
            };

            await _writer.WriteAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            if (options.ContentFailureMode == AuditContentFailureMode.FailClosed)
            {
                throw;
            }

            _logger.LogWarning(ex,
                "Audit yazımı başarısız (FailOpen). Action={Action} Entity={EntityType}/{EntityId}",
                action, entityType, entityId);
        }
    }

    private static string? ToJson(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value);
}
