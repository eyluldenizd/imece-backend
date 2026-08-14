using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Audit;

/// <summary>
/// Audit kuyruğunu boşaltıp SQL writer'a yazar. İstek thread'ini bloklamaz.
/// </summary>
public sealed class AuditLogBackgroundWorker : BackgroundService
{
    private readonly IAuditEventQueue _queue;
    private readonly IAuditLogWriter _writer;
    private readonly ILogger<AuditLogBackgroundWorker> _logger;

    public AuditLogBackgroundWorker(
        IAuditEventQueue queue,
        IAuditLogWriter writer,
        ILogger<AuditLogBackgroundWorker> logger)
    {
        _queue = queue;
        _writer = writer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var entry in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await _writer.WriteAsync(entry, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(
                    ex,
                    "Arka plan audit yazımı başarısız: {Action} {EntityType}/{EntityId}",
                    entry.Action,
                    entry.EntityType,
                    entry.EntityId);
            }
        }
    }
}
