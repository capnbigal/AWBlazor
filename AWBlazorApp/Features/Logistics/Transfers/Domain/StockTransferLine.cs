using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Logistics.Transfers.Domain;

/// <summary>
/// One line on a <see cref="StockTransfer"/>. Carries both transaction-id back-pointers
/// so a reviewer can click through to either leg of the paired ledger event.
/// </summary>
[Table("StockTransferLine", Schema = "lgx")]
public class StockTransferLine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int StockTransferId { get; set; }

    public int InventoryItemId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }

    public long? FromTransactionId { get; set; }
    public long? ToTransactionId { get; set; }

    public DateTime ModifiedDate { get; set; }
}
