using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Line item in a customer's shopping cart. Maps onto the pre-existing <c>Sales.ShoppingCartItem</c> table. <c>ProductID</c> is an FK stored as a plain int. Note: this table has both a <see cref="DateCreated"/> timestamp (cart-item creation) and <see cref="ModifiedDate"/> (audit).</summary>
[Table("ShoppingCartItem", Schema = "Sales")]
public class ShoppingCartItem
{
    [Key]
    [Column("ShoppingCartItemID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Shopping cart correlation id (session/cookie-scoped).</summary>
    [Column("ShoppingCartID")]
    [MaxLength(50)]
    public string ShoppingCartId { get; set; } = string.Empty;

    [Column("Quantity")]
    public int Quantity { get; set; }

    /// <summary>Foreign key to <c>Production.Product.ProductID</c>.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>When the cart item was first added. Separate from <see cref="ModifiedDate"/>.</summary>
    [Column("DateCreated")]
    public DateTime DateCreated { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
