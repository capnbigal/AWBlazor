using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities;

/// <summary>
/// A named, reusable query definition owned by a user. The /queries page runs these on demand,
/// and future PRs layer scheduled snapshots (KPI builder) and email reports on top.
/// </summary>
public class SavedQuery
{
    public int Id { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public QueryMetric Metric { get; set; }

    public QueryGroupBy GroupBy { get; set; } = QueryGroupBy.None;

    /// <summary>Optional start of the date window; null = no lower bound.</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Optional end of the date window; null = no upper bound.</summary>
    public DateTime? ToDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastRunDate { get; set; }
}

public enum QueryMetric
{
    SalesOrderCount,
    TotalSalesRevenue,
    AverageOrderValue,
    OpenWorkOrderCount,
    WorkOrderCount,
    PurchaseOrderCount,
    TotalPurchaseSpend,
    ActiveForecastCount,
    ActiveProcessCount,
    RegisteredUserCount,
}

public enum QueryGroupBy
{
    None,
    Day,
    Week,
    Month,
    Quarter,
    Year,
}
