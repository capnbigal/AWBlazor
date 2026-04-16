using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>State / province lookup with FK references to <c>Person.CountryRegion</c> and <c>Sales.SalesTerritory</c>. Maps onto the pre-existing <c>Person.StateProvince</c> table. FKs are stored as plain scalars — no EF navigations.</summary>
[Table("StateProvince", Schema = "Person")]
public class StateProvince
{
    [Key]
    [Column("StateProvinceID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>State/province ISO code. SQL <c>nchar(3)</c>.</summary>
    [Column("StateProvinceCode")]
    [MaxLength(3)]
    public string StateProvinceCode { get; set; } = string.Empty;

    /// <summary>Foreign key to <c>Person.CountryRegion.CountryRegionCode</c>.</summary>
    [Column("CountryRegionCode")]
    [MaxLength(3)]
    public string CountryRegionCode { get; set; } = string.Empty;

    /// <summary>True when this state is the only one for its country (e.g. Vatican City).</summary>
    [Column("IsOnlyStateProvinceFlag")]
    public bool IsOnlyStateProvinceFlag { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Foreign key to <c>Sales.SalesTerritory.TerritoryID</c>.</summary>
    [Column("TerritoryID")]
    public int TerritoryId { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
