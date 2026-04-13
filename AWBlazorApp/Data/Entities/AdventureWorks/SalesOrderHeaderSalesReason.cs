using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Junction table linking sales orders to the reasons they happened. Maps onto the pre-existing <c>Sales.SalesOrderHeaderSalesReason</c> table. Composite PK = (SalesOrderID, SalesReasonID). No non-key data columns apart from <see cref="ModifiedDate"/>.</summary>
[PrimaryKey(nameof(SalesOrderId), nameof(SalesReasonId))]
[Table("SalesOrderHeaderSalesReason", Schema = "Sales")]
public class SalesOrderHeaderSalesReason
{
    /// <summary>FK to <c>Sales.SalesOrderHeader.SalesOrderID</c>. Part of the composite PK.</summary>
    [Column("SalesOrderID")]
    public int SalesOrderId { get; set; }

    /// <summary>FK to <c>Sales.SalesReason.SalesReasonID</c>. Part of the composite PK.</summary>
    [Column("SalesReasonID")]
    public int SalesReasonId { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
