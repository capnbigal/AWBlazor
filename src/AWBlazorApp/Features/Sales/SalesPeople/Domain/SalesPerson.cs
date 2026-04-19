using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.SalesPeople.Domain;

/// <summary>Sales person compensation info. Maps onto the pre-existing <c>Sales.SalesPerson</c> table. The PK is <c>BusinessEntityID</c> and is <b>not</b> an identity column — it's shared with <c>HumanResources.Employee</c> / <c>Person.BusinessEntity</c>, so callers must supply the id explicitly on create.</summary>
[Table("SalesPerson", Schema = "Sales")]
public class SalesPerson
{
    /// <summary>Shared PK / FK to <c>Person.BusinessEntity.BusinessEntityID</c>. NOT an identity column.</summary>
    [Key]
    [Column("BusinessEntityID")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    /// <summary>Foreign key to <c>Sales.SalesTerritory.TerritoryID</c>. Null when unassigned.</summary>
    [Column("TerritoryID")]
    public int? TerritoryId { get; set; }

    /// <summary>Annual sales quota. SQL <c>money</c>.</summary>
    [Column("SalesQuota", TypeName = "money")]
    public decimal? SalesQuota { get; set; }

    [Column("Bonus", TypeName = "money")]
    public decimal Bonus { get; set; }

    [Column("CommissionPct", TypeName = "smallmoney")]
    public decimal CommissionPct { get; set; }

    [Column("SalesYTD", TypeName = "money")]
    public decimal SalesYtd { get; set; }

    [Column("SalesLastYear", TypeName = "money")]
    public decimal SalesLastYear { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
