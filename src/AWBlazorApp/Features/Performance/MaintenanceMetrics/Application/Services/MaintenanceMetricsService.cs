using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.MaintenanceMetrics.Application.Services;

public sealed class MaintenanceMetricsService(IDbContextFactory<ApplicationDbContext> dbFactory) : IMaintenanceMetricsService
{

    public async Task<MaintenanceMonthlyMetric> ComputeMonthlyAsync(int assetId, int year, int month, CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var wos = await db.MaintenanceWorkOrders.AsNoTracking()
            .Where(w => w.AssetId == assetId
                     && w.RaisedAt < monthEnd
                     && (w.StartedAt == null || w.StartedAt < monthEnd)
                     && (w.CompletedAt == null || w.CompletedAt >= monthStart))
            .Select(w => new { w.Type, w.Status, w.StartedAt, w.CompletedAt, w.PmScheduleId })
            .ToListAsync(cancellationToken);

        var woCount = wos.Count;
        var breakdowns = wos.Where(w => w.Type == WorkOrderType.Breakdown).ToList();
        var pmsScheduled = wos.Count(w => w.Type == WorkOrderType.Preventive);
        var pmsCompleted = wos.Count(w => w.Type == WorkOrderType.Preventive && w.Status == WorkOrderStatus.Completed);

        // MTBF: average time between breakdowns that occurred during this month.
        decimal? mtbfHours = null;
        if (breakdowns.Count >= 2)
        {
            var ordered = breakdowns
                .Where(b => b.StartedAt.HasValue)
                .Select(b => b.StartedAt!.Value)
                .OrderBy(d => d)
                .ToList();
            if (ordered.Count >= 2)
            {
                var deltas = new List<double>();
                for (var i = 1; i < ordered.Count; i++)
                    deltas.Add((ordered[i] - ordered[i - 1]).TotalHours);
                if (deltas.Count > 0) mtbfHours = (decimal)deltas.Average();
            }
        }

        // MTTR: average time from start to completion on repair WOs completed in this month.
        var repaired = wos
            .Where(w => (w.Type == WorkOrderType.Breakdown || w.Type == WorkOrderType.Corrective)
                     && w.StartedAt.HasValue && w.CompletedAt.HasValue
                     && w.CompletedAt >= monthStart && w.CompletedAt < monthEnd)
            .Select(w => (w.CompletedAt!.Value - w.StartedAt!.Value).TotalHours)
            .ToList();
        decimal? mttrHours = repaired.Count > 0 ? (decimal)repaired.Average() : null;

        // Availability: fraction of the month that the asset was NOT in breakdown.
        var monthHours = (decimal)(monthEnd - monthStart).TotalHours;
        decimal breakdownHours = 0m;
        foreach (var b in breakdowns)
        {
            if (!b.StartedAt.HasValue) continue;
            var start = b.StartedAt.Value < monthStart ? monthStart : b.StartedAt.Value;
            var end = (b.CompletedAt ?? monthEnd) > monthEnd ? monthEnd : (b.CompletedAt ?? monthEnd);
            if (end > start) breakdownHours += (decimal)(end - start).TotalHours;
        }
        var availability = monthHours > 0
            ? Math.Clamp((monthHours - breakdownHours) / monthHours, 0m, 1m)
            : (decimal?)null;

        decimal? pmCompliance = pmsScheduled > 0 ? (decimal)pmsCompleted / pmsScheduled : null;

        var now = DateTime.UtcNow;

        var existing = await db.MaintenanceMonthlyMetrics.FirstOrDefaultAsync(
            x => x.AssetId == assetId && x.Year == year && x.Month == month, cancellationToken);

        if (existing is null)
        {
            existing = new MaintenanceMonthlyMetric
            {
                AssetId = assetId, Year = year, Month = month,
                ModifiedDate = now,
            };
            db.MaintenanceMonthlyMetrics.Add(existing);
        }

        existing.WorkOrderCount = woCount;
        existing.BreakdownCount = breakdowns.Count;
        existing.PmWorkOrderCount = pmsScheduled;
        existing.PmCompletedCount = pmsCompleted;
        existing.MtbfHours = mtbfHours;
        existing.MttrHours = mttrHours;
        existing.AvailabilityFraction = availability;
        existing.PmComplianceFraction = pmCompliance;
        existing.ComputedAt = now;
        existing.ModifiedDate = now;

        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
