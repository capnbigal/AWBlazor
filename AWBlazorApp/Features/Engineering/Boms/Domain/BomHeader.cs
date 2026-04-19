using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Boms.Domain;

/// <summary>
/// Engineering bill of materials — the recipe for producing a <c>Production.Product</c>.
/// Deliberately separate from AdventureWorks' finance-oriented <c>Production.BillOfMaterials</c>
/// so we can add revision control + ECO linkage without disturbing the AW schema. The ECO
/// workflow activates a new revision by setting <see cref="IsActive"/> on the new row and
/// clearing it on prior rows for the same <see cref="ProductId"/>.
/// </summary>
[Table("BomHeader", Schema = "eng")]
public class BomHeader
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    /// <summary>FK → <c>Production.Product.ProductID</c>.</summary>
    public int ProductId { get; set; }

    public int RevisionNumber { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
