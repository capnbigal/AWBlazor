using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>
/// Junction linking products to their photos. Maps onto the pre-existing
/// <c>Production.ProductProductPhoto</c> table. Composite PK = (ProductID, ProductPhotoID).
/// The <c>Primary</c> flag marks one photo per product as the primary display image.
/// </summary>
[PrimaryKey(nameof(ProductId), nameof(ProductPhotoId))]
[Table("ProductProductPhoto", Schema = "Production")]
public class ProductProductPhoto
{
    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>FK to <c>Production.ProductPhoto.ProductPhotoID</c>. Part of the composite PK.</summary>
    [Column("ProductPhotoID")]
    public int ProductPhotoId { get; set; }

    /// <summary>True for the product's primary display photo. Only one row per product should have this set.</summary>
    [Column("Primary")]
    public bool Primary { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
