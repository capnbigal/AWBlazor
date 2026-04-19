using AWBlazorApp.Features.Enterprise.Domain;
using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Features.Performance.Services;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Jobs;

/// <summary>
/// Nightly Hangfire recurring job that pre-computes the periodic metric snapshots so the
/// Performance pages render real numbers without anyone having to click "Compute".
///
/// Runs once a day (registered at 02:00 UTC). For every active station it computes:
///   - <see cref="OeeSnapshot"/> for yesterday (Day period)
///   - <see cref="ProductionDailyMetric"/> for yesterday
/// And on the 1st of the month it additionally computes, for every active asset:
///   - <see cref="MaintenanceMonthlyMetric"/> for the previous month
///
/// All compute calls use the existing services and are upsert (idempotent) — re-running
/// the job for a date that's already rolled up is safe.
///
/// Also exposed via <c>POST /api/admin/run-metrics-rollup</c> for one-shot manual runs
/// (testing, backfill).
/// </summary>
public sealed class MetricsRollupJob
{
    /// <summary>Default ideal cycle assumed when a station doesn't carry an explicit value yet.</summary>
    private const decimal DefaultIdealCycleSeconds = 60m;

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IOeeService _oee;
    private readonly IProductionMetricsService _production;
    private readonly IMaintenanceMetricsService _maintenance;
    private readonly ILogger<MetricsRollupJob> _logger;

    public MetricsRollupJob(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IOeeService oee,
        IProductionMetricsService production,
        IMaintenanceMetricsService maintenance,
        ILogger<MetricsRollupJob> logger)
    {
        _dbFactory = dbFactory;
        _oee = oee;
        _production = production;
        _maintenance = maintenance;
        _logger = logger;
    }

    /// <summary>
    /// Hangfire entry point — rolls up "yesterday" relative to UTC now. The optional
    /// parameters let a manual call backfill a different date.
    /// </summary>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => RunAsync(targetDate: null, idealCycleSeconds: null, cancellationToken);

    /// <summary>
    /// Real entry point — used by both the recurring job (with nulls) and the manual admin
    /// trigger (with explicit overrides).
    /// </summary>
    public async Task<MetricsRollupResult> RunAsync(
        DateOnly? targetDate,
        decimal? idealCycleSeconds,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var date = targetDate ?? DateOnly.FromDateTime(nowUtc.AddDays(-1));
        var ideal = idealCycleSeconds ?? DefaultIdealCycleSeconds;

        var result = new MetricsRollupResult { Date = date };

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // ── Stations: OEE + production daily ────────────────────────────────
        var activeStationIds = await db.Stations.AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var periodStart = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var periodEnd = periodStart.AddDays(1);

        foreach (var stationId in activeStationIds)
        {
            try
            {
                await _oee.ComputeAsync(stationId, PerformancePeriodKind.Day, periodStart, periodEnd, ideal, cancellationToken);
                result.OeeRollups++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OEE rollup failed for station {StationId} on {Date}", stationId, date);
                result.Failures++;
            }

            try
            {
                await _production.ComputeDailyAsync(stationId, date, cancellationToken);
                result.ProductionRollups++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Production daily rollup failed for station {StationId} on {Date}", stationId, date);
                result.Failures++;
            }
        }

        // ── On the 1st: Assets monthly maintenance metrics for the previous month ────
        // Backfill case: when a target date is passed, run the maintenance rollup if the
        // target is the first of a month so callers can backfill on demand.
        var runMaintenance = (date.Day == 1) || (targetDate is null && nowUtc.Day == 1);
        if (runMaintenance)
        {
            // Previous month relative to the target date.
            var monthDate = date.AddMonths(-1);
            var year = monthDate.Year;
            var month = monthDate.Month;

            var activeAssetIds = await db.Assets.AsNoTracking()
                .Where(a => a.Status == AssetStatus.Active)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            foreach (var assetId in activeAssetIds)
            {
                try
                {
                    await _maintenance.ComputeMonthlyAsync(assetId, year, month, cancellationToken);
                    result.MaintenanceRollups++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Maintenance monthly rollup failed for asset {AssetId} {Year}-{Month:D2}",
                        assetId, year, month);
                    result.Failures++;
                }
            }
            result.MaintenanceMonthRolledUp = $"{year:D4}-{month:D2}";
        }

        _logger.LogInformation(
            "Metrics rollup for {Date}: oee={Oee} prod={Prod} maint={Maint} failures={Failures}",
            date, result.OeeRollups, result.ProductionRollups, result.MaintenanceRollups, result.Failures);

        return result;
    }
}

public sealed class MetricsRollupResult
{
    public DateOnly Date { get; set; }
    public int OeeRollups { get; set; }
    public int ProductionRollups { get; set; }
    public int MaintenanceRollups { get; set; }
    public string? MaintenanceMonthRolledUp { get; set; }
    public int Failures { get; set; }
    public int Total => OeeRollups + ProductionRollups + MaintenanceRollups;
}
