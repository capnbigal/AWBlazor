using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Purchase order line item. Maps onto the pre-existing <c>Purchasing.PurchaseOrderDetail</c> table. Composite PK = (PurchaseOrderID, PurchaseOrderDetailID). <c>LineTotal</c> and <c>StockedQty</c> are computed columns — EF never writes them.</summary>
[PrimaryKey(nameof(PurchaseOrderId), nameof(PurchaseOrderDetailId))]
[Table("PurchaseOrderDetail", Schema = "Purchasing")]
public class PurchaseOrderDetail
{
    /// <summary>FK to <c>Purchasing.PurchaseOrderHeader.PurchaseOrderID</c>. Part of the composite PK.</summary>
    [Column("PurchaseOrderID")]
    public int PurchaseOrderId { get; set; }

    /// <summary>Identity within the order. Part of the composite PK.</summary>
    [Column("PurchaseOrderDetailID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PurchaseOrderDetailId { get; set; }

    [Column("DueDate")]
    public DateTime DueDate { get; set; }

    [Column("OrderQty")]
    public short OrderQty { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("UnitPrice", TypeName = "money")]
    public decimal UnitPrice { get; set; }

    /// <summary>Computed in SQL as <c>OrderQty * UnitPrice</c>. Read-only.</summary>
    [Column("LineTotal", TypeName = "money")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal LineTotal { get; set; }

    [Column("ReceivedQty")]
    public decimal ReceivedQty { get; set; }

    [Column("RejectedQty")]
    public decimal RejectedQty { get; set; }

    /// <summary>Computed in SQL as <c>ReceivedQty - RejectedQty</c>. Read-only.</summary>
    [Column("StockedQty")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal StockedQty { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
