using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Audit;

public static class SalesOrderDetailAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesOrderDetail e) => new(e);

    public static SalesOrderDetailAuditLog RecordCreate(SalesOrderDetail e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesOrderDetailAuditLog RecordUpdate(Snapshot before, SalesOrderDetail after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesOrderDetailAuditLog RecordDelete(SalesOrderDetail e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesOrderDetailAuditLog BuildLog(SalesOrderDetail e, string action, string? by, string? summary)
        => new()
        {
            SalesOrderId = e.SalesOrderId,
            SalesOrderDetailId = e.SalesOrderDetailId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            CarrierTrackingNumber = e.CarrierTrackingNumber,
            OrderQty = e.OrderQty,
            ProductId = e.ProductId,
            SpecialOfferId = e.SpecialOfferId,
            UnitPrice = e.UnitPrice,
            UnitPriceDiscount = e.UnitPriceDiscount,
            LineTotal = e.LineTotal,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesOrderDetail after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "CarrierTrackingNumber", before.CarrierTrackingNumber, after.CarrierTrackingNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "OrderQty", before.OrderQty, after.OrderQty);
        AuditDiffHelpers.AppendIfChanged(sb, "UnitPrice", before.UnitPrice, after.UnitPrice);
        AuditDiffHelpers.AppendIfChanged(sb, "UnitPriceDiscount", before.UnitPriceDiscount, after.UnitPriceDiscount);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string? CarrierTrackingNumber, short OrderQty, decimal UnitPrice, decimal UnitPriceDiscount)
    {
        public Snapshot(SalesOrderDetail e) : this(
            e.CarrierTrackingNumber, e.OrderQty, e.UnitPrice, e.UnitPriceDiscount)
        { }
    }
}
