using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Unit-of-measure lookup (inch, lb, kg, ...). Maps onto the pre-existing <c>Production.UnitMeasure</c> table in AdventureWorks2022.</summary>
[Table("UnitMeasure", Schema = "Production")]
public class UnitMeasure
{
    /// <summary>Unit code (fixed-length <c>nchar(3)</c>). This is the primary key — NOT an identity column.</summary>
    [Key]
    [Column("UnitMeasureCode")]
    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
