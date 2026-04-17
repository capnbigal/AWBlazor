using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Logistics.Domain;

/// <summary>
/// One line on a <see cref="GoodsReceipt"/>. Optionally references the originating
/// <c>Purchasing.PurchaseOrderDetail</c> so the posting service can enforce partial-receipt
/// caps (sum of prior receipts + this line ≤ PO line's ordered quantity). <see cref="LotId"/>
/// and <see cref="PostedTransactionId"/> are back-populated by the posting service.
/// </summary>
[Table("GoodsReceiptLine", Schema = "lgx")]
public class GoodsReceiptLine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int GoodsReceiptId { get; set; }

    public int? PurchaseOrderDetailId { get; set; }

    public int InventoryItemId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    public int? LotId { get; set; }

    public long? PostedTransactionId { get; set; }

    public DateTime ModifiedDate { get; set; }
}
