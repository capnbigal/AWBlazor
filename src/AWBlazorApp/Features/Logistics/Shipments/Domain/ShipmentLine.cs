using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Logistics.Shipments.Domain;

/// <summary>
/// One line on a <see cref="Shipment"/>. Links back to the originating
/// <c>Sales.SalesOrderDetail</c> so downstream analytics can walk "orders vs shipments"
/// without a heuristic join.
/// </summary>
[Table("ShipmentLine", Schema = "lgx")]
public class ShipmentLine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ShipmentId { get; set; }

    public int? SalesOrderDetailId { get; set; }

    public int InventoryItemId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    public int? LotId { get; set; }
    public int? SerialUnitId { get; set; }

    public long? PostedTransactionId { get; set; }

    public DateTime ModifiedDate { get; set; }
}
