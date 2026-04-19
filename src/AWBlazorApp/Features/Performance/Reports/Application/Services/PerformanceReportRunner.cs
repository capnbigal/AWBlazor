using System.Diagnostics;
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Dtos; using AWBlazorApp.Features.Performance.ProductionMetrics.Dtos; using AWBlazorApp.Features.Performance.Scorecards.Dtos; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Reports.Application.Services;

public interface IPerformanceReportRunner
{
    /// <summary>
    /// Runs the saved report against the live data warehouse and returns its rows in a
    /// generic column/value shape so the UI can render every kind without per-kind branches.
    /// Side effects: updates <see cref="PerformanceReport.LastRunAt"/> and inserts a
    /// <see cref="PerformanceReportRun"/> row capturing the duration / row count / errors.
    /// </summary>
    Task<PerformanceReportResultDto> RunAsync(int reportId, string? runByUserId, CancellationToken ct);
}

public sealed class PerformanceReportRunner : IPerformanceReportRunner
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<PerformanceReportRunner> _logger;

    public PerformanceReportRunner(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogger<PerformanceReportRunner> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<PerformanceReportResultDto> RunAsync(int reportId, string? runByUserId, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var report = await db.PerformanceReports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
            ?? throw new KeyNotFoundException($"PerformanceReport #{reportId} not found.");

        var (rangeStart, rangeEnd) = ResolveRange(report.RangePreset, DateTime.UtcNow);
        string? errorMessage = null;
        IReadOnlyList<PerformanceReportColumnDto> columns = Array.Empty<PerformanceReportColumnDto>();
        IReadOnlyList<IReadOnlyList<string>> rows = Array.Empty<IReadOnlyList<string>>();

        try
        {
            var (cols, data) = report.Kind switch
            {
                PerformanceReportKind.OeeSummary => await BuildOeeSummaryAsync(db, report, rangeStart, rangeEnd, ct),
                PerformanceReportKind.MaintenanceScorecard => await BuildMaintenanceScorecardAsync(db, report, rangeStart, rangeEnd, ct),
                PerformanceReportKind.ProductionTrend => await BuildProductionTrendAsync(db, report, rangeStart, rangeEnd, ct),
                _ => throw new NotSupportedException($"Report kind '{report.Kind}' is not yet supported by the runner."),
            };
            columns = cols;
            rows = data;
        }
        catch (Exception ex)
        {
            // Capture the error so it shows up in the run history, but still throw so the
            // caller can surface it to the user. The finally block records the failed run.
            errorMessage = ex.Message;
            _logger.LogError(ex, "Report {ReportCode} failed to run.", report.Code);
            throw;
        }
        finally
        {
            sw.Stop();
            // Persist the run row even on failure — the history view shows red rows for
            // failures so users can debug. LastRunAt only advances on success.
            db.PerformanceReportRuns.Add(new PerformanceReportRun
            {
                PerformanceReportId = report.Id,
                RunAt = DateTime.UtcNow,
                RunByUserId = runByUserId,
                RowCount = rows.Count,
                DurationMs = (int)sw.ElapsedMilliseconds,
                ResultJson = null, // results aren't archived — re-run if you need them again
                ErrorMessage = errorMessage,
                ModifiedDate = DateTime.UtcNow,
            });
            if (errorMessage is null)
            {
                report.LastRunAt = DateTime.UtcNow;
                report.ModifiedDate = DateTime.UtcNow;
            }
            await db.SaveChangesAsync(ct);
        }

        return new PerformanceReportResultDto(
            ReportId: report.Id,
            ReportCode: report.Code,
            ReportName: report.Name,
            Kind: report.Kind,
            RangeStart: rangeStart,
            RangeEnd: rangeEnd,
            Columns: columns,
            Rows: rows,
            GeneratedAt: DateTime.UtcNow,
            DurationMs: (int)sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Maps a rolling preset to a concrete (start, end) UTC pair. End is exclusive: a
    /// "Last 7 days" run on 2026-04-19 returns rows where PeriodStart &lt; 2026-04-19 00:00 UTC
    /// and &gt;= 2026-04-12 00:00 UTC. This matches the bin convention used by the rollup job.
    /// </summary>
    public static (DateTime Start, DateTime End) ResolveRange(ReportRangePreset preset, DateTime nowUtc)
    {
        var today = DateTime.SpecifyKind(nowUtc.Date, DateTimeKind.Utc);
        return preset switch
        {
            ReportRangePreset.Last7Days => (today.AddDays(-7), today),
            ReportRangePreset.Last30Days => (today.AddDays(-30), today),
            ReportRangePreset.Last90Days => (today.AddDays(-90), today),
            ReportRangePreset.ThisWeek => (today.AddDays(-(int)today.DayOfWeek), today.AddDays(1)),
            ReportRangePreset.ThisMonth => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), today.AddDays(1)),
            ReportRangePreset.LastMonth => LastMonthRange(today),
            ReportRangePreset.YearToDate => (new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), today.AddDays(1)),
            _ => (today.AddDays(-7), today),
        };

        static (DateTime, DateTime) LastMonthRange(DateTime today)
        {
            var firstOfThisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstOfLastMonth = firstOfThisMonth.AddMonths(-1);
            return (firstOfLastMonth, firstOfThisMonth);
        }
    }

    /// <summary>
    /// OEE per station for the range — averages availability/performance/quality and OEE,
    /// sums units produced and scrapped. If <see cref="PerformanceReport.StationId"/> is
    /// set, restricts the result to that one station; otherwise returns one row per station
    /// that has at least one snapshot in the window.
    /// </summary>
    private static async Task<(IReadOnlyList<PerformanceReportColumnDto>, IReadOnlyList<IReadOnlyList<string>>)>
        BuildOeeSummaryAsync(ApplicationDbContext db, PerformanceReport report, DateTime start, DateTime end, CancellationToken ct)
    {
        var q = db.OeeSnapshots.AsNoTracking()
            .Where(s => s.PeriodKind == PerformancePeriodKind.Day
                     && s.PeriodStart >= start && s.PeriodStart < end);
        if (report.StationId.HasValue) q = q.Where(s => s.StationId == report.StationId.Value);

        // Pull rows then aggregate in memory — the snapshot table is small after rollup
        // (one row per station per day) and EF can't translate Average() over decimals
        // grouped by a foreign key without a tracker subquery here.
        var raw = await q.Select(s => new { s.StationId, s.Availability, s.Performance, s.Quality, s.Oee, s.UnitsProduced, s.UnitsScrapped }).ToListAsync(ct);
        var stations = await db.Stations.AsNoTracking()
            .Where(st => raw.Select(r => r.StationId).Distinct().Contains(st.Id))
            .Select(st => new { st.Id, st.Code, st.Name })
            .ToListAsync(ct);
        var nameById = stations.ToDictionary(x => x.Id, x => $"{x.Code} — {x.Name}");

        var rows = raw.GroupBy(r => r.StationId)
            .OrderBy(g => g.Key)
            .Select(g => (IReadOnlyList<string>)new[]
            {
                g.Key.ToString(),
                nameById.TryGetValue(g.Key, out var n) ? n : $"Station #{g.Key}",
                g.Count().ToString("N0"),
                Pct(g.Average(x => x.Availability)),
                Pct(g.Average(x => x.Performance)),
                Pct(g.Average(x => x.Quality)),
                Pct(g.Average(x => x.Oee)),
                g.Sum(x => x.UnitsProduced).ToString("N0"),
                g.Sum(x => x.UnitsScrapped).ToString("N0"),
            })
            .ToList();

        var cols = new[]
        {
            new PerformanceReportColumnDto("station_id", "Station ID", "int"),
            new PerformanceReportColumnDto("station", "Station", "string"),
            new PerformanceReportColumnDto("days", "Days", "int"),
            new PerformanceReportColumnDto("availability", "Availability", "string"),
            new PerformanceReportColumnDto("performance", "Performance", "string"),
            new PerformanceReportColumnDto("quality", "Quality", "string"),
            new PerformanceReportColumnDto("oee", "OEE", "string"),
            new PerformanceReportColumnDto("units_produced", "Units produced", "int"),
            new PerformanceReportColumnDto("units_scrapped", "Units scrapped", "int"),
        };
        return (cols, rows);
    }

    /// <summary>
    /// Maintenance metrics per asset for the range. Aggregates monthly rollups (the only
    /// granularity MaintenanceMonthlyMetric carries) within the window. If
    /// <see cref="PerformanceReport.AssetId"/> is set, restricts the result to that one asset.
    /// </summary>
    private static async Task<(IReadOnlyList<PerformanceReportColumnDto>, IReadOnlyList<IReadOnlyList<string>>)>
        BuildMaintenanceScorecardAsync(ApplicationDbContext db, PerformanceReport report, DateTime start, DateTime end, CancellationToken ct)
    {
        // Months whose 1st falls within the window.
        var startKey = start.Year * 100 + start.Month;
        var endKey = end.Year * 100 + end.Month;

        var q = db.MaintenanceMonthlyMetrics.AsNoTracking()
            .Where(m => (m.Year * 100 + m.Month) >= startKey && (m.Year * 100 + m.Month) <= endKey);
        if (report.AssetId.HasValue) q = q.Where(m => m.AssetId == report.AssetId.Value);

        var raw = await q.Select(m => new { m.AssetId, m.Year, m.Month, m.WorkOrderCount, m.BreakdownCount, m.PmWorkOrderCount, m.PmCompletedCount, m.MtbfHours, m.MttrHours, m.AvailabilityFraction, m.PmComplianceFraction }).ToListAsync(ct);
        var assets = await db.Assets.AsNoTracking()
            .Where(a => raw.Select(r => r.AssetId).Distinct().Contains(a.Id))
            .Select(a => new { a.Id, a.AssetTag, a.Name })
            .ToListAsync(ct);
        var nameById = assets.ToDictionary(x => x.Id, x => $"{x.AssetTag} — {x.Name}");

        var rows = raw.GroupBy(r => r.AssetId)
            .OrderBy(g => g.Key)
            .Select(g => (IReadOnlyList<string>)new[]
            {
                g.Key.ToString(),
                nameById.TryGetValue(g.Key, out var n) ? n : $"Asset #{g.Key}",
                g.Count().ToString("N0"),
                g.Sum(x => x.WorkOrderCount).ToString("N0"),
                g.Sum(x => x.BreakdownCount).ToString("N0"),
                g.Sum(x => x.PmWorkOrderCount).ToString("N0"),
                g.Sum(x => x.PmCompletedCount).ToString("N0"),
                g.Average(x => x.MtbfHours ?? 0m).ToString("F1"),
                g.Average(x => x.MttrHours ?? 0m).ToString("F2"),
                Pct(g.Average(x => x.AvailabilityFraction ?? 0m)),
                Pct(g.Average(x => x.PmComplianceFraction ?? 0m)),
            })
            .ToList();

        var cols = new[]
        {
            new PerformanceReportColumnDto("asset_id", "Asset ID", "int"),
            new PerformanceReportColumnDto("asset", "Asset", "string"),
            new PerformanceReportColumnDto("months", "Months", "int"),
            new PerformanceReportColumnDto("wo_total", "WOs", "int"),
            new PerformanceReportColumnDto("wo_breakdown", "Breakdowns", "int"),
            new PerformanceReportColumnDto("pm_total", "PM WOs", "int"),
            new PerformanceReportColumnDto("pm_completed", "PM completed", "int"),
            new PerformanceReportColumnDto("mtbf_avg", "MTBF (hrs)", "decimal"),
            new PerformanceReportColumnDto("mttr_avg", "MTTR (hrs)", "decimal"),
            new PerformanceReportColumnDto("availability_avg", "Availability", "string"),
            new PerformanceReportColumnDto("pm_compliance_avg", "PM compliance", "string"),
        };
        return (cols, rows);
    }

    /// <summary>
    /// Daily production trend — one row per day in the window, station-filtered when set.
    /// Sums units produced, scrapped, and runs across all stations on each date.
    /// </summary>
    private static async Task<(IReadOnlyList<PerformanceReportColumnDto>, IReadOnlyList<IReadOnlyList<string>>)>
        BuildProductionTrendAsync(ApplicationDbContext db, PerformanceReport report, DateTime start, DateTime end, CancellationToken ct)
    {
        var startDate = DateOnly.FromDateTime(start);
        var endDate = DateOnly.FromDateTime(end);

        var q = db.ProductionDailyMetrics.AsNoTracking()
            .Where(m => m.Date >= startDate && m.Date < endDate);
        if (report.StationId.HasValue) q = q.Where(m => m.StationId == report.StationId.Value);

        var raw = await q.Select(m => new { m.Date, m.UnitsProduced, m.UnitsScrapped, m.RunCount, m.YieldFraction }).ToListAsync(ct);

        var rows = raw.GroupBy(m => m.Date)
            .OrderBy(g => g.Key)
            .Select(g => (IReadOnlyList<string>)new[]
            {
                g.Key.ToString("yyyy-MM-dd"),
                g.Sum(x => x.UnitsProduced).ToString("N0"),
                g.Sum(x => x.UnitsScrapped).ToString("N0"),
                g.Sum(x => x.RunCount).ToString("N0"),
                Pct(g.Average(x => x.YieldFraction ?? 0m)),
            })
            .ToList();

        var cols = new[]
        {
            new PerformanceReportColumnDto("date", "Date", "date"),
            new PerformanceReportColumnDto("units_produced", "Units produced", "int"),
            new PerformanceReportColumnDto("units_scrapped", "Units scrapped", "int"),
            new PerformanceReportColumnDto("runs", "Runs", "int"),
            new PerformanceReportColumnDto("yield_avg", "Yield (avg)", "string"),
        };
        return (cols, rows);
    }

    private static string Pct(decimal fraction) => $"{fraction * 100m:F1}%";
}
