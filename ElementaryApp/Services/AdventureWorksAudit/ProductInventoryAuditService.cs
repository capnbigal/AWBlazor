using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class ProductInventoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductInventory e) => new(e);

    public static ProductInventoryAuditLog RecordCreate(ProductInventory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductInventoryAuditLog RecordUpdate(Snapshot before, ProductInventory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductInventoryAuditLog RecordDelete(ProductInventory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductInventoryAuditLog BuildLog(ProductInventory e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.ProductId,
            LocationId = e.LocationId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Shelf = e.Shelf,
            Bin = e.Bin,
            Quantity = e.Quantity,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductInventory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Shelf", before.Shelf, after.Shelf);
        AuditDiffHelpers.AppendIfChanged(sb, "Bin", before.Bin, after.Bin);
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", before.Quantity, after.Quantity);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Shelf, byte Bin, short Quantity)
    {
        public Snapshot(ProductInventory e) : this(e.Shelf, e.Bin, e.Quantity) { }
    }
}
