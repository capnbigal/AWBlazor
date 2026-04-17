using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Domain;

/// <summary>
/// A reason-coded wrapper around an ADJUST transaction. Adjustments sit in Draft / PendingApproval
/// until approved, at which point they post exactly one <see cref="InventoryTransaction"/> (id
/// stored in <see cref="PostedTransactionId"/>) and flip to Posted. Rejected adjustments never
/// post a transaction.
/// </summary>
[Table("InventoryAdjustment", Schema = "inv")]
public class InventoryAdjustment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)]
    public string AdjustmentNumber { get; set; } = string.Empty;

    public int InventoryItemId { get; set; }

    public int LocationId { get; set; }

    public int? LotId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal QuantityDelta { get; set; }

    public AdjustmentReason ReasonCode { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Draft;

    [MaxLength(450)]
    public string? RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }

    [MaxLength(450)]
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Set when <see cref="Status"/> = <see cref="AdjustmentStatus.Posted"/>.</summary>
    public long? PostedTransactionId { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum AdjustmentReason : byte
{
    Damaged = 1,
    Lost = 2,
    Found = 3,
    Count = 4,
    Obsolete = 5,
    Other = 6,
}

public enum AdjustmentStatus : byte
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Posted = 5,
}
