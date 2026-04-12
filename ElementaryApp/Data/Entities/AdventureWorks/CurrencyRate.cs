using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Daily exchange-rate snapshot. Maps onto the pre-existing <c>Sales.CurrencyRate</c> table. The natural key is (CurrencyRateDate, FromCurrencyCode, ToCurrencyCode) but the PK is a surrogate <c>CurrencyRateID</c> identity column.</summary>
[Table("CurrencyRate", Schema = "Sales")]
public class CurrencyRate
{
    [Key]
    [Column("CurrencyRateID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>The day this rate applies to. Truncated to midnight in practice.</summary>
    [Column("CurrencyRateDate")]
    public DateTime CurrencyRateDate { get; set; }

    /// <summary>Source currency. FK to <c>Sales.Currency.CurrencyCode</c>.</summary>
    [Column("FromCurrencyCode")]
    [MaxLength(3)]
    public string FromCurrencyCode { get; set; } = string.Empty;

    /// <summary>Target currency. FK to <c>Sales.Currency.CurrencyCode</c>.</summary>
    [Column("ToCurrencyCode")]
    [MaxLength(3)]
    public string ToCurrencyCode { get; set; } = string.Empty;

    [Column("AverageRate", TypeName = "money")]
    public decimal AverageRate { get; set; }

    [Column("EndOfDayRate", TypeName = "money")]
    public decimal EndOfDayRate { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
