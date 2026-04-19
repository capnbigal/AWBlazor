using AWBlazorApp.Features.Purchasing.ProductVendors.Domain; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Domain; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Domain; using AWBlazorApp.Features.Purchasing.ShipMethods.Domain; using AWBlazorApp.Features.Purchasing.Vendors.Domain; 

namespace AWBlazorApp.Features.Purchasing.ShipMethods.Dtos;

public sealed record ShipMethodDto(int Id, string Name, decimal ShipBase, decimal ShipRate, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateShipMethodRequest
{
    public string? Name { get; set; }
    public decimal ShipBase { get; set; }
    public decimal ShipRate { get; set; }
}

public sealed record UpdateShipMethodRequest
{
    public string? Name { get; set; }
    public decimal? ShipBase { get; set; }
    public decimal? ShipRate { get; set; }
}

public sealed record ShipMethodAuditLogDto(
    int Id, int ShipMethodId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, decimal ShipBase, decimal ShipRate, Guid RowGuid, DateTime SourceModifiedDate);

public static class ShipMethodMappings
{
    public static ShipMethodDto ToDto(this ShipMethod e) => new(e.Id, e.Name, e.ShipBase, e.ShipRate, e.RowGuid, e.ModifiedDate);

    public static ShipMethod ToEntity(this CreateShipMethodRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ShipBase = r.ShipBase,
        ShipRate = r.ShipRate,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateShipMethodRequest r, ShipMethod e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.ShipBase.HasValue) e.ShipBase = r.ShipBase.Value;
        if (r.ShipRate.HasValue) e.ShipRate = r.ShipRate.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ShipMethodAuditLogDto ToDto(this ShipMethodAuditLog a) => new(
        a.Id, a.ShipMethodId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.ShipBase, a.ShipRate, a.RowGuid, a.SourceModifiedDate);
}
