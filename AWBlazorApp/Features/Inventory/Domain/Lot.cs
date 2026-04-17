using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Domain;

/// <summary>
/// A production lot: one manufactured or received batch of an inventory item. Only meaningful
/// when the parent <see cref="InventoryItem.TracksLot"/> is true. Lot codes are unique per item,
/// not globally, so recycling codes across items is allowed (and common in practice).
/// </summary>
[Table("Lot", Schema = "inv")]
public class Lot
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    [MaxLength(64)]
    public string LotCode { get; set; } = string.Empty;

    public DateTime? ManufacturedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    /// <summary>FK → <c>Purchasing.Vendor.BusinessEntityID</c> for inbound lots.</summary>
    public int? VendorBusinessEntityId { get; set; }

    public LotStatus Status { get; set; } = LotStatus.Available;

    public DateTime ModifiedDate { get; set; }
}

public enum LotStatus : byte
{
    Available = 1,
    Hold = 2,
    Quarantine = 3,
    Scrap = 4,
}
