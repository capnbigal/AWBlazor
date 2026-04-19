using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.TransactionHistories.Domain;

/// <summary>Transaction history for products (work orders, sales orders, purchase orders). Maps onto the pre-existing <c>Production.TransactionHistory</c> table. Single PK = TransactionID (identity).</summary>
[Table("TransactionHistory", Schema = "Production")]
public class TransactionHistory
{
    [Key]
    [Column("TransactionID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("ReferenceOrderID")]
    public int ReferenceOrderId { get; set; }

    [Column("ReferenceOrderLineID")]
    public int ReferenceOrderLineId { get; set; }

    /// <summary>Date of the transaction.</summary>
    [Column("TransactionDate")]
    public DateTime TransactionDate { get; set; }

    /// <summary>Transaction type: W = WorkOrder, S = SalesOrder, P = PurchaseOrder. nchar(1).</summary>
    [Column("TransactionType")]
    [MaxLength(1)]
    public string TransactionType { get; set; } = string.Empty;

    [Column("Quantity")]
    public int Quantity { get; set; }

    /// <summary>Cost per unit (money).</summary>
    [Column("ActualCost", TypeName = "money")]
    public decimal ActualCost { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
