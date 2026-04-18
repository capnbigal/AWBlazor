using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Services;

public sealed class KpiEvaluationService : IKpiEvaluationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public KpiEvaluationService(IDbContextFactory<ApplicationDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<KpiValue> EvaluateAsync(
        int kpiDefinitionId,
        PerformancePeriodKind periodKind,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var def = await db.KpiDefinitions.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kpiDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"KPI definition {kpiDefinitionId} not found.");

        var samples = await FetchSamplesAsync(db, def.Source, periodStart, periodEnd, cancellationToken);
        var aggregated = Aggregate(samples, def.Aggregation);
        var status = ClassifyStatus(aggregated, def);

        var now = DateTime.UtcNow;
        var existing = await db.KpiValues.FirstOrDefaultAsync(
            v => v.KpiDefinitionId == kpiDefinitionId
              && v.PeriodKind == periodKind
              && v.PeriodStart == periodStart,
            cancellationToken);

        if (existing is null)
        {
            existing = new KpiValue
            {
                KpiDefinitionId = kpiDefinitionId,
                PeriodKind = periodKind,
                PeriodStart = periodStart,
                ModifiedDate = now,
            };
            db.KpiValues.Add(existing);
        }

        existing.PeriodEnd = periodEnd;
        existing.Value = aggregated;
        existing.TargetAtPeriod = def.TargetValue;
        existing.Status = status;
        existing.ComputedAt = now;
        existing.ModifiedDate = now;

        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static async Task<List<decimal>> FetchSamplesAsync(
        ApplicationDbContext db, KpiSource source, DateTime periodStart, DateTime periodEnd, CancellationToken ct)
    {
        return source switch
        {
            KpiSource.OeeOverall =>
                (await db.OeeSnapshots.AsNoTracking()
                    .Where(s => s.PeriodStart >= periodStart && s.PeriodStart < periodEnd)
                    .Select(s => s.Oee).ToListAsync(ct)),
            KpiSource.OeeAvailability =>
                (await db.OeeSnapshots.AsNoTracking()
                    .Where(s => s.PeriodStart >= periodStart && s.PeriodStart < periodEnd)
                    .Select(s => s.Availability).ToListAsync(ct)),
            KpiSource.OeePerformance =>
                (await db.OeeSnapshots.AsNoTracking()
                    .Where(s => s.PeriodStart >= periodStart && s.PeriodStart < periodEnd)
                    .Select(s => s.Performance).ToListAsync(ct)),
            KpiSource.OeeQuality =>
                (await db.OeeSnapshots.AsNoTracking()
                    .Where(s => s.PeriodStart >= periodStart && s.PeriodStart < periodEnd)
                    .Select(s => s.Quality).ToListAsync(ct)),
            KpiSource.ProductionUnits =>
                (await db.ProductionDailyMetrics.AsNoTracking()
                    .Where(m => m.Date >= DateOnly.FromDateTime(periodStart) && m.Date < DateOnly.FromDateTime(periodEnd))
                    .Select(m => m.UnitsProduced).ToListAsync(ct)),
            KpiSource.ProductionYield =>
                (await db.ProductionDailyMetrics.AsNoTracking()
                    .Where(m => m.Date >= DateOnly.FromDateTime(periodStart) && m.Date < DateOnly.FromDateTime(periodEnd) && m.YieldFraction != null)
                    .Select(m => m.YieldFraction!.Value).ToListAsync(ct)),
            KpiSource.ProductionCycleSeconds =>
                (await db.ProductionDailyMetrics.AsNoTracking()
                    .Where(m => m.Date >= DateOnly.FromDateTime(periodStart) && m.Date < DateOnly.FromDateTime(periodEnd) && m.AverageCycleSeconds != null)
                    .Select(m => m.AverageCycleSeconds!.Value).ToListAsync(ct)),
            KpiSource.MaintenanceMtbf =>
                (await db.MaintenanceMonthlyMetrics.AsNoTracking()
                    .Where(m => m.MtbfHours != null)
                    .Where(m => IsMonthInRange(m.Year, m.Month, periodStart, periodEnd))
                    .Select(m => m.MtbfHours!.Value).ToListAsync(ct)),
            KpiSource.MaintenanceMttr =>
                (await db.MaintenanceMonthlyMetrics.AsNoTracking()
                    .Where(m => m.MttrHours != null)
                    .Where(m => IsMonthInRange(m.Year, m.Month, periodStart, periodEnd))
                    .Select(m => m.MttrHours!.Value).ToListAsync(ct)),
            KpiSource.MaintenanceAvailability =>
                (await db.MaintenanceMonthlyMetrics.AsNoTracking()
                    .Where(m => m.AvailabilityFraction != null)
                    .Where(m => IsMonthInRange(m.Year, m.Month, periodStart, periodEnd))
                    .Select(m => m.AvailabilityFraction!.Value).ToListAsync(ct)),
            KpiSource.MaintenancePmCompliance =>
                (await db.MaintenanceMonthlyMetrics.AsNoTracking()
                    .Where(m => m.PmComplianceFraction != null)
                    .Where(m => IsMonthInRange(m.Year, m.Month, periodStart, periodEnd))
                    .Select(m => m.PmComplianceFraction!.Value).ToListAsync(ct)),
            _ => new List<decimal>(),
        };
    }

    /// <summary>Client-side filter after the main query — a month "is in range" if its first day falls in [periodStart, periodEnd).</summary>
    private static bool IsMonthInRange(int year, int month, DateTime start, DateTime end)
    {
        var mStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        return mStart >= start && mStart < end;
    }

    private static decimal? Aggregate(List<decimal> samples, KpiAggregation agg)
    {
        if (samples.Count == 0) return null;
        return agg switch
        {
            KpiAggregation.Sum => samples.Sum(),
            KpiAggregation.Average => samples.Average(),
            KpiAggregation.Minimum => samples.Min(),
            KpiAggregation.Maximum => samples.Max(),
            KpiAggregation.Latest => samples[^1],
            _ => null,
        };
    }

    private static KpiStatus ClassifyStatus(decimal? value, KpiDefinition def)
    {
        if (!value.HasValue) return KpiStatus.Unknown;
        // If no thresholds configured, fall back to OnTarget vs Warning based on target.
        if (def.Direction == KpiDirection.HigherIsBetter)
        {
            if (def.CriticalThreshold.HasValue && value.Value < def.CriticalThreshold.Value) return KpiStatus.Critical;
            if (def.WarningThreshold.HasValue && value.Value < def.WarningThreshold.Value) return KpiStatus.Warning;
            return KpiStatus.OnTarget;
        }
        else
        {
            if (def.CriticalThreshold.HasValue && value.Value > def.CriticalThreshold.Value) return KpiStatus.Critical;
            if (def.WarningThreshold.HasValue && value.Value > def.WarningThreshold.Value) return KpiStatus.Warning;
            return KpiStatus.OnTarget;
        }
    }
}
