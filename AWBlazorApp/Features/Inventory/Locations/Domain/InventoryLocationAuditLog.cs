using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Inventory.Locations.Domain;

public class InventoryLocationAuditLog : AdventureWorksAuditLogBase
{
    public int InventoryLocationId { get; set; }

    public int OrganizationId { get; set; }
    public int? OrgUnitId { get; set; }
    [MaxLength(64)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    public InventoryLocationKind Kind { get; set; }
    public int? ParentLocationId { get; set; }
    [MaxLength(1024)] public string? Path { get; set; }
    public byte Depth { get; set; }
    public short? ProductionLocationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
