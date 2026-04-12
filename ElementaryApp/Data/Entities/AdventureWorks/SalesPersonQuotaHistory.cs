using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>History of sales-quota changes per sales person. Maps onto the pre-existing <c>Sales.SalesPersonQuotaHistory</c> table. Composite PK = (BusinessEntityID, QuotaDate).</summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(QuotaDate))]
[Table("SalesPersonQuotaHistory", Schema = "Sales")]
public class SalesPersonQuotaHistory
{
    /// <summary>Foreign key to <c>Sales.SalesPerson.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>Quota effective date. Part of the composite PK.</summary>
    [Column("QuotaDate")]
    public DateTime QuotaDate { get; set; }

    [Column("SalesQuota", TypeName = "money")]
    public decimal SalesQuota { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
