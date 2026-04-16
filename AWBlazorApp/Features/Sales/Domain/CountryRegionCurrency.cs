using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Junction linking countries to the currencies used there. Maps onto the pre-existing <c>Sales.CountryRegionCurrency</c> table. Composite string PK = (CountryRegionCode, CurrencyCode).</summary>
[PrimaryKey(nameof(CountryRegionCode), nameof(CurrencyCode))]
[Table("CountryRegionCurrency", Schema = "Sales")]
public class CountryRegionCurrency
{
    /// <summary>FK to <c>Person.CountryRegion.CountryRegionCode</c>. Part of the composite PK.</summary>
    [Column("CountryRegionCode")]
    [MaxLength(3)]
    public string CountryRegionCode { get; set; } = string.Empty;

    /// <summary>FK to <c>Sales.Currency.CurrencyCode</c>. Part of the composite PK.</summary>
    [Column("CurrencyCode")]
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
