using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Domain;

/// <summary>
/// Inventory metadata layer over <c>Production.Product</c>. One row per ProductId. Captures
/// traceability flags, reorder points, and the default putaway location. Existence of a row
/// here means "this product is managed in inventory"; absence means it's a master-data-only
/// product (e.g. a virtual service SKU).
/// </summary>
[Table("InventoryItem", Schema = "inv")]
public class InventoryItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK → <c>Production.Product.ProductID</c>. Unique — one inventory row per product.</summary>
    public int ProductId { get; set; }

    public bool TracksLot { get; set; }
    public bool TracksSerial { get; set; }

    public int? DefaultLocationId { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal MinQty { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal MaxQty { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal ReorderPoint { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal ReorderQty { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
