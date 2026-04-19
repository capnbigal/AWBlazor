using AWBlazorApp.Features.Inventory.Domain;

namespace AWBlazorApp.Features.Inventory.Dtos;

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

public sealed record LotAuditLogDto(
    int Id, int LotId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int InventoryItemId, string? LotCode, DateTime? ManufacturedAt, DateTime? ReceivedAt,
    int? VendorBusinessEntityId, LotStatus Status, DateTime SourceModifiedDate);

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

    public static LotAuditLogDto ToDto(this LotAuditLog a) => new(
        a.Id, a.LotId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.InventoryItemId, a.LotCode, a.ManufacturedAt, a.ReceivedAt,
        a.VendorBusinessEntityId, a.Status, a.SourceModifiedDate);
}
