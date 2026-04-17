namespace AWBlazorApp.Features.Inventory.Services;

/// <summary>
/// Pluggable outbound transport for the inventory JSON envelope. Default implementation logs
/// and marks the row published — AS2, webhook, Kafka, etc. can implement this later without
/// touching the service or the outbox emitter. Throw on failure; the emitter handles retry
/// and dead-letter bookkeeping.
/// </summary>
public interface IInventoryOutboxPublisher
{
    Task PublishAsync(long outboxId, string payload, CancellationToken cancellationToken);
}

/// <summary>
/// No-op publisher. Logs the payload at Information level and returns. Used when no real
/// transport is configured — keeps the outbox flowing so the audit/history UI still works.
/// </summary>
public sealed class LoggingInventoryOutboxPublisher(ILogger<LoggingInventoryOutboxPublisher> logger)
    : IInventoryOutboxPublisher
{
    public Task PublishAsync(long outboxId, string payload, CancellationToken cancellationToken)
    {
        logger.LogInformation("Outbox {Id} published (payload length {Length})", outboxId, payload.Length);
        return Task.CompletedTask;
    }
}
