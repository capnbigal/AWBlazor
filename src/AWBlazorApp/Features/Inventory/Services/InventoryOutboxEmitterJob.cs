using AWBlazorApp.Features.Inventory.Dtos;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Inventory.Services;

/// <summary>
/// Hangfire recurring job that drains <c>inv.InventoryTransactionOutbox</c>. Picks up Pending
/// rows whose <c>NextAttemptAt</c> is past, hands them to <see cref="IInventoryOutboxPublisher"/>,
/// and flips to Published on success. On failure: bumps <c>Attempts</c>, schedules exponential
/// backoff in <c>NextAttemptAt</c>, and moves to <see cref="OutboxStatus.DeadLetter"/> after
/// <see cref="MaxAttempts"/> retries. Registered every minute in <c>MiddlewarePipeline</c>.
/// </summary>
public sealed class InventoryOutboxEmitterJob(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IInventoryOutboxPublisher publisher,
    ILogger<InventoryOutboxEmitterJob> logger)
{
    public const int MaxAttempts = 5;
    public const int BatchSize = 50;

    [AutomaticRetry(Attempts = 0)] // we own the retry policy ourselves via NextAttemptAt
    public async Task ExecuteAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;

        var batch = await db.InventoryTransactionOutbox
            .Where(o => (o.Status == OutboxStatus.Pending || o.Status == OutboxStatus.Failed)
                        && (o.NextAttemptAt == null || o.NextAttemptAt <= now))
            .OrderBy(o => o.CreatedAt)
            .Take(BatchSize)
            .ToListAsync();

        if (batch.Count == 0) return;

        logger.LogInformation("InventoryOutboxEmitter: draining {Count} rows", batch.Count);

        foreach (var row in batch)
        {
            try
            {
                row.Status = OutboxStatus.Publishing;
                await db.SaveChangesAsync();

                await publisher.PublishAsync(row.Id, row.Payload, CancellationToken.None);

                row.Status = OutboxStatus.Published;
                row.PublishedAt = DateTime.UtcNow;
                row.LastError = null;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                row.Attempts++;
                row.LastError = ex.Message;
                if (row.Attempts >= MaxAttempts)
                {
                    row.Status = OutboxStatus.DeadLetter;
                    row.NextAttemptAt = null;
                    logger.LogWarning(ex, "Outbox {Id} moved to DeadLetter after {Attempts} attempts", row.Id, row.Attempts);
                }
                else
                {
                    row.Status = OutboxStatus.Failed;
                    row.NextAttemptAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, row.Attempts) * 30);
                    logger.LogWarning(ex, "Outbox {Id} failed (attempt {Attempts}); next try at {Next}",
                        row.Id, row.Attempts, row.NextAttemptAt);
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
