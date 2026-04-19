using AWBlazorApp.Features.Inventory.Domain;

namespace AWBlazorApp.Features.Inventory.Dtos;

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

public sealed record SerialUnitAuditLogDto(
    int Id, int SerialUnitId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int InventoryItemId, int? LotId, string? SerialNumber, SerialUnitStatus Status,
    int? CurrentLocationId, DateTime SourceModifiedDate);

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

    public static SerialUnitAuditLogDto ToDto(this SerialUnitAuditLog a) => new(
        a.Id, a.SerialUnitId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.InventoryItemId, a.LotId, a.SerialNumber, a.Status, a.CurrentLocationId, a.SourceModifiedDate);
}
