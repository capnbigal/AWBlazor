using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 

namespace AWBlazorApp.Features.Inventory.Items.Dtos;

public sealed record InventoryItemDto(
    int Id, int ProductId, bool TracksLot, bool TracksSerial, int? DefaultLocationId,
    decimal MinQty, decimal MaxQty, decimal ReorderPoint, decimal ReorderQty,
    bool IsActive, DateTime ModifiedDate);

public sealed record CreateInventoryItemRequest
{
    public int ProductId { get; set; }
    public bool TracksLot { get; set; }
    public bool TracksSerial { get; set; }
    public int? DefaultLocationId { get; set; }
    public decimal MinQty { get; set; }
    public decimal MaxQty { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQty { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateInventoryItemRequest
{
    public bool? TracksLot { get; set; }
    public bool? TracksSerial { get; set; }
    public int? DefaultLocationId { get; set; }
    public decimal? MinQty { get; set; }
    public decimal? MaxQty { get; set; }
    public decimal? ReorderPoint { get; set; }
    public decimal? ReorderQty { get; set; }
    public bool? IsActive { get; set; }
}

public static class InventoryItemMappings
{
    public static InventoryItemDto ToDto(this InventoryItem e) => new(
        e.Id, e.ProductId, e.TracksLot, e.TracksSerial, e.DefaultLocationId,
        e.MinQty, e.MaxQty, e.ReorderPoint, e.ReorderQty, e.IsActive, e.ModifiedDate);

    public static InventoryItem ToEntity(this CreateInventoryItemRequest r) => new()
    {
        ProductId = r.ProductId,
        TracksLot = r.TracksLot,
        TracksSerial = r.TracksSerial,
        DefaultLocationId = r.DefaultLocationId,
        MinQty = r.MinQty,
        MaxQty = r.MaxQty,
        ReorderPoint = r.ReorderPoint,
        ReorderQty = r.ReorderQty,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateInventoryItemRequest r, InventoryItem e)
    {
        if (r.TracksLot is not null) e.TracksLot = r.TracksLot.Value;
        if (r.TracksSerial is not null) e.TracksSerial = r.TracksSerial.Value;
        if (r.DefaultLocationId is not null) e.DefaultLocationId = r.DefaultLocationId;
        if (r.MinQty is not null) e.MinQty = r.MinQty.Value;
        if (r.MaxQty is not null) e.MaxQty = r.MaxQty.Value;
        if (r.ReorderPoint is not null) e.ReorderPoint = r.ReorderPoint.Value;
        if (r.ReorderQty is not null) e.ReorderQty = r.ReorderQty.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
