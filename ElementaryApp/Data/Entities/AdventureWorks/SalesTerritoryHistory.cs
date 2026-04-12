using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Tracks sales-person territory assignments over time. Maps onto the pre-existing <c>Sales.SalesTerritoryHistory</c> table. Composite PK = (BusinessEntityID, StartDate, TerritoryID).</summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(StartDate), nameof(TerritoryId))]
[Table("SalesTerritoryHistory", Schema = "Sales")]
public class SalesTerritoryHistory
{
    /// <summary>FK to <c>Sales.SalesPerson.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>FK to <c>Sales.SalesTerritory.TerritoryID</c>. Part of the composite PK.</summary>
    [Column("TerritoryID")]
    public int TerritoryId { get; set; }

    /// <summary>Start date of the assignment. Part of the composite PK.</summary>
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    /// <summary>End date — null when the assignment is still current.</summary>
    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
