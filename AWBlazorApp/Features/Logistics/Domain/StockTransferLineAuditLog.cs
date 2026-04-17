using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Logistics.Domain;

public class StockTransferLineAuditLog : AdventureWorksAuditLogBase
{
    public int StockTransferLineId { get; set; }

    public int StockTransferId { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
    public long? FromTransactionId { get; set; }
    public long? ToTransactionId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
