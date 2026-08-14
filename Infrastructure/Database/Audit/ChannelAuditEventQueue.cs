using System.Threading.Channels;
using Infrastructure.Database.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Audit;

public sealed class ChannelAuditEventQueue : IAuditEventQueue
{
    private readonly Channel<AuditLogEntry> _channel;
    private readonly ILogger<ChannelAuditEventQueue> _logger;

    public ChannelAuditEventQueue(
        IOptions<AuditOptions> options,
        ILogger<ChannelAuditEventQueue> logger)
    {
        _logger = logger;
        var capacity = Math.Max(64, options.Value.QueueCapacity);
        _channel = Channel.CreateBounded<AuditLogEntry>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (_channel.Writer.TryWrite(entry))
        {
            return ValueTask.CompletedTask;
        }

        _logger.LogWarning(
            "Audit kuyruğu dolu; kayıt düşürüldü: {Action} {EntityType}/{EntityId}",
            entry.Action,
            entry.EntityType,
            entry.EntityId);

        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<AuditLogEntry> DequeueAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var entry in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return entry;
        }
    }
}
