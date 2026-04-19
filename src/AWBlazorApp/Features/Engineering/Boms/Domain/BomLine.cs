using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Boms.Domain;

/// <summary>A single component row in a <see cref="BomHeader"/>.</summary>
[Table("BomLine", Schema = "eng")]
public class BomLine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int BomHeaderId { get; set; }

    /// <summary>FK → <c>Production.Product.ProductID</c> of the component.</summary>
    public int ComponentProductId { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal Quantity { get; set; }

    [MaxLength(3)] public string UnitMeasureCode { get; set; } = "EA";

    /// <summary>Percentage of expected scrap (0-1, e.g. 0.05 = 5%). Used by MRP for demand planning.</summary>
    [Column(TypeName = "decimal(5,4)")] public decimal ScrapPercentage { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
