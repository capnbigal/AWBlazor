using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>
/// Per-product, per-location stock counts. Maps onto the pre-existing
/// <c>Production.ProductInventory</c> table. Composite PK = (ProductID, LocationID).
/// SQL CHECK constraint restricts <c>Shelf</c> to 'A'–'Z' or 'N/A' and <c>Bin</c> to 0–100.
/// </summary>
[PrimaryKey(nameof(ProductId), nameof(LocationId))]
[Table("ProductInventory", Schema = "Production")]
public class ProductInventory
{
    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>FK to <c>Production.Location.LocationID</c>. Part of the composite PK.</summary>
    [Column("LocationID")]
    public short LocationId { get; set; }

    [Column("Shelf")]
    [MaxLength(10)]
    public string Shelf { get; set; } = string.Empty;

    [Column("Bin")]
    public byte Bin { get; set; }

    [Column("Quantity")]
    public short Quantity { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
