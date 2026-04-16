using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>
/// Manufactured / sold products. Maps onto the pre-existing <c>Production.Product</c> table —
/// the most-referenced table in AdventureWorks. SQL CHECK constraints restrict
/// <c>Class</c> to (H, M, L), <c>Style</c> to (W, M, U), and <c>ProductLine</c> to (R, M, T, S).
/// All four are validated at the FluentValidation layer; the dialog UI uses MudSelect dropdowns
/// to surface them as labelled options.
/// </summary>
[Table("Product", Schema = "Production")]
public class Product
{
    [Key]
    [Column("ProductID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ProductNumber")]
    [MaxLength(25)]
    public string ProductNumber { get; set; } = string.Empty;

    [Column("MakeFlag")]
    public bool MakeFlag { get; set; }

    [Column("FinishedGoodsFlag")]
    public bool FinishedGoodsFlag { get; set; }

    [Column("Color")]
    [MaxLength(15)]
    public string? Color { get; set; }

    [Column("SafetyStockLevel")]
    public short SafetyStockLevel { get; set; }

    [Column("ReorderPoint")]
    public short ReorderPoint { get; set; }

    [Column("StandardCost", TypeName = "money")]
    public decimal StandardCost { get; set; }

    [Column("ListPrice", TypeName = "money")]
    public decimal ListPrice { get; set; }

    [Column("Size")]
    [MaxLength(5)]
    public string? Size { get; set; }

    /// <summary>FK to <c>Production.UnitMeasure.UnitMeasureCode</c>.</summary>
    [Column("SizeUnitMeasureCode")]
    [MaxLength(3)]
    public string? SizeUnitMeasureCode { get; set; }

    /// <summary>FK to <c>Production.UnitMeasure.UnitMeasureCode</c>.</summary>
    [Column("WeightUnitMeasureCode")]
    [MaxLength(3)]
    public string? WeightUnitMeasureCode { get; set; }

    [Column("Weight", TypeName = "decimal(8,2)")]
    public decimal? Weight { get; set; }

    [Column("DaysToManufacture")]
    public int DaysToManufacture { get; set; }

    /// <summary>SQL CHECK: must be one of R (Road), M (Mountain), T (Touring), S (Standard).</summary>
    [Column("ProductLine")]
    [MaxLength(2)]
    public string? ProductLine { get; set; }

    /// <summary>SQL CHECK: must be one of H (High), M (Medium), L (Low).</summary>
    [Column("Class")]
    [MaxLength(2)]
    public string? Class { get; set; }

    /// <summary>SQL CHECK: must be one of W (Womens), M (Mens), U (Universal).</summary>
    [Column("Style")]
    [MaxLength(2)]
    public string? Style { get; set; }

    /// <summary>FK to <c>Production.ProductSubcategory.ProductSubcategoryID</c>.</summary>
    [Column("ProductSubcategoryID")]
    public int? ProductSubcategoryId { get; set; }

    /// <summary>FK to <c>Production.ProductModel.ProductModelID</c>.</summary>
    [Column("ProductModelID")]
    public int? ProductModelId { get; set; }

    [Column("SellStartDate")]
    public DateTime SellStartDate { get; set; }

    [Column("SellEndDate")]
    public DateTime? SellEndDate { get; set; }

    [Column("DiscontinuedDate")]
    public DateTime? DiscontinuedDate { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
