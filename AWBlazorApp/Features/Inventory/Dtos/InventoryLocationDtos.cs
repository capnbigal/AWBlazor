using AWBlazorApp.Features.Inventory.Domain;

namespace AWBlazorApp.Features.Inventory.Dtos;

public sealed record InventoryLocationDto(
    int Id, int OrganizationId, int? OrgUnitId, string Code, string Name,
    InventoryLocationKind Kind, int? ParentLocationId, string Path, byte Depth,
    short? ProductionLocationId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateInventoryLocationRequest
{
    public int OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public InventoryLocationKind Kind { get; set; }
    public int? ParentLocationId { get; set; }
    public short? ProductionLocationId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateInventoryLocationRequest
{
    public int? OrgUnitId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public InventoryLocationKind? Kind { get; set; }
    public int? ParentLocationId { get; set; }
    public short? ProductionLocationId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record InventoryLocationAuditLogDto(
    int Id, int InventoryLocationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int OrganizationId, int? OrgUnitId, string? Code, string? Name,
    InventoryLocationKind Kind, int? ParentLocationId, string? Path, byte Depth,
    short? ProductionLocationId, bool IsActive, DateTime SourceModifiedDate);

public static class InventoryLocationMappings
{
    public static InventoryLocationDto ToDto(this InventoryLocation e) => new(
        e.Id, e.OrganizationId, e.OrgUnitId, e.Code, e.Name, e.Kind, e.ParentLocationId,
        e.Path, e.Depth, e.ProductionLocationId, e.IsActive, e.ModifiedDate);

    public static InventoryLocation ToEntity(this CreateInventoryLocationRequest r) => new()
    {
        OrganizationId = r.OrganizationId,
        OrgUnitId = r.OrgUnitId,
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Kind = r.Kind,
        ParentLocationId = r.ParentLocationId,
        ProductionLocationId = r.ProductionLocationId,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateInventoryLocationRequest r, InventoryLocation e)
    {
        if (r.OrgUnitId is not null) e.OrgUnitId = r.OrgUnitId;
        if (r.Code is not null) e.Code = r.Code.Trim().ToUpperInvariant();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Kind is not null) e.Kind = r.Kind.Value;
        if (r.ParentLocationId is not null) e.ParentLocationId = r.ParentLocationId;
        if (r.ProductionLocationId is not null) e.ProductionLocationId = r.ProductionLocationId;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static InventoryLocationAuditLogDto ToDto(this InventoryLocationAuditLog a) => new(
        a.Id, a.InventoryLocationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.OrganizationId, a.OrgUnitId, a.Code, a.Name, a.Kind, a.ParentLocationId, a.Path, a.Depth,
        a.ProductionLocationId, a.IsActive, a.SourceModifiedDate);
}
