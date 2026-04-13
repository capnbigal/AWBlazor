using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Sales order line items. Maps onto the pre-existing <c>Sales.SalesOrderDetail</c> table. Composite PK = (SalesOrderID, SalesOrderDetailID). <c>LineTotal</c> is a computed column (<c>UnitPrice * (1 - UnitPriceDiscount) * OrderQty</c>). <c>SalesOrderDetailID</c> is an identity column scoped per order.</summary>
[PrimaryKey(nameof(SalesOrderId), nameof(SalesOrderDetailId))]
[Table("SalesOrderDetail", Schema = "Sales")]
public class SalesOrderDetail
{
    /// <summary>FK to <c>Sales.SalesOrderHeader.SalesOrderID</c>. Part of the composite PK.</summary>
    [Column("SalesOrderID")]
    public int SalesOrderId { get; set; }

    /// <summary>Identity column within each order. Part of the composite PK.</summary>
    [Column("SalesOrderDetailID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SalesOrderDetailId { get; set; }

    [Column("CarrierTrackingNumber")]
    [System.ComponentModel.DataAnnotations.MaxLength(25)]
    public string? CarrierTrackingNumber { get; set; }

    [Column("OrderQty")]
    public short OrderQty { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>FK to <c>Sales.SpecialOffer.SpecialOfferID</c>.</summary>
    [Column("SpecialOfferID")]
    public int SpecialOfferId { get; set; }

    [Column("UnitPrice", TypeName = "money")]
    public decimal UnitPrice { get; set; }

    [Column("UnitPriceDiscount", TypeName = "money")]
    public decimal UnitPriceDiscount { get; set; }

    /// <summary>Computed by SQL Server — read-only from EF's perspective.</summary>
    [Column("LineTotal", TypeName = "numeric(38,6)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal LineTotal { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
