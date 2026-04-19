using AWBlazorApp.Features.Inventory.Reports.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Transactions.Domain;

/// <summary>
/// The append-only stock-movement ledger. Every material event — receipt, pick, ship, move,
/// adjust, WIP — writes one row here. Never updated, never deleted: the "correction" for a
/// wrong posting is a compensating transaction, not a DELETE. <c>CorrelationId</c> ties the two
/// legs of a MOVE (or any other paired type) together.
/// </summary>
[Table("InventoryTransaction", Schema = "inv")]
public class InventoryTransaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [MaxLength(32)]
    public string TransactionNumber { get; set; } = string.Empty;

    public int TransactionTypeId { get; set; }

    public DateTime OccurredAt { get; set; }
    public DateTime PostedAt { get; set; }

    [MaxLength(450)]
    public string? PostedByUserId { get; set; }

    public int InventoryItemId { get; set; }

    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }

    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    /// <summary>FK → <c>Production.UnitMeasure.UnitMeasureCode</c> — a string key in AW.</summary>
    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    public BalanceStatus? FromStatus { get; set; }
    public BalanceStatus? ToStatus { get; set; }

    public TransactionReferenceKind? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int? ReferenceLineId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid? CorrelationId { get; set; }
}

public enum TransactionReferenceKind : byte
{
    SalesOrder = 1,
    PurchaseOrder = 2,
    WorkOrder = 3,
    Adjustment = 4,
    Transfer = 5,
    Manual = 6,
}
