using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Sales territory. Maps onto the pre-existing <c>Sales.SalesTerritory</c> table. The SQL column <c>Group</c> is a reserved word in T-SQL and is mapped to the C# property <see cref="GroupName"/>.</summary>
[Table("SalesTerritory", Schema = "Sales")]
public class SalesTerritory
{
    [Key]
    [Column("TerritoryID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Foreign key to <c>Person.CountryRegion.CountryRegionCode</c>.</summary>
    [Column("CountryRegionCode")]
    [MaxLength(3)]
    public string CountryRegionCode { get; set; } = string.Empty;

    /// <summary>Territory group (North America, Europe, Pacific). Mapped from the SQL column <c>Group</c>.</summary>
    [Column("Group")]
    [MaxLength(50)]
    public string GroupName { get; set; } = string.Empty;

    [Column("SalesYTD", TypeName = "money")]
    public decimal SalesYtd { get; set; }

    [Column("SalesLastYear", TypeName = "money")]
    public decimal SalesLastYear { get; set; }

    [Column("CostYTD", TypeName = "money")]
    public decimal CostYtd { get; set; }

    [Column("CostLastYear", TypeName = "money")]
    public decimal CostLastYear { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
