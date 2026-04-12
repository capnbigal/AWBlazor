using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Junction linking special offers to products. Maps onto the pre-existing <c>Sales.SpecialOfferProduct</c> table. Composite PK = (SpecialOfferID, ProductID).</summary>
[PrimaryKey(nameof(SpecialOfferId), nameof(ProductId))]
[Table("SpecialOfferProduct", Schema = "Sales")]
public class SpecialOfferProduct
{
    /// <summary>FK to <c>Sales.SpecialOffer.SpecialOfferID</c>. Part of the composite PK.</summary>
    [Column("SpecialOfferID")]
    public int SpecialOfferId { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
