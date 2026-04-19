using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Reports.Domain;

/// <summary>
/// The derived on-hand aggregate: one row per (Item, Location, Lot, Status) tuple. Updated
/// atomically by <c>IInventoryService.PostTransactionAsync</c> when a transaction posts — never
/// written to directly. Putting <c>Status</c> in the unique key means Quarantined/Hold stock
/// shows as its own row rather than blending into Available.
/// </summary>
[Table("InventoryBalance", Schema = "inv")]
public class InventoryBalance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int InventoryItemId { get; set; }
    public int LocationId { get; set; }
    public int? LotId { get; set; }

    public BalanceStatus Status { get; set; } = BalanceStatus.Available;

    [Column(TypeName = "decimal(18,4)")] public decimal Quantity { get; set; }

    public DateTime? LastCountedAt { get; set; }
    public DateTime? LastTransactionAt { get; set; }
}

public enum BalanceStatus : byte
{
    Available = 1,
    Hold = 2,
    Quarantine = 3,
}
