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
            QueryMetric.SalesOrderCount        => await db.SalesOrderHeaders.Where(DateFilter<Features.AdventureWorks.Domain.SalesOrderHeader>(q, o => o.OrderDate)).CountAsync(ct),
            QueryMetric.TotalSalesRevenue      => (double)await db.SalesOrderHeaders.Where(DateFilter<Features.AdventureWorks.Domain.SalesOrderHeader>(q, o => o.OrderDate)).SumAsync(o => o.TotalDue, ct),
            QueryMetric.AverageOrderValue      => (double)await db.SalesOrderHeaders.Where(DateFilter<Features.AdventureWorks.Domain.SalesOrderHeader>(q, o => o.OrderDate)).AverageAsync(o => o.TotalDue, ct),
            QueryMetric.OpenWorkOrderCount     => await db.WorkOrders.CountAsync(w => w.EndDate == null, ct),
            QueryMetric.WorkOrderCount         => await db.WorkOrders.Where(DateFilter<Features.AdventureWorks.Domain.WorkOrder>(q, w => w.StartDate)).CountAsync(ct),
            QueryMetric.PurchaseOrderCount     => await db.PurchaseOrderHeaders.Where(DateFilter<Features.AdventureWorks.Domain.PurchaseOrderHeader>(q, p => p.OrderDate)).CountAsync(ct),
            QueryMetric.TotalPurchaseSpend     => (double)await db.PurchaseOrderHeaders.Where(DateFilter<Features.AdventureWorks.Domain.PurchaseOrderHeader>(q, p => p.OrderDate)).SumAsync(p => p.TotalDue, ct),
            QueryMetric.ActiveForecastCount    => await db.ForecastDefinitions.CountAsync(f => f.DeletedDate == null && f.Status == ForecastStatus.Active, ct),
            QueryMetric.ActiveProcessCount     => await db.Processes.CountAsync(p => p.DeletedDate == null && p.Status == ProcessStatus.Active, ct),
            QueryMetric.RegisteredUserCount    => await db.Users.CountAsync(ct),
            _                                  => 0,
        };
    }

    private static async Task<List<QueryBucket>> GroupedAsync(ApplicationDbContext db, SavedQuery q, CancellationToken ct)
    {
        // Only the time-based metrics support grouping — the non-temporal ones collapse into a single bucket.
        return q.Metric switch
        {
            QueryMetric.SalesOrderCount => await db.SalesOrderHeaders
                .Where(DateFilter<Features.AdventureWorks.Domain.SalesOrderHeader>(q, o => o.OrderDate))
                .GroupBy(BucketSelector<Features.AdventureWorks.Domain.SalesOrderHeader>(o => o.OrderDate, q.GroupBy))
                .Select(g => new QueryBucket(g.Key, g.Count()))
                .OrderBy(b => b.Period)
                .ToListAsync(ct),

            QueryMetric.TotalSalesRevenue => (await db.SalesOrderHeaders
                .Where(DateFilter<Features.AdventureWorks.Domain.SalesOrderHeader>(q, o => o.OrderDate))
                .GroupBy(BucketSelector<Features.AdventureWorks.Domain.SalesOrderHeader>(o => o.OrderDate, q.GroupBy))
                .Select(g => new { Period = g.Key, Value = (double)g.Sum(x => x.TotalDue) })
                .OrderBy(b => b.Period)
                .ToListAsync(ct)).Select(x => new QueryBucket(x.Period, x.Value)).ToList(),

            QueryMetric.AverageOrderValue => (await db.SalesOrderHeaders
                .Where(DateFilter<Features.AdventureWorks.Domain.SalesOrderHeader>(q, o => o.OrderDate))
                .GroupBy(BucketSelector<Features.AdventureWorks.Domain.SalesOrderHeader>(o => o.OrderDate, q.GroupBy))
                .Select(g => new { Period = g.Key, Value = (double)g.Average(x => x.TotalDue) })
                .OrderBy(b => b.Period)
                .ToListAsync(ct)).Select(x => new QueryBucket(x.Period, x.Value)).ToList(),

            QueryMetric.WorkOrderCount => await db.WorkOrders
                .Where(DateFilter<Features.AdventureWorks.Domain.WorkOrder>(q, w => w.StartDate))
                .GroupBy(BucketSelector<Features.AdventureWorks.Domain.WorkOrder>(w => w.StartDate, q.GroupBy))
                .Select(g => new QueryBucket(g.Key, g.Count()))
                .OrderBy(b => b.Period)
                .ToListAsync(ct),

            QueryMetric.PurchaseOrderCount => await db.PurchaseOrderHeaders
                .Where(DateFilter<Features.AdventureWorks.Domain.PurchaseOrderHeader>(q, p => p.OrderDate))
                .GroupBy(BucketSelector<Features.AdventureWorks.Domain.PurchaseOrderHeader>(p => p.OrderDate, q.GroupBy))
                .Select(g => new QueryBucket(g.Key, g.Count()))
                .OrderBy(b => b.Period)
                .ToListAsync(ct),

            QueryMetric.TotalPurchaseSpend => (await db.PurchaseOrderHeaders
                .Where(DateFilter<Features.AdventureWorks.Domain.PurchaseOrderHeader>(q, p => p.OrderDate))
                .GroupBy(BucketSelector<Features.AdventureWorks.Domain.PurchaseOrderHeader>(p => p.OrderDate, q.GroupBy))
                .Select(g => new { Period = g.Key, Value = (double)g.Sum(x => x.TotalDue) })
                .OrderBy(b => b.Period)
                .ToListAsync(ct)).Select(x => new QueryBucket(x.Period, x.Value)).ToList(),

            // Non-temporal metrics → collapse into a single bucket.
            _ => new List<QueryBucket> { new(null, await ScalarAsync(db, q, ct)) },
        };
    }

    /// <summary>Builds a date-range filter expression over an entity property.</summary>
    private static System.Linq.Expressions.Expression<Func<T, bool>> DateFilter<T>(
        SavedQuery q, System.Linq.Expressions.Expression<Func<T, DateTime>> selector)
    {
        // Compose bounds inline — EF translates DateTime comparisons fine.
        var from = q.FromDate;
        var to = q.ToDate;
        var param = selector.Parameters[0];
        var body = selector.Body;

        System.Linq.Expressions.Expression? predicate = null;
        if (from.HasValue)
            predicate = System.Linq.Expressions.Expression.GreaterThanOrEqual(body, System.Linq.Expressions.Expression.Constant(from.Value, typeof(DateTime)));
        if (to.HasValue)
        {
            var upper = System.Linq.Expressions.Expression.LessThanOrEqual(body, System.Linq.Expressions.Expression.Constant(to.Value, typeof(DateTime)));
            predicate = predicate is null ? upper : System.Linq.Expressions.Expression.AndAlso(predicate, upper);
        }

        predicate ??= System.Linq.Expressions.Expression.Constant(true);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(predicate, param);
    }

    // Sunday anchor used by the Week bucket — `(DateDiffDay(anchor, d) % 7)` gives the
    // Sunday-relative day-of-week without leaning on `DateTime.DayOfWeek`, which the
    // SQL Server provider doesn't translate.
    private static readonly DateTime WeekAnchor = new(2000, 1, 2);

    // Builds a translatable group-key selector by composing the per-group bucket lambda
    // with the entity's date selector. Inlined this way (rather than calling a static
    // method inside the GroupBy), EF Core can map each arm to DATEFROMPARTS / DATEADD.
    private static Expression<Func<T, DateTime>> BucketSelector<T>(
        Expression<Func<T, DateTime>> dateSelector, QueryGroupBy g)
    {
        Expression<Func<DateTime, DateTime>> bucket = g switch
        {
            QueryGroupBy.Day     => d => new DateTime(d.Year, d.Month, d.Day),
            QueryGroupBy.Week    => d => new DateTime(d.Year, d.Month, d.Day).AddDays(-(EF.Functions.DateDiffDay(WeekAnchor, d) % 7)),
            QueryGroupBy.Month   => d => new DateTime(d.Year, d.Month, 1),
            QueryGroupBy.Quarter => d => new DateTime(d.Year, ((d.Month - 1) / 3) * 3 + 1, 1),
            QueryGroupBy.Year    => d => new DateTime(d.Year, 1, 1),
            _                    => d => d,
        };

        var body = new ParameterReplacer(bucket.Parameters[0], dateSelector.Body).Visit(bucket.Body)!;
        return Expression.Lambda<Func<T, DateTime>>(body, dateSelector.Parameters);
    }

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
