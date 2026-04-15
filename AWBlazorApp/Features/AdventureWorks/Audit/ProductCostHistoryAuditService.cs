using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ProductCostHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductCostHistory e) => new(e);

    public static ProductCostHistoryAuditLog RecordCreate(ProductCostHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductCostHistoryAuditLog RecordUpdate(Snapshot before, ProductCostHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductCostHistoryAuditLog RecordDelete(ProductCostHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductCostHistoryAuditLog BuildLog(ProductCostHistory e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.ProductId,
            StartDate = e.StartDate,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            EndDate = e.EndDate,
            StandardCost = e.StandardCost,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductCostHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "StandardCost", before.StandardCost, after.StandardCost);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DateTime? EndDate, decimal StandardCost)
    {
        public Snapshot(ProductCostHistory e) : this(e.EndDate, e.StandardCost) { }
    }
}
