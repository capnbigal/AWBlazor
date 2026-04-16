using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Features.Insights.Domain;
using AWBlazorApp.Shared.Domain;
using AWBlazorApp.Features.ProcessManagement.Domain;
using AWBlazorApp.Features.Forecasting.Domain;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Insights.Services;

/// <summary>
/// Executes SavedQuery definitions. Each <see cref="QueryMetric"/> knows how to pull a scalar
/// or grouped result from the database; the <see cref="QueryGroupBy"/> bucketizes by period.
/// </summary>
public sealed class SavedQueryRunner(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public async Task<QueryResult> RunAsync(SavedQuery q, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (q.GroupBy == QueryGroupBy.None)
        {
            var scalar = await ScalarAsync(db, q, ct);
            return new QueryResult(q.Metric, q.GroupBy, new List<QueryBucket> { new(null, scalar) }, scalar);
        }

        var buckets = await GroupedAsync(db, q, ct);
        var total = buckets.Sum(b => b.Value);
        return new QueryResult(q.Metric, q.GroupBy, buckets, total);
    }

    private static async Task<double> ScalarAsync(ApplicationDbContext db, SavedQuery q, CancellationToken ct)
    {
        return q.Metric switch
        {
            QueryMetric.SalesOrderCount        => await db.SalesOrderHeaders.Where(DateFilter<Features.Sales.Domain.SalesOrderHeader>(q, o => o.OrderDate)).CountAsync(ct),
            QueryMetric.TotalSalesRevenue      => (double)await db.SalesOrderHeaders.Where(DateFilter<Features.Sales.Domain.SalesOrderHeader>(q, o => o.OrderDate)).SumAsync(o => o.TotalDue, ct),
            QueryMetric.AverageOrderValue      => (double)await db.SalesOrderHeaders.Where(DateFilter<Features.Sales.Domain.SalesOrderHeader>(q, o => o.OrderDate)).AverageAsync(o => o.TotalDue, ct),
            QueryMetric.OpenWorkOrderCount     => await db.WorkOrders.CountAsync(w => w.EndDate == null, ct),
            QueryMetric.WorkOrderCount         => await db.WorkOrders.Where(DateFilter<Features.Production.Domain.WorkOrder>(q, w => w.StartDate)).CountAsync(ct),
            QueryMetric.PurchaseOrderCount     => await db.PurchaseOrderHeaders.Where(DateFilter<Features.Purchasing.Domain.PurchaseOrderHeader>(q, p => p.OrderDate)).CountAsync(ct),
            QueryMetric.TotalPurchaseSpend     => (double)await db.PurchaseOrderHeaders.Where(DateFilter<Features.Purchasing.Domain.PurchaseOrderHeader>(q, p => p.OrderDate)).SumAsync(p => p.TotalDue, ct),
            QueryMetric.ActiveForecastCount    => await db.ForecastDefinitions.CountAsync(f => f.DeletedDate == null && f.Status == ForecastStatus.Active, ct),
            QueryMetric.ActiveProcessCount     => await db.Processes.CountAsync(p => p.DeletedDate == null && p.Status == ProcessStatus.Active, ct),
            QueryMetric.RegisteredUserCount    => await db.Users.CountAsync(ct),
            _                                  => 0,
        };
    }

    private static async Task<List<QueryBucket>> GroupedAsync(ApplicationDbContext db, SavedQuery q, CancellationToken ct)
    {
        // Group by an integer bucket key (always translatable on SQL Server) and rebuild the
        // DateTime client-side. Avoids the EF Core failure where a `new DateTime(...)` GroupBy
        // key combined with an aggregate projection + OrderBy can't be translated.
        var g = q.GroupBy;
        return q.Metric switch
        {
            QueryMetric.SalesOrderCount => ToBuckets(await db.SalesOrderHeaders
                .Where(DateFilter<Features.Sales.Domain.SalesOrderHeader>(q, o => o.OrderDate))
                .GroupBy(IntBucketSelector<Features.Sales.Domain.SalesOrderHeader>(o => o.OrderDate, g))
                .Select(grp => new BucketRow(grp.Key, grp.Count()))
                .ToListAsync(ct), g),

            QueryMetric.TotalSalesRevenue => ToBuckets(await db.SalesOrderHeaders
                .Where(DateFilter<Features.Sales.Domain.SalesOrderHeader>(q, o => o.OrderDate))
                .GroupBy(IntBucketSelector<Features.Sales.Domain.SalesOrderHeader>(o => o.OrderDate, g))
                .Select(grp => new BucketRow(grp.Key, (double)grp.Sum(x => x.TotalDue)))
                .ToListAsync(ct), g),

            QueryMetric.AverageOrderValue => ToBuckets(await db.SalesOrderHeaders
                .Where(DateFilter<Features.Sales.Domain.SalesOrderHeader>(q, o => o.OrderDate))
                .GroupBy(IntBucketSelector<Features.Sales.Domain.SalesOrderHeader>(o => o.OrderDate, g))
                .Select(grp => new BucketRow(grp.Key, (double)grp.Average(x => x.TotalDue)))
                .ToListAsync(ct), g),

            QueryMetric.WorkOrderCount => ToBuckets(await db.WorkOrders
                .Where(DateFilter<Features.Production.Domain.WorkOrder>(q, w => w.StartDate))
                .GroupBy(IntBucketSelector<Features.Production.Domain.WorkOrder>(w => w.StartDate, g))
                .Select(grp => new BucketRow(grp.Key, grp.Count()))
                .ToListAsync(ct), g),

            QueryMetric.PurchaseOrderCount => ToBuckets(await db.PurchaseOrderHeaders
                .Where(DateFilter<Features.Purchasing.Domain.PurchaseOrderHeader>(q, p => p.OrderDate))
                .GroupBy(IntBucketSelector<Features.Purchasing.Domain.PurchaseOrderHeader>(p => p.OrderDate, g))
                .Select(grp => new BucketRow(grp.Key, grp.Count()))
                .ToListAsync(ct), g),

            QueryMetric.TotalPurchaseSpend => ToBuckets(await db.PurchaseOrderHeaders
                .Where(DateFilter<Features.Purchasing.Domain.PurchaseOrderHeader>(q, p => p.OrderDate))
                .GroupBy(IntBucketSelector<Features.Purchasing.Domain.PurchaseOrderHeader>(p => p.OrderDate, g))
                .Select(grp => new BucketRow(grp.Key, (double)grp.Sum(x => x.TotalDue)))
                .ToListAsync(ct), g),

            // Non-temporal metrics → collapse into a single bucket.
            _ => new List<QueryBucket> { new(null, await ScalarAsync(db, q, ct)) },
        };
    }

    private static List<QueryBucket> ToBuckets(List<BucketRow> rows, QueryGroupBy g)
        => rows
            .Select(r => new QueryBucket(IntBucketToDate(r.Key, g), r.Value))
            .OrderBy(b => b.Period)
            .ToList();

    private sealed record BucketRow(int Key, double Value);

    /// <summary>Builds a date-range filter expression over an entity property.</summary>
    private static Expression<Func<T, bool>> DateFilter<T>(
        SavedQuery q, Expression<Func<T, DateTime>> selector)
    {
        // The bounds are wrapped in a ValueHolder reference and accessed via Expression.Field
        // so EF Core parameterizes them (`@p0 datetime2`) instead of inlining them as literal
        // strings. Inlined literals fail against AdventureWorks `datetime` columns because the
        // 7-digit fractional-second ISO format SQL Server emits isn't a valid `datetime` literal.
        var from = q.FromDate;
        var to = q.ToDate;
        var param = selector.Parameters[0];
        var body = selector.Body;

        Expression? predicate = null;
        if (from.HasValue)
            predicate = Expression.GreaterThanOrEqual(body, ParameterizedDateTime(from.Value));
        if (to.HasValue)
        {
            var upper = Expression.LessThanOrEqual(body, ParameterizedDateTime(to.Value));
            predicate = predicate is null ? upper : Expression.AndAlso(predicate, upper);
        }

        predicate ??= Expression.Constant(true);
        return Expression.Lambda<Func<T, bool>>(predicate, param);
    }

    // Wrapping the value in a heap object and reading it through MemberAccess mimics the
    // shape of a captured closure local — EF Core parameterizes it instead of baking a literal.
    private static MemberExpression ParameterizedDateTime(DateTime value)
        => Expression.Field(Expression.Constant(new DateTimeBox(value)), nameof(DateTimeBox.Value));

    private sealed class DateTimeBox(DateTime value)
    {
        public readonly DateTime Value = value;
    }

    // Anchors used by the int bucket keys. Day buckets count days since DayAnchor, week buckets
    // count weeks since WeekAnchor (a Sunday so `% 7` gives Sunday-relative day-of-week),
    // month/quarter buckets pack year+sub-period into a single int.
    private static readonly DateTime DayAnchor = new(2000, 1, 1);
    private static readonly DateTime WeekAnchor = new(2000, 1, 2); // Sunday

    // Returns a GroupBy key selector that produces an int bucket key. SQL Server can always
    // translate int arithmetic + DATEDIFF, whereas projecting a `new DateTime(...)` GroupBy
    // key + aggregating + ordering trips a known EF Core translator limitation.
    private static Expression<Func<T, int>> IntBucketSelector<T>(
        Expression<Func<T, DateTime>> dateSelector, QueryGroupBy g)
    {
        Expression<Func<DateTime, int>> bucket = g switch
        {
            QueryGroupBy.Day     => d => EF.Functions.DateDiffDay(DayAnchor, d),
            QueryGroupBy.Week    => d => EF.Functions.DateDiffDay(WeekAnchor, d) / 7,
            QueryGroupBy.Month   => d => d.Year * 12 + (d.Month - 1),
            QueryGroupBy.Quarter => d => d.Year * 4 + (d.Month - 1) / 3,
            QueryGroupBy.Year    => d => d.Year,
            _                    => d => 0,
        };

        var body = new ParameterReplacer(bucket.Parameters[0], dateSelector.Body).Visit(bucket.Body)!;
        return Expression.Lambda<Func<T, int>>(body, dateSelector.Parameters);
    }

    // Inverse of IntBucketSelector — turn the integer key back into a DateTime that
    // represents the start of the bucket period.
    private static DateTime IntBucketToDate(int key, QueryGroupBy g) => g switch
    {
        QueryGroupBy.Day     => DayAnchor.AddDays(key),
        QueryGroupBy.Week    => WeekAnchor.AddDays((long)key * 7),
        QueryGroupBy.Month   => new DateTime(key / 12, key % 12 + 1, 1),
        QueryGroupBy.Quarter => new DateTime(key / 4, key % 4 * 3 + 1, 1),
        QueryGroupBy.Year    => new DateTime(key, 1, 1),
        _                    => default,
    };

    private sealed class ParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }

    public static string MetricLabel(QueryMetric m) => m switch
    {
        QueryMetric.SalesOrderCount       => "Sales order count",
        QueryMetric.TotalSalesRevenue     => "Total sales revenue",
        QueryMetric.AverageOrderValue     => "Average order value",
        QueryMetric.OpenWorkOrderCount    => "Open work orders",
        QueryMetric.WorkOrderCount        => "Work orders started",
        QueryMetric.PurchaseOrderCount    => "Purchase orders",
        QueryMetric.TotalPurchaseSpend    => "Total purchase spend",
        QueryMetric.ActiveForecastCount   => "Active forecasts",
        QueryMetric.ActiveProcessCount    => "Active processes",
        QueryMetric.RegisteredUserCount   => "Registered users",
        _                                 => m.ToString(),
    };

    public static bool IsCurrency(QueryMetric m) => m is
        QueryMetric.TotalSalesRevenue or
        QueryMetric.AverageOrderValue or
        QueryMetric.TotalPurchaseSpend;

    public sealed record QueryResult(QueryMetric Metric, QueryGroupBy GroupBy, List<QueryBucket> Buckets, double Scalar);
    public sealed record QueryBucket(DateTime? Period, double Value);
}
