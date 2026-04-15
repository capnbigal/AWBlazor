using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Product standard-cost history. Maps onto the pre-existing <c>Production.ProductCostHistory</c> table. Composite PK = (ProductID, StartDate).</summary>
[PrimaryKey(nameof(ProductId), nameof(StartDate))]
[Table("ProductCostHistory", Schema = "Production")]
public class ProductCostHistory
{
    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>Cost effective date. Part of the composite PK.</summary>
    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("StandardCost", TypeName = "money")]
    public decimal StandardCost { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
