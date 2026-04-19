using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain;

/// <summary>Archived transaction history. Maps onto the pre-existing <c>Production.TransactionHistoryArchive</c> table. Single PK = TransactionID (NOT identity — archive rows carry their original id).</summary>
[Table("TransactionHistoryArchive", Schema = "Production")]
public class TransactionHistoryArchive
{
    [Key]
    [Column("TransactionID")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
