using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class ProductListPriceHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductListPriceHistory e) => new(e);

    public static ProductListPriceHistoryAuditLog RecordCreate(ProductListPriceHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductListPriceHistoryAuditLog RecordUpdate(Snapshot before, ProductListPriceHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductListPriceHistoryAuditLog RecordDelete(ProductListPriceHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductListPriceHistoryAuditLog BuildLog(
        ProductListPriceHistory e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.ProductId,
            StartDate = e.StartDate,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            EndDate = e.EndDate,
            ListPrice = e.ListPrice,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductListPriceHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "EndDate", before.EndDate, after.EndDate);
        AuditDiffHelpers.AppendIfChanged(sb, "ListPrice", before.ListPrice, after.ListPrice);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DateTime? EndDate, decimal ListPrice)
    {
        public Snapshot(ProductListPriceHistory e) : this(e.EndDate, e.ListPrice) { }
    }
}
