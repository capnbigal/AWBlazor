using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Services;

public sealed class ProductionMetricsService : IProductionMetricsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public ProductionMetricsService(IDbContextFactory<ApplicationDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<ProductionDailyMetric> ComputeDailyAsync(int stationId, DateOnly date, CancellationToken cancellationToken)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var runs = await db.ProductionRuns.AsNoTracking()
            .Where(r => r.StationId == stationId
                     && r.ActualStartAt != null
                     && r.ActualStartAt < dayEnd
                     && (r.ActualEndAt == null || r.ActualEndAt > dayStart))
            .Select(r => new { r.ActualStartAt, r.ActualEndAt, r.QuantityProduced, r.QuantityScrapped })
            .ToListAsync(cancellationToken);

        decimal unitsProduced = 0m, unitsScrapped = 0m, totalSeconds = 0m;
        foreach (var r in runs)
        {
            unitsProduced += r.QuantityProduced;
            unitsScrapped += r.QuantityScrapped;
            if (r.ActualEndAt.HasValue)
            {
                var start = r.ActualStartAt!.Value < dayStart ? dayStart : r.ActualStartAt.Value;
                var end = r.ActualEndAt.Value > dayEnd ? dayEnd : r.ActualEndAt.Value;
                if (end > start) totalSeconds += (decimal)(end - start).TotalSeconds;
            }
        }

        decimal? avgCycle = unitsProduced > 0 && totalSeconds > 0 ? totalSeconds / unitsProduced : null;
        decimal? yieldFraction = (unitsProduced + unitsScrapped) > 0
            ? (unitsProduced - unitsScrapped) / (unitsProduced + unitsScrapped)
            : null;
        // Clamp yield to 0-1 (prod − scrap can go negative if data is bad; better to clamp than expose weirdness).
        if (yieldFraction.HasValue) yieldFraction = Math.Clamp(yieldFraction.Value, 0m, 1m);

        var now = DateTime.UtcNow;

        var existing = await db.ProductionDailyMetrics.FirstOrDefaultAsync(
            x => x.StationId == stationId && x.Date == date, cancellationToken);

        if (existing is null)
        {
            existing = new ProductionDailyMetric
            {
                StationId = stationId,
                Date = date,
                ModifiedDate = now,
            };
            db.ProductionDailyMetrics.Add(existing);
        }

        existing.UnitsProduced = unitsProduced;
        existing.UnitsScrapped = unitsScrapped;
        existing.AverageCycleSeconds = avgCycle;
        existing.YieldFraction = yieldFraction;
        existing.RunCount = runs.Count;
        existing.ComputedAt = now;
        existing.ModifiedDate = now;

        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
