using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>
/// Product list-price history. Maps onto the pre-existing
/// <c>Production.ProductListPriceHistory</c> table. Composite PK = (ProductID, StartDate).
/// </summary>
[PrimaryKey(nameof(ProductId), nameof(StartDate))]
[Table("ProductListPriceHistory", Schema = "Production")]
public class ProductListPriceHistory
{
    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>List-price effective date. Part of the composite PK.</summary>
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("ListPrice", TypeName = "money")]
    public decimal ListPrice { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
