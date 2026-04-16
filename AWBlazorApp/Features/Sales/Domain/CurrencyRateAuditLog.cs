using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="CurrencyRate"/>. EF-managed table <c>dbo.CurrencyRateAuditLogs</c>.</summary>
public class CurrencyRateAuditLog : AdventureWorksAuditLogBase
{
    public int CurrencyRateId { get; set; }

    public DateTime CurrencyRateDate { get; set; }
    [MaxLength(3)] public string? FromCurrencyCode { get; set; }
    [MaxLength(3)] public string? ToCurrencyCode { get; set; }
    public decimal AverageRate { get; set; }
    public decimal EndOfDayRate { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
