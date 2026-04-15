using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

public sealed record ProductInventoryDto(
    int ProductId, short LocationId, string Shelf, byte Bin, short Quantity,
    Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateProductInventoryRequest
{
    public int ProductId { get; set; }
    public short LocationId { get; set; }
    public string? Shelf { get; set; }
    public byte Bin { get; set; }
    public short Quantity { get; set; }
}

public sealed record UpdateProductInventoryRequest
{
    public string? Shelf { get; set; }
    public byte? Bin { get; set; }
    public short? Quantity { get; set; }
}

public sealed record ProductInventoryAuditLogDto(
    int Id, int ProductId, short LocationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Shelf, byte Bin, short Quantity, Guid RowGuid,
    DateTime SourceModifiedDate);

public static class ProductInventoryMappings
{
    public static ProductInventoryDto ToDto(this ProductInventory e) => new(
        e.ProductId, e.LocationId, e.Shelf, e.Bin, e.Quantity, e.RowGuid, e.ModifiedDate);

    public static ProductInventory ToEntity(this CreateProductInventoryRequest r) => new()
    {
        ProductId = r.ProductId,
        LocationId = r.LocationId,
        Shelf = (r.Shelf ?? string.Empty).Trim(),
        Bin = r.Bin,
        Quantity = r.Quantity,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductInventoryRequest r, ProductInventory e)
    {
        if (r.Shelf is not null) e.Shelf = r.Shelf.Trim();
        if (r.Bin.HasValue) e.Bin = r.Bin.Value;
        if (r.Quantity.HasValue) e.Quantity = r.Quantity.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductInventoryAuditLogDto ToDto(this ProductInventoryAuditLog a) => new(
        a.Id, a.ProductId, a.LocationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Shelf, a.Bin, a.Quantity, a.RowGuid, a.SourceModifiedDate);
}
