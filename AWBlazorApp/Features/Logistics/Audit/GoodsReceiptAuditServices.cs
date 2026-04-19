using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Logistics.Domain;
using System.Text;

namespace AWBlazorApp.Features.Logistics.Audit;

public static class GoodsReceiptAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(GoodsReceipt e) => new(e);

    public static GoodsReceiptAuditLog RecordCreate(GoodsReceipt e, string? by) => Build(e, ActionCreated, by, "Created");
    public static GoodsReceiptAuditLog RecordUpdate(Snapshot before, GoodsReceipt after, string? by)
        => Build(after, ActionUpdated, by, Diff(before, after));
    public static GoodsReceiptAuditLog RecordDelete(GoodsReceipt e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static GoodsReceiptAuditLog Build(GoodsReceipt e, string action, string? by, string? summary) => new()
    {
        GoodsReceiptId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        ReceiptNumber = e.ReceiptNumber,
        PurchaseOrderId = e.PurchaseOrderId,
        VendorBusinessEntityId = e.VendorBusinessEntityId,
        ReceivedLocationId = e.ReceivedLocationId,
        Status = e.Status,
        ReceivedAt = e.ReceivedAt,
        PostedAt = e.PostedAt,
        PostedByUserId = e.PostedByUserId,
        Notes = e.Notes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, GoodsReceipt a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "PurchaseOrderId", b.PurchaseOrderId, a.PurchaseOrderId);
        AuditDiffHelpers.AppendIfChanged(sb, "VendorBusinessEntityId", b.VendorBusinessEntityId, a.VendorBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "ReceivedLocationId", b.ReceivedLocationId, a.ReceivedLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "ReceivedAt", b.ReceivedAt, a.ReceivedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "PostedAt", b.PostedAt, a.PostedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? PurchaseOrderId, int? VendorBusinessEntityId, int ReceivedLocationId,
        GoodsReceiptStatus Status, DateTime ReceivedAt, DateTime? PostedAt, string? Notes)
    {
        public Snapshot(GoodsReceipt e) : this(
            e.PurchaseOrderId, e.VendorBusinessEntityId, e.ReceivedLocationId,
            e.Status, e.ReceivedAt, e.PostedAt, e.Notes) { }
    }
}

public static class GoodsReceiptLineAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(GoodsReceiptLine e) => new(e);

    public static GoodsReceiptLineAuditLog RecordCreate(GoodsReceiptLine e, string? by) => Build(e, ActionCreated, by, "Created");
    public static GoodsReceiptLineAuditLog RecordUpdate(Snapshot before, GoodsReceiptLine after, string? by)
        => Build(after, ActionUpdated, by, Diff(before, after));
    public static GoodsReceiptLineAuditLog RecordDelete(GoodsReceiptLine e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static GoodsReceiptLineAuditLog Build(GoodsReceiptLine e, string action, string? by, string? summary) => new()
    {
        GoodsReceiptLineId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        GoodsReceiptId = e.GoodsReceiptId,
        PurchaseOrderDetailId = e.PurchaseOrderDetailId,
        InventoryItemId = e.InventoryItemId,
        Quantity = e.Quantity,
        UnitMeasureCode = e.UnitMeasureCode,
        LotId = e.LotId,
        PostedTransactionId = e.PostedTransactionId,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, GoodsReceiptLine a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "PurchaseOrderDetailId", b.PurchaseOrderDetailId, a.PurchaseOrderDetailId);
        AuditDiffHelpers.AppendIfChanged(sb, "InventoryItemId", b.InventoryItemId, a.InventoryItemId);
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", b.Quantity, a.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "UnitMeasureCode", b.UnitMeasureCode, a.UnitMeasureCode);
        AuditDiffHelpers.AppendIfChanged(sb, "LotId", b.LotId, a.LotId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? PurchaseOrderDetailId, int InventoryItemId, decimal Quantity, string UnitMeasureCode, int? LotId)
    {
        public Snapshot(GoodsReceiptLine e) : this(
            e.PurchaseOrderDetailId, e.InventoryItemId, e.Quantity, e.UnitMeasureCode, e.LotId) { }
    }
}
