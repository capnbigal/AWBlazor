using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Assembly-component relationship with effective-date range. Maps onto the pre-existing <c>Production.BillOfMaterials</c> table.</summary>
[Table("BillOfMaterials", Schema = "Production")]
public class BillOfMaterials
{
    [Key]
    [Column("BillOfMaterialsID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>. Null for top-level assemblies.</summary>
    [Column("ProductAssemblyID")]
    public int? ProductAssemblyId { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>. The component being used.</summary>
    [Column("ComponentID")]
    public int ComponentId { get; set; }

    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    /// <summary>FK to <c>Production.UnitMeasure.UnitMeasureCode</c>.</summary>
    [Column("UnitMeasureCode")]
    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    [Column("BOMLevel")]
    public short BomLevel { get; set; }

    [Column("PerAssemblyQty", TypeName = "decimal(8,2)")]
    public decimal PerAssemblyQty { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
