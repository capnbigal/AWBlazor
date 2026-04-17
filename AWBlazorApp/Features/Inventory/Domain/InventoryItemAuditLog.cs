using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.Inventory.Domain;

public class InventoryItemAuditLog : AdventureWorksAuditLogBase
{
    public int InventoryItemId { get; set; }

    public int ProductId { get; set; }
    public bool TracksLot { get; set; }
    public bool TracksSerial { get; set; }
    public int? DefaultLocationId { get; set; }
    public decimal MinQty { get; set; }
    public decimal MaxQty { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal ReorderQty { get; set; }
    public bool IsActive { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
