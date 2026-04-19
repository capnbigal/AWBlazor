namespace AWBlazorApp.Features.Insights.Domain;

/// <summary>
/// One recorded run of a SavedQuery flagged IsKpi, used to build the sparkline history on the
/// KPI tile. Written by the hourly Hangfire KpiSnapshotJob.
/// </summary>
public class KpiSnapshot
{
    public int Id { get; set; }
    public int SavedQueryId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}
