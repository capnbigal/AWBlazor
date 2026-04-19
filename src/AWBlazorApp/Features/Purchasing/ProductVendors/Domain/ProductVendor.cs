using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Purchasing.ProductVendors.Domain;

/// <summary>Product-to-vendor cross-reference. Maps onto the pre-existing <c>Purchasing.ProductVendor</c> table. Composite PK = (ProductID, BusinessEntityID).</summary>
[PrimaryKey(nameof(ProductId), nameof(BusinessEntityId))]
[Table("ProductVendor", Schema = "Purchasing")]
public class ProductVendor
{
    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>FK to <c>Purchasing.Vendor.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    [Column("AverageLeadTime")]
    public int AverageLeadTime { get; set; }

    [Column("StandardPrice", TypeName = "money")]
    public decimal StandardPrice { get; set; }

    [Column("LastReceiptCost", TypeName = "money")]
    public decimal? LastReceiptCost { get; set; }

    [Column("LastReceiptDate")]
    public DateTime? LastReceiptDate { get; set; }

    [Column("MinOrderQty")]
    public int MinOrderQty { get; set; }

    [Column("MaxOrderQty")]
    public int MaxOrderQty { get; set; }

    [Column("OnOrderQty")]
    public int? OnOrderQty { get; set; }

    [Column("UnitMeasureCode")]
    [MaxLength(3)]
    public string UnitMeasureCode { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
