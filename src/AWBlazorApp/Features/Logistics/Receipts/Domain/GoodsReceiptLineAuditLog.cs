using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Logistics.Receipts.Domain;

public class GoodsReceiptLineAuditLog : AdventureWorksAuditLogBase
{
    public int GoodsReceiptLineId { get; set; }

    public int GoodsReceiptId { get; set; }
    public int? PurchaseOrderDetailId { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
    public long? PostedTransactionId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
