using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.ProductModelIllustrations.Domain;

/// <summary>Junction table linking product models to illustrations. Maps onto the pre-existing <c>Production.ProductModelIllustration</c> table. Composite PK = (ProductModelID, IllustrationID). No non-key data columns apart from <see cref="ModifiedDate"/>.</summary>
[PrimaryKey(nameof(ProductModelId), nameof(IllustrationId))]
[Table("ProductModelIllustration", Schema = "Production")]
public class ProductModelIllustration
{
    /// <summary>FK to <c>Production.ProductModel.ProductModelID</c>. Part of the composite PK.</summary>
    [Column("ProductModelID")]
    public int ProductModelId { get; set; }

    /// <summary>FK to <c>Production.Illustration.IllustrationID</c>. Part of the composite PK.</summary>
    [Column("IllustrationID")]
    public int IllustrationId { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
