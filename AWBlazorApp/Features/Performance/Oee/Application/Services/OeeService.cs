using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Oee.Application.Services;

public sealed class OeeService : IOeeService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public OeeService(IDbContextFactory<ApplicationDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<OeeSnapshot> ComputeAsync(
        int stationId, PerformancePeriodKind periodKind,
        DateTime periodStart, DateTime periodEnd,
        decimal idealCycleSeconds, CancellationToken cancellationToken)
    {
        if (periodEnd <= periodStart)
            throw new ArgumentException("periodEnd must be after periodStart.", nameof(periodEnd));

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // Production runs that overlap the period AT this station.
        var runs = await db.ProductionRuns.AsNoTracking()
            .Where(r => r.StationId == stationId
                     && r.ActualStartAt != null
                     && r.ActualStartAt < periodEnd
                     && (r.ActualEndAt == null || r.ActualEndAt > periodStart))
            .Select(r => new { r.ActualStartAt, r.ActualEndAt, r.QuantityProduced, r.QuantityScrapped })
            .ToListAsync(cancellationToken);

        // Clip each run to the period and sum runtime minutes.
        decimal actualRuntimeMinutes = 0m;
        decimal unitsProduced = 0m;
        decimal unitsScrapped = 0m;
        foreach (var r in runs)
        {
            var start = r.ActualStartAt!.Value < periodStart ? periodStart : r.ActualStartAt.Value;
            var end = (r.ActualEndAt ?? periodEnd) > periodEnd ? periodEnd : (r.ActualEndAt ?? periodEnd);
            if (end > start)
                actualRuntimeMinutes += (decimal)(end - start).TotalMinutes;
            unitsProduced += r.QuantityProduced;
            unitsScrapped += r.QuantityScrapped;
        }

        // Downtime at this station overlapping the period.
        var downtimes = await db.DowntimeEvents.AsNoTracking()
            .Where(d => d.StationId == stationId
                     && d.StartAt < periodEnd
                     && (d.EndAt == null || d.EndAt > periodStart))
            .Select(d => new { d.StartAt, d.EndAt })
            .ToListAsync(cancellationToken);

        decimal downtimeMinutes = 0m;
        foreach (var d in downtimes)
        {
            var start = d.StartAt < periodStart ? periodStart : d.StartAt;
            var end = (d.EndAt ?? periodEnd) > periodEnd ? periodEnd : (d.EndAt ?? periodEnd);
            if (end > start)
                downtimeMinutes += (decimal)(end - start).TotalMinutes;
        }

        var plannedRuntimeMinutes = actualRuntimeMinutes + downtimeMinutes;

        var availability = plannedRuntimeMinutes > 0
            ? Math.Clamp(actualRuntimeMinutes / plannedRuntimeMinutes, 0m, 1m)
            : 0m;

        // Performance = (good cycles × ideal) / actual runtime seconds.
        var performance = 0m;
        if (actualRuntimeMinutes > 0 && idealCycleSeconds > 0)
        {
            var actualSeconds = actualRuntimeMinutes * 60m;
            var idealTotal = unitsProduced * idealCycleSeconds;
            performance = Math.Clamp(idealTotal / actualSeconds, 0m, 1m);
        }

        // Quality: units produced minus scrap, divided by units produced.
        // Quality also pulls in quality-module failures as "scrap" when they reference this station.
        var qualityFailures = await db.Inspections.AsNoTracking()
            .Where(i => i.InspectedAt != null
                     && i.InspectedAt >= periodStart && i.InspectedAt < periodEnd
                     && i.Status == InspectionStatus.Fail)
            .CountAsync(cancellationToken);

        var effectiveScrap = unitsScrapped + qualityFailures;
        var quality = unitsProduced > 0
            ? Math.Clamp((unitsProduced - effectiveScrap) / unitsProduced, 0m, 1m)
            : 0m;

        var oee = availability * performance * quality;
        var now = DateTime.UtcNow;

        // Upsert: find existing snapshot for this (station, periodKind, periodStart).
        var existing = await db.OeeSnapshots.FirstOrDefaultAsync(
            s => s.StationId == stationId
              && s.PeriodKind == periodKind
              && s.PeriodStart == periodStart,
            cancellationToken);

        if (existing is null)
        {
            existing = new OeeSnapshot
            {
                StationId = stationId,
                PeriodKind = periodKind,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                ModifiedDate = now,
            };
            db.OeeSnapshots.Add(existing);
        }

        existing.PeriodEnd = periodEnd;
        existing.PlannedRuntimeMinutes = plannedRuntimeMinutes;
        existing.ActualRuntimeMinutes = actualRuntimeMinutes;
        existing.DowntimeMinutes = downtimeMinutes;
        existing.UnitsProduced = unitsProduced;
        existing.UnitsScrapped = effectiveScrap;
        existing.IdealCycleSeconds = idealCycleSeconds;
        existing.Availability = availability;
        existing.Performance = performance;
        existing.Quality = quality;
        existing.Oee = oee;
        existing.ComputedAt = now;
        existing.ModifiedDate = now;

        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
