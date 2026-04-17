using AWBlazorApp.Features.AdventureWorks.Audit;
using AWBlazorApp.Features.Logistics.Domain;
using System.Text;

namespace AWBlazorApp.Features.Logistics.Audit;

public static class ShipmentAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(Shipment e) => new(e);

    public static ShipmentAuditLog RecordCreate(Shipment e, string? by) => Build(e, ActionCreated, by, "Created");
    public static ShipmentAuditLog RecordUpdate(Snapshot before, Shipment after, string? by)
        => Build(after, ActionUpdated, by, Diff(before, after));
    public static ShipmentAuditLog RecordDelete(Shipment e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static ShipmentAuditLog Build(Shipment e, string action, string? by, string? summary) => new()
    {
        ShipmentId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        ShipmentNumber = e.ShipmentNumber,
        SalesOrderId = e.SalesOrderId,
        CustomerBusinessEntityId = e.CustomerBusinessEntityId,
        ShipMethodId = e.ShipMethodId,
        TrackingNumber = e.TrackingNumber,
        ShippedFromLocationId = e.ShippedFromLocationId,
        Status = e.Status,
        ShippedAt = e.ShippedAt,
        DeliveredAt = e.DeliveredAt,
        PostedAt = e.PostedAt,
        PostedByUserId = e.PostedByUserId,
        Notes = e.Notes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, Shipment a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "SalesOrderId", b.SalesOrderId, a.SalesOrderId);
        AuditDiffHelpers.AppendIfChanged(sb, "CustomerBusinessEntityId", b.CustomerBusinessEntityId, a.CustomerBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "ShipMethodId", b.ShipMethodId, a.ShipMethodId);
        AuditDiffHelpers.AppendIfChanged(sb, "TrackingNumber", b.TrackingNumber, a.TrackingNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "ShippedFromLocationId", b.ShippedFromLocationId, a.ShippedFromLocationId);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "ShippedAt", b.ShippedAt, a.ShippedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "DeliveredAt", b.DeliveredAt, a.DeliveredAt);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        int? SalesOrderId, int? CustomerBusinessEntityId, int? ShipMethodId, string? TrackingNumber,
        int ShippedFromLocationId, ShipmentStatus Status, DateTime? ShippedAt, DateTime? DeliveredAt)
    {
        public Snapshot(Shipment e) : this(
            e.SalesOrderId, e.CustomerBusinessEntityId, e.ShipMethodId, e.TrackingNumber,
            e.ShippedFromLocationId, e.Status, e.ShippedAt, e.DeliveredAt) { }
    }
}

public static class ShipmentLineAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ShipmentLine e) => new(e);

    public static ShipmentLineAuditLog RecordCreate(ShipmentLine e, string? by) => Build(e, ActionCreated, by, "Created");
    public static ShipmentLineAuditLog RecordUpdate(Snapshot before, ShipmentLine after, string? by)
        => Build(after, ActionUpdated, by, Diff(before, after));
    public static ShipmentLineAuditLog RecordDelete(ShipmentLine e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static ShipmentLineAuditLog Build(ShipmentLine e, string action, string? by, string? summary) => new()
    {
        ShipmentLineId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        ShipmentId = e.ShipmentId,
        SalesOrderDetailId = e.SalesOrderDetailId,
        InventoryItemId = e.InventoryItemId,
        Quantity = e.Quantity,
        UnitMeasureCode = e.UnitMeasureCode,
        LotId = e.LotId,
        SerialUnitId = e.SerialUnitId,
        PostedTransactionId = e.PostedTransactionId,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, ShipmentLine a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", b.Quantity, a.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "LotId", b.LotId, a.LotId);
        AuditDiffHelpers.AppendIfChanged(sb, "SerialUnitId", b.SerialUnitId, a.SerialUnitId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(decimal Quantity, int? LotId, int? SerialUnitId)
    {
        public Snapshot(ShipmentLine e) : this(e.Quantity, e.LotId, e.SerialUnitId) { }
    }
}
