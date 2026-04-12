using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Junction table linking product models to product descriptions for a specific culture. Maps onto the pre-existing <c>Production.ProductModelProductDescriptionCulture</c> table. 3-column composite PK = (ProductModelID, ProductDescriptionID, CultureID). No non-key data columns apart from <see cref="ModifiedDate"/>.</summary>
[PrimaryKey(nameof(ProductModelId), nameof(ProductDescriptionId), nameof(CultureId))]
[Table("ProductModelProductDescriptionCulture", Schema = "Production")]
public class ProductModelProductDescriptionCulture
{
    /// <summary>FK to <c>Production.ProductModel.ProductModelID</c>. Part of the composite PK.</summary>
    [Column("ProductModelID")]
    public int ProductModelId { get; set; }

    /// <summary>FK to <c>Production.ProductDescription.ProductDescriptionID</c>. Part of the composite PK.</summary>
    [Column("ProductDescriptionID")]
    public int ProductDescriptionId { get; set; }

    /// <summary>FK to <c>Production.Culture.CultureID</c>. Part of the composite PK. nchar(6).</summary>
    [Column("CultureID")]
    [MaxLength(6)]
    public string CultureId { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
