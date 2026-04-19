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
    /// parameters on RunAsync let a manual call backfill a different date or a date range.
    /// </summary>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => RunAsync(targetDate: null, days: null, idealCycleSeconds: null, cancellationToken);

    /// <summary>
    /// Real entry point — used by both the recurring job (with nulls) and the manual admin
    /// trigger (with explicit overrides). When <paramref name="days"/> &gt; 1, rolls up a
    /// range ending on <paramref name="targetDate"/> (inclusive) and going back N-1 days.
    /// Maintenance monthly rollups fire for every distinct (year, month) covered by the
    /// range — useful for populating a fresh deploy with several months of history in one call.
    /// </summary>
    public async Task<MetricsRollupResult> RunAsync(
        DateOnly? targetDate,
        int? days,
        decimal? idealCycleSeconds,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var endDate = targetDate ?? DateOnly.FromDateTime(nowUtc.AddDays(-1));
        var rangeDays = days ?? 1;
        if (rangeDays < 1) rangeDays = 1;
        if (rangeDays > 365) rangeDays = 365; // sanity cap so a typo can't hammer the DB
        var startDate = endDate.AddDays(-(rangeDays - 1));
        var fallbackIdeal = idealCycleSeconds ?? DefaultIdealCycleSeconds;

        var result = new MetricsRollupResult
        {
            FromDate = startDate,
            ToDate = endDate,
            DaysCovered = rangeDays,
        };

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // Pull stations with their per-station IdealCycleSeconds (falls back to the supplied
        // ideal — usually 60s — when the station hasn't been tuned).
        var activeStations = await db.Stations.AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new { s.Id, s.IdealCycleSeconds })
            .ToListAsync(cancellationToken);

        // ── Stations: OEE + production daily — looped over each day in the range ────
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            var periodStart = DateTime.SpecifyKind(d.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var periodEnd = periodStart.AddDays(1);

            foreach (var st in activeStations)
            {
                var ideal = st.IdealCycleSeconds ?? fallbackIdeal;
                try
                {
                    await _oee.ComputeAsync(st.Id, PerformancePeriodKind.Day, periodStart, periodEnd, ideal, cancellationToken);
                    result.OeeRollups++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "OEE rollup failed for station {StationId} on {Date}", st.Id, d);
                    result.Failures++;
                }

                try
                {
                    await _production.ComputeDailyAsync(st.Id, d, cancellationToken);
                    result.ProductionRollups++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Production daily rollup failed for station {StationId} on {Date}", st.Id, d);
                    result.Failures++;
                }
            }
        }

        // ── Maintenance monthly metrics ─────────────────────────────────────
        // Recurring (single-day) call: only fires on the 1st, and rolls up the prior month.
        // Backfill (multi-day) call: fires for every distinct prior-month covered by the
        //   range. e.g. days=60 ending 2026-04-19 covers months that complete on
        //   2026-03-01 (Feb) and 2026-04-01 (Mar) — both get rolled up.
        var monthsToRoll = new HashSet<(int Year, int Month)>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            // For each first-of-month in the range, the month that just completed is (d - 1 month).
            if (d.Day == 1)
            {
                var prior = d.AddMonths(-1);
                monthsToRoll.Add((prior.Year, prior.Month));
            }
        }
        // Recurring case: if no explicit date and today is the 1st, also fire for the prior month.
        if (targetDate is null && nowUtc.Day == 1)
        {
            var prior = DateOnly.FromDateTime(nowUtc).AddMonths(-1);
            monthsToRoll.Add((prior.Year, prior.Month));
        }

        if (monthsToRoll.Count > 0)
        {
            var activeAssetIds = await db.Assets.AsNoTracking()
                .Where(a => a.Status == AssetStatus.Active)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            foreach (var (year, month) in monthsToRoll.OrderBy(m => m.Year).ThenBy(m => m.Month))
            {
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
                result.MaintenanceMonthsRolledUp.Add($"{year:D4}-{month:D2}");
            }
        }

        _logger.LogInformation(
            "Metrics rollup {From}..{To} ({Days}d): oee={Oee} prod={Prod} maint={Maint} months={Months} failures={Failures}",
            startDate, endDate, rangeDays,
            result.OeeRollups, result.ProductionRollups, result.MaintenanceRollups,
            result.MaintenanceMonthsRolledUp.Count, result.Failures);

        return result;
    }
}

public sealed class MetricsRollupResult
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public int DaysCovered { get; set; }
    public int OeeRollups { get; set; }
    public int ProductionRollups { get; set; }
    public int MaintenanceRollups { get; set; }
    public List<string> MaintenanceMonthsRolledUp { get; set; } = new();
    public int Failures { get; set; }
    public int Total => OeeRollups + ProductionRollups + MaintenanceRollups;
}
