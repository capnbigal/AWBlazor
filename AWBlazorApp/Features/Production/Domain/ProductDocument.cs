using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>
/// Junction linking products to documents. Maps onto the pre-existing
/// <c>Production.ProductDocument</c> table. Composite PK = (ProductID, DocumentNode).
/// The <c>DocumentNode</c> component is SQL <c>hierarchyid</c>.
/// </summary>
[PrimaryKey(nameof(ProductId), nameof(DocumentNode))]
[Table("ProductDocument", Schema = "Production")]
public class ProductDocument
{
    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>FK to <c>Production.Document.DocumentNode</c>. Part of the composite PK. SQL <c>hierarchyid</c>.</summary>
    [Column("DocumentNode")]
    public HierarchyId DocumentNode { get; set; } = HierarchyId.GetRoot();

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
