using System.Text;
using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Services.AdventureWorksAudit;

public static class PurchaseOrderDetailAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(PurchaseOrderDetail e) => new(e);

    public static PurchaseOrderDetailAuditLog RecordCreate(PurchaseOrderDetail e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static PurchaseOrderDetailAuditLog RecordUpdate(Snapshot before, PurchaseOrderDetail after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static PurchaseOrderDetailAuditLog RecordDelete(PurchaseOrderDetail e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static PurchaseOrderDetailAuditLog BuildLog(PurchaseOrderDetail e, string action, string? by, string? summary)
        => new()
        {
            PurchaseOrderId = e.PurchaseOrderId,
            PurchaseOrderDetailId = e.PurchaseOrderDetailId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            DueDate = e.DueDate,
            OrderQty = e.OrderQty,
            ProductId = e.ProductId,
            UnitPrice = e.UnitPrice,
            LineTotal = e.LineTotal,
            ReceivedQty = e.ReceivedQty,
            RejectedQty = e.RejectedQty,
            StockedQty = e.StockedQty,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, PurchaseOrderDetail after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "DueDate", before.DueDate, after.DueDate);
        AuditDiffHelpers.AppendIfChanged(sb, "OrderQty", before.OrderQty, after.OrderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", before.ProductId, after.ProductId);
        AuditDiffHelpers.AppendIfChanged(sb, "UnitPrice", before.UnitPrice, after.UnitPrice);
        AuditDiffHelpers.AppendIfChanged(sb, "ReceivedQty", before.ReceivedQty, after.ReceivedQty);
        AuditDiffHelpers.AppendIfChanged(sb, "RejectedQty", before.RejectedQty, after.RejectedQty);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        DateTime DueDate, short OrderQty, int ProductId, decimal UnitPrice,
        decimal ReceivedQty, decimal RejectedQty)
    {
        public Snapshot(PurchaseOrderDetail e) : this(
            e.DueDate, e.OrderQty, e.ProductId, e.UnitPrice,
            e.ReceivedQty, e.RejectedQty)
        { }
    }
}
