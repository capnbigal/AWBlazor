using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="SalesPersonQuotaHistory"/>. EF-managed table <c>dbo.SalesPersonQuotaHistoryAuditLogs</c>. Stores both composite key components as separate columns.</summary>
public class SalesPersonQuotaHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public DateTime QuotaDate { get; set; }

    public decimal SalesQuota { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
