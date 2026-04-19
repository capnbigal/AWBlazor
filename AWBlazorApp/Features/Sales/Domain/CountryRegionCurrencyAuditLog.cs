using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="CountryRegionCurrency"/>. EF-managed table <c>dbo.CountryRegionCurrencyAuditLogs</c>.</summary>
public class CountryRegionCurrencyAuditLog : AdventureWorksAuditLogBase
{
    [MaxLength(3)] public string CountryRegionCode { get; set; } = string.Empty;
    [MaxLength(3)] public string CurrencyCode { get; set; } = string.Empty;

    public DateTime SourceModifiedDate { get; set; }
}
