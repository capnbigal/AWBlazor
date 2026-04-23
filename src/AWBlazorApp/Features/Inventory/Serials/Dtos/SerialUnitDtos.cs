using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 

namespace AWBlazorApp.Features.Inventory.Serials.Dtos;

public sealed record SerialUnitDto(
    int Id, int InventoryItemId, int? LotId, string SerialNumber,
    SerialUnitStatus Status, int? CurrentLocationId, DateTime ModifiedDate);

public sealed record CreateSerialUnitRequest
{
    public int InventoryItemId { get; set; }
    public int? LotId { get; set; }
    public string? SerialNumber { get; set; }
    public SerialUnitStatus Status { get; set; } = SerialUnitStatus.InStock;
    public int? CurrentLocationId { get; set; }
}

public sealed record UpdateSerialUnitRequest
{
    public int? LotId { get; set; }
    public string? SerialNumber { get; set; }
    public SerialUnitStatus? Status { get; set; }
    public int? CurrentLocationId { get; set; }
}

public static class SerialUnitMappings
{
    public static SerialUnitDto ToDto(this SerialUnit e) => new(
        e.Id, e.InventoryItemId, e.LotId, e.SerialNumber, e.Status, e.CurrentLocationId, e.ModifiedDate);

    public static SerialUnit ToEntity(this CreateSerialUnitRequest r) => new()
    {
        InventoryItemId = r.InventoryItemId,
        LotId = r.LotId,
        SerialNumber = (r.SerialNumber ?? string.Empty).Trim(),
        Status = r.Status,
        CurrentLocationId = r.CurrentLocationId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSerialUnitRequest r, SerialUnit e)
    {
        if (r.LotId is not null) e.LotId = r.LotId;
        if (r.SerialNumber is not null) e.SerialNumber = r.SerialNumber.Trim();
        if (r.Status is not null) e.Status = r.Status.Value;
        if (r.CurrentLocationId is not null) e.CurrentLocationId = r.CurrentLocationId;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
