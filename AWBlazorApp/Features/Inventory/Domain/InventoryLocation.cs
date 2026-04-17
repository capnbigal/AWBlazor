using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Domain;

/// <summary>
/// Physical storage container: Warehouse → Zone → Bin, or the special staging/quarantine/scrap
/// locations. Self-referencing tree rooted at Warehouse rows; <see cref="Path"/> is the
/// materialized slash-separated code path from root → this node.
/// </summary>
[Table("InventoryLocation", Schema = "inv")]
public class InventoryLocation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    /// <summary>Plant/Area this location physically belongs to. Null for virtual locations (e.g. "in-transit").</summary>
    public int? OrgUnitId { get; set; }

    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public InventoryLocationKind Kind { get; set; }

    public int? ParentLocationId { get; set; }

    [MaxLength(1024)]
    public string Path { get; set; } = string.Empty;

    public byte Depth { get; set; }

    /// <summary>Optional hook into AdventureWorks' legacy <c>Production.Location</c> rows so historical WIP has a mapping.</summary>
    public short? ProductionLocationId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum InventoryLocationKind : byte
{
    Warehouse = 1,
    Zone = 2,
    Bin = 3,
    StagingIn = 4,
    StagingOut = 5,
    Quarantine = 6,
    Scrap = 7,
}
