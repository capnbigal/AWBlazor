using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.CreditCards.Domain;

/// <summary>Credit card information. Maps onto the pre-existing <c>Sales.CreditCard</c> table. <c>CardNumber</c> has an AK unique constraint in SQL Server. Note: this column contains sensitive PCI data.</summary>
[Table("CreditCard", Schema = "Sales")]
public class CreditCard
{
    [Key]
    [Column("CreditCardID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("CardType")]
    [MaxLength(50)]
    public string CardType { get; set; } = string.Empty;

    /// <summary>Unique card number (AK constraint in SQL). Sensitive PCI data.</summary>
    [Column("CardNumber")]
    [MaxLength(25)]
    public string CardNumber { get; set; } = string.Empty;

    [Column("ExpMonth")]
    public byte ExpMonth { get; set; }

    [Column("ExpYear")]
    public short ExpYear { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
