using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.Domain;

/// <summary>
/// Spare parts catalog. Many maintenance spares aren't in <c>Production.Product</c>
/// (shop consumables, OEM parts not sold), so this table stands alone with an optional
/// <see cref="ProductId"/> link for spares that are also AW products.
/// </summary>
[Table("SparePart", Schema = "maint")]
public class SparePart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)] public string PartNumber { get; set; } = string.Empty;

    [MaxLength(200)] public string Name { get; set; } = string.Empty;

    [MaxLength(2000)] public string? Description { get; set; }

    /// <summary>FK → <c>Production.Product.ProductID</c>. Null when the spare isn't in AW's catalog.</summary>
    public int? ProductId { get; set; }

    [MaxLength(3)] public string UnitMeasureCode { get; set; } = "EA";

    [Column(TypeName = "decimal(18,4)")] public decimal? StandardCost { get; set; }

    public int? ReorderPoint { get; set; }
    public int? ReorderQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
