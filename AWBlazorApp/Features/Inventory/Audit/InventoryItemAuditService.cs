using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Inventory.Domain;
using System.Text;

namespace AWBlazorApp.Features.Inventory.Audit;

public static class InventoryItemAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(InventoryItem e) => new(e);

    public static InventoryItemAuditLog RecordCreate(InventoryItem e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static InventoryItemAuditLog RecordUpdate(Snapshot before, InventoryItem after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static InventoryItemAuditLog RecordDelete(InventoryItem e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static InventoryItemAuditLog BuildLog(InventoryItem e, string action, string? by, string? summary)
        => new()
        {
            InventoryItemId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ProductId = e.ProductId,
            TracksLot = e.TracksLot,
            TracksSerial = e.TracksSerial,
            DefaultLocationId = e.DefaultLocationId,
            MinQty = e.MinQty,
            MaxQty = e.MaxQty,
            ReorderPoint = e.ReorderPoint,
            ReorderQty = e.ReorderQty,
            IsActive = e.IsActive,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot b, InventoryItem a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "TracksLot", b.TracksLot, a.TracksLot);
        AuditDiffHelpers.AppendIfChanged(sb, "TracksSerial", b.TracksSerial, a.TracksSerial);
        AuditDiffHelpers.AppendIfChanged(sb, "DefaultLocationId", b.DefaultLocationId, a.DefaultLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "MinQty", b.MinQty, a.MinQty);
        AuditDiffHelpers.AppendIfChanged(sb, "MaxQty", b.MaxQty, a.MaxQty);
        AuditDiffHelpers.AppendIfChanged(sb, "ReorderPoint", b.ReorderPoint, a.ReorderPoint);
        AuditDiffHelpers.AppendIfChanged(sb, "ReorderQty", b.ReorderQty, a.ReorderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        bool TracksLot, bool TracksSerial, int? DefaultLocationId,
        decimal MinQty, decimal MaxQty, decimal ReorderPoint, decimal ReorderQty, bool IsActive)
    {
        public Snapshot(InventoryItem e) : this(
            e.TracksLot, e.TracksSerial, e.DefaultLocationId,
            e.MinQty, e.MaxQty, e.ReorderPoint, e.ReorderQty, e.IsActive) { }
    }
}
