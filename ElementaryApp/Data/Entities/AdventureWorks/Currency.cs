using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>ISO currency codes. Maps onto the pre-existing <c>Sales.Currency</c> table in AdventureWorks2022.</summary>
[Table("Currency", Schema = "Sales")]
public class Currency
{
    /// <summary>ISO currency code (fixed-length <c>nchar(3)</c>). This is the primary key — NOT an identity column.</summary>
    [Key]
    [Column("CurrencyCode")]
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = string.Empty;

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
