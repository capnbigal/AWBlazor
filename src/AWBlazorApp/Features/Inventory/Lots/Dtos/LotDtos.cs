using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 

namespace AWBlazorApp.Features.Inventory.Lots.Dtos;

public sealed record LotDto(
    int Id, int InventoryItemId, string LotCode, DateTime? ManufacturedAt, DateTime? ReceivedAt,
    int? VendorBusinessEntityId, LotStatus Status, DateTime ModifiedDate);

public sealed record CreateLotRequest
{
    public int InventoryItemId { get; set; }
    public string? LotCode { get; set; }
    public DateTime? ManufacturedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public LotStatus Status { get; set; } = LotStatus.Available;
}

public sealed record UpdateLotRequest
{
    public string? LotCode { get; set; }
    public DateTime? ManufacturedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public LotStatus? Status { get; set; }
}

public static class LotMappings
{
    public static LotDto ToDto(this Lot e) => new(
        e.Id, e.InventoryItemId, e.LotCode, e.ManufacturedAt, e.ReceivedAt,
        e.VendorBusinessEntityId, e.Status, e.ModifiedDate);

    public static Lot ToEntity(this CreateLotRequest r) => new()
    {
        InventoryItemId = r.InventoryItemId,
        LotCode = (r.LotCode ?? string.Empty).Trim(),
        ManufacturedAt = r.ManufacturedAt,
        ReceivedAt = r.ReceivedAt,
        VendorBusinessEntityId = r.VendorBusinessEntityId,
        Status = r.Status,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateLotRequest r, Lot e)
    {
        if (r.LotCode is not null) e.LotCode = r.LotCode.Trim();
        if (r.ManufacturedAt is not null) e.ManufacturedAt = r.ManufacturedAt;
        if (r.ReceivedAt is not null) e.ReceivedAt = r.ReceivedAt;
        if (r.VendorBusinessEntityId is not null) e.VendorBusinessEntityId = r.VendorBusinessEntityId;
        if (r.Status is not null) e.Status = r.Status.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
