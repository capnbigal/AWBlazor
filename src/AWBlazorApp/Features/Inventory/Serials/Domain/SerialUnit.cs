using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Serials.Domain;

/// <summary>
/// A single serialized unit of an inventory item. Only meaningful when the parent item's
/// <see cref="InventoryItem.TracksSerial"/> is true. Every serial has a current-location pointer
/// that the transaction posting logic keeps in sync — so serial history is walkable via the
/// transaction ledger, not a separate location-history table.
/// </summary>
[Table("SerialUnit", Schema = "inv")]
public class SerialUnit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    public int? LotId { get; set; }

    [MaxLength(128)]
    public string SerialNumber { get; set; } = string.Empty;

    public SerialUnitStatus Status { get; set; } = SerialUnitStatus.InStock;

    public int? CurrentLocationId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum SerialUnitStatus : byte
{
    InStock = 1,
    Issued = 2,
    Shipped = 3,
    Scrapped = 4,
    Returned = 5,
}
