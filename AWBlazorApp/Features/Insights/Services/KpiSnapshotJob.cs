using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Features.Insights.Services;
using AWBlazorApp.Features.Insights.Domain;
using AWBlazorApp.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Insights.Services;

/// <summary>
/// Hourly Hangfire job: runs every SavedQuery flagged IsKpi and appends a KpiSnapshot row with
/// the scalar result. The /kpis dashboard reads the last ~90 days of snapshots for the sparkline.
/// </summary>
public sealed class KpiSnapshotJob(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    SavedQueryRunner runner,
    ILogger<KpiSnapshotJob> logger)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var queries = await db.SavedQueries.Where(q => q.IsKpi).ToListAsync(ct);
        if (queries.Count == 0) return;

        var now = DateTime.UtcNow;
        var snapshots = new List<KpiSnapshot>(queries.Count);

        foreach (var q in queries)
        {
            try
            {
                // Force a scalar read regardless of the saved GroupBy so the sparkline has a
                // consistent single-value-per-timestamp shape.
                var scalarQuery = new SavedQuery
                {
                    Metric = q.Metric,
                    GroupBy = QueryGroupBy.None,
                    FromDate = q.FromDate,
                    ToDate = q.ToDate,
                };
                var result = await runner.RunAsync(scalarQuery, ct);
                snapshots.Add(new KpiSnapshot
                {
                    SavedQueryId = q.Id,
                    Timestamp = now,
                    Value = result.Scalar,
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "KPI snapshot failed for SavedQuery {QueryId} ({Name})", q.Id, q.Name);
            }
        }

        if (snapshots.Count > 0)
        {
            db.KpiSnapshots.AddRange(snapshots);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Recorded {Count} KPI snapshots", snapshots.Count);
        }
    }
}
