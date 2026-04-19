using AWBlazorApp.Features.Logistics.Domain;

namespace AWBlazorApp.Features.Logistics.Dtos;

public sealed record ShipmentDto(
    int Id, string ShipmentNumber, int? SalesOrderId, int? CustomerBusinessEntityId,
    int? ShipMethodId, string? TrackingNumber, int ShippedFromLocationId, ShipmentStatus Status,
    DateTime? ShippedAt, DateTime? DeliveredAt, DateTime? PostedAt, string? PostedByUserId,
    string? Notes, DateTime ModifiedDate);

public sealed record CreateShipmentRequest
{
    public int? SalesOrderId { get; set; }
    public int? CustomerBusinessEntityId { get; set; }
    public int? ShipMethodId { get; set; }
    public string? TrackingNumber { get; set; }
    public int ShippedFromLocationId { get; set; }
    public string? Notes { get; set; }
}

public sealed record UpdateShipmentRequest
{
    public int? SalesOrderId { get; set; }
    public int? CustomerBusinessEntityId { get; set; }
    public int? ShipMethodId { get; set; }
    public string? TrackingNumber { get; set; }
    public int? ShippedFromLocationId { get; set; }
    public ShipmentStatus? Status { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record ShipmentAuditLogDto(
    int Id, int ShipmentId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? ShipmentNumber, int? SalesOrderId, int? CustomerBusinessEntityId, int? ShipMethodId,
    string? TrackingNumber, int ShippedFromLocationId, ShipmentStatus Status,
    DateTime? ShippedAt, DateTime? DeliveredAt, DateTime? PostedAt, string? PostedByUserId,
    string? Notes, DateTime SourceModifiedDate);

public sealed record ShipmentLineDto(
    int Id, int ShipmentId, int? SalesOrderDetailId, int InventoryItemId,
    decimal Quantity, string UnitMeasureCode, int? LotId, int? SerialUnitId,
    long? PostedTransactionId, DateTime ModifiedDate);

public sealed record CreateShipmentLineRequest
{
    public int ShipmentId { get; set; }
    public int? SalesOrderDetailId { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
}

public sealed record UpdateShipmentLineRequest
{
    public decimal? Quantity { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
}

public sealed record ShipmentLineAuditLogDto(
    int Id, int ShipmentLineId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int ShipmentId, int? SalesOrderDetailId, int InventoryItemId,
    decimal Quantity, string? UnitMeasureCode, int? LotId, int? SerialUnitId,
    long? PostedTransactionId, DateTime SourceModifiedDate);

public static class ShipmentMappings
{
    public static ShipmentDto ToDto(this Shipment e) => new(
        e.Id, e.ShipmentNumber, e.SalesOrderId, e.CustomerBusinessEntityId,
        e.ShipMethodId, e.TrackingNumber, e.ShippedFromLocationId, e.Status,
        e.ShippedAt, e.DeliveredAt, e.PostedAt, e.PostedByUserId, e.Notes, e.ModifiedDate);

    public static Shipment ToEntity(this CreateShipmentRequest r)
    {
        var now = DateTime.UtcNow;
        return new Shipment
        {
            ShipmentNumber = $"SHP-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            SalesOrderId = r.SalesOrderId,
            CustomerBusinessEntityId = r.CustomerBusinessEntityId,
            ShipMethodId = r.ShipMethodId,
            TrackingNumber = r.TrackingNumber?.Trim(),
            ShippedFromLocationId = r.ShippedFromLocationId,
            Status = ShipmentStatus.Draft,
            Notes = r.Notes?.Trim(),
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateShipmentRequest r, Shipment e)
    {
        if (r.SalesOrderId is not null) e.SalesOrderId = r.SalesOrderId;
        if (r.CustomerBusinessEntityId is not null) e.CustomerBusinessEntityId = r.CustomerBusinessEntityId;
        if (r.ShipMethodId is not null) e.ShipMethodId = r.ShipMethodId;
        if (r.TrackingNumber is not null) e.TrackingNumber = r.TrackingNumber.Trim();
        if (r.ShippedFromLocationId is not null) e.ShippedFromLocationId = r.ShippedFromLocationId.Value;
        if (r.Status is not null) e.Status = r.Status.Value;
        if (r.ShippedAt is not null) e.ShippedAt = r.ShippedAt;
        if (r.DeliveredAt is not null) e.DeliveredAt = r.DeliveredAt;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ShipmentAuditLogDto ToDto(this ShipmentAuditLog a) => new(
        a.Id, a.ShipmentId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ShipmentNumber, a.SalesOrderId, a.CustomerBusinessEntityId, a.ShipMethodId,
        a.TrackingNumber, a.ShippedFromLocationId, a.Status, a.ShippedAt, a.DeliveredAt,
        a.PostedAt, a.PostedByUserId, a.Notes, a.SourceModifiedDate);

    public static ShipmentLineDto ToDto(this ShipmentLine e) => new(
        e.Id, e.ShipmentId, e.SalesOrderDetailId, e.InventoryItemId,
        e.Quantity, e.UnitMeasureCode, e.LotId, e.SerialUnitId, e.PostedTransactionId, e.ModifiedDate);

    public static ShipmentLine ToEntity(this CreateShipmentLineRequest r) => new()
    {
        ShipmentId = r.ShipmentId,
        SalesOrderDetailId = r.SalesOrderDetailId,
        InventoryItemId = r.InventoryItemId,
        Quantity = r.Quantity,
        UnitMeasureCode = (r.UnitMeasureCode ?? "EA").Trim().ToUpperInvariant(),
        LotId = r.LotId,
        SerialUnitId = r.SerialUnitId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateShipmentLineRequest r, ShipmentLine e)
    {
        if (r.Quantity is not null) e.Quantity = r.Quantity.Value;
        if (r.LotId is not null) e.LotId = r.LotId;
        if (r.SerialUnitId is not null) e.SerialUnitId = r.SerialUnitId;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ShipmentLineAuditLogDto ToDto(this ShipmentLineAuditLog a) => new(
        a.Id, a.ShipmentLineId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ShipmentId, a.SalesOrderDetailId, a.InventoryItemId,
        a.Quantity, a.UnitMeasureCode, a.LotId, a.SerialUnitId, a.PostedTransactionId, a.SourceModifiedDate);
}
