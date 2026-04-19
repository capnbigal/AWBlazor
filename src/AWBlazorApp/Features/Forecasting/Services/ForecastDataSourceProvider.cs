using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Features.Forecasting.Domain;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Forecasting.Services;

public sealed class ForecastDataSourceProvider(IDbContextFactory<ApplicationDbContext> dbFactory) : IForecastDataSourceProvider
{
    public async Task<List<TimeSeriesPoint>> GetHistoricalDataAsync(
        ForecastDataSource dataSource,
        ForecastGranularity granularity,
        int lookbackMonths,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cutoff = DateTime.UtcNow.AddMonths(-lookbackMonths);

        return dataSource switch
        {
            ForecastDataSource.SalesRevenue => await GetGroupedAsync(db, cutoff, granularity,
                q => q.SalesOrderHeaders.Where(o => o.OrderDate >= cutoff),
                o => o.OrderDate, o => o.TotalDue, ct),

            ForecastDataSource.SalesOrderCount => await GetCountGroupedAsync(db, cutoff, granularity,
                q => q.SalesOrderHeaders.Where(o => o.OrderDate >= cutoff),
                o => o.OrderDate, ct),

            ForecastDataSource.WorkOrderVolume => await GetCountGroupedAsync(db, cutoff, granularity,
                q => q.WorkOrders.Where(w => w.StartDate >= cutoff),
                w => w.StartDate, ct),

            ForecastDataSource.PurchaseOrderSpend => await GetGroupedAsync(db, cutoff, granularity,
                q => q.PurchaseOrderHeaders.Where(p => p.OrderDate >= cutoff),
                p => p.OrderDate, p => p.TotalDue, ct),

            ForecastDataSource.EmployeeHireCount => await GetCountGroupedAsync(db, cutoff, granularity,
                q => q.Employees.Where(e => e.HireDate >= cutoff),
                e => e.HireDate, ct),

            ForecastDataSource.ProductionScrapRate => await GetScrapRateAsync(db, cutoff, granularity, ct),

            _ => throw new ArgumentOutOfRangeException(nameof(dataSource)),
        };
    }

    public async Task<decimal?> GetActualValueAsync(
        ForecastDataSource dataSource,
        ForecastGranularity granularity,
        DateTime periodDate,
        CancellationToken ct = default)
    {
        var periodEnd = granularity == ForecastGranularity.Quarterly
            ? periodDate.AddMonths(3)
            : periodDate.AddMonths(1);

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        return dataSource switch
        {
            ForecastDataSource.SalesRevenue =>
                await db.SalesOrderHeaders.Where(o => o.OrderDate >= periodDate && o.OrderDate < periodEnd)
                    .Select(o => (decimal?)o.TotalDue).SumAsync(ct),

            ForecastDataSource.SalesOrderCount =>
                await db.SalesOrderHeaders.CountAsync(o => o.OrderDate >= periodDate && o.OrderDate < periodEnd, ct),

            ForecastDataSource.WorkOrderVolume =>
                await db.WorkOrders.CountAsync(w => w.StartDate >= periodDate && w.StartDate < periodEnd, ct),

            ForecastDataSource.PurchaseOrderSpend =>
                await db.PurchaseOrderHeaders.Where(p => p.OrderDate >= periodDate && p.OrderDate < periodEnd)
                    .Select(p => (decimal?)p.TotalDue).SumAsync(ct),

            ForecastDataSource.EmployeeHireCount =>
                await db.Employees.CountAsync(e => e.HireDate >= periodDate && e.HireDate < periodEnd, ct),

            ForecastDataSource.ProductionScrapRate =>
                await GetScrapRateForPeriodAsync(db, periodDate, periodEnd, ct),

            _ => null,
        };
    }

    private static async Task<List<TimeSeriesPoint>> GetGroupedAsync<T>(
        ApplicationDbContext db, DateTime cutoff, ForecastGranularity granularity,
        Func<ApplicationDbContext, IQueryable<T>> queryBuilder,
        Func<T, DateTime> dateSelector, Func<T, decimal> valueSelector,
        CancellationToken ct) where T : class
    {
        // EF can't always translate arbitrary Func-based GroupBy, so materialize then group in memory
        var data = await queryBuilder(db).AsNoTracking().ToListAsync(ct);

        var grouped = granularity == ForecastGranularity.Quarterly
            ? data.GroupBy(x => new DateTime(dateSelector(x).Year, ((dateSelector(x).Month - 1) / 3) * 3 + 1, 1))
            : data.GroupBy(x => new DateTime(dateSelector(x).Year, dateSelector(x).Month, 1));

        return grouped
            .Select(g => new TimeSeriesPoint(g.Key, g.Sum(valueSelector)))
            .OrderBy(p => p.PeriodDate)
            .ToList();
    }

    private static async Task<List<TimeSeriesPoint>> GetCountGroupedAsync<T>(
        ApplicationDbContext db, DateTime cutoff, ForecastGranularity granularity,
        Func<ApplicationDbContext, IQueryable<T>> queryBuilder,
        Func<T, DateTime> dateSelector,
        CancellationToken ct) where T : class
    {
        var data = await queryBuilder(db).AsNoTracking().ToListAsync(ct);

        var grouped = granularity == ForecastGranularity.Quarterly
            ? data.GroupBy(x => new DateTime(dateSelector(x).Year, ((dateSelector(x).Month - 1) / 3) * 3 + 1, 1))
            : data.GroupBy(x => new DateTime(dateSelector(x).Year, dateSelector(x).Month, 1));

        return grouped
            .Select(g => new TimeSeriesPoint(g.Key, g.Count()))
            .OrderBy(p => p.PeriodDate)
            .ToList();
    }

    private static async Task<List<TimeSeriesPoint>> GetScrapRateAsync(
        ApplicationDbContext db, DateTime cutoff, ForecastGranularity granularity, CancellationToken ct)
    {
        var data = await db.WorkOrders.AsNoTracking()
            .Where(w => w.StartDate >= cutoff)
            .ToListAsync(ct);

        var grouped = granularity == ForecastGranularity.Quarterly
            ? data.GroupBy(w => new DateTime(w.StartDate.Year, ((w.StartDate.Month - 1) / 3) * 3 + 1, 1))
            : data.GroupBy(w => new DateTime(w.StartDate.Year, w.StartDate.Month, 1));

        return grouped
            .Select(g =>
            {
                var totalQty = g.Sum(w => (long)w.OrderQty);
                var scrapped = g.Sum(w => (long)w.ScrappedQty);
                var rate = totalQty > 0 ? (decimal)scrapped / totalQty * 100 : 0;
                return new TimeSeriesPoint(g.Key, Math.Round(rate, 4));
            })
            .OrderBy(p => p.PeriodDate)
            .ToList();
    }

    private static async Task<decimal?> GetScrapRateForPeriodAsync(
        ApplicationDbContext db, DateTime periodDate, DateTime periodEnd, CancellationToken ct)
    {
        var data = await db.WorkOrders.AsNoTracking()
            .Where(w => w.StartDate >= periodDate && w.StartDate < periodEnd)
            .ToListAsync(ct);

        if (data.Count == 0) return null;
        var totalQty = data.Sum(w => (long)w.OrderQty);
        var scrapped = data.Sum(w => (long)w.ScrappedQty);
        return totalQty > 0 ? Math.Round((decimal)scrapped / totalQty * 100, 4) : 0;
    }
}
