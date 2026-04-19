using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Logistics.Shipments.Domain;

public class ShipmentLineAuditLog : AdventureWorksAuditLogBase
{
    public int ShipmentLineId { get; set; }

    public int ShipmentId { get; set; }
    public int? SalesOrderDetailId { get; set; }
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }
    public long? PostedTransactionId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
