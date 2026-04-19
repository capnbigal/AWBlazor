using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.Sales.SalesPeople.Domain;

/// <summary>Audit log for <see cref="SalesPerson"/>. EF-managed table <c>dbo.SalesPersonAuditLogs</c>.</summary>
public class SalesPersonAuditLog : AdventureWorksAuditLogBase
{
    public int SalesPersonId { get; set; }

    public int? TerritoryId { get; set; }
    public decimal? SalesQuota { get; set; }
    public decimal Bonus { get; set; }
    public decimal CommissionPct { get; set; }
    public decimal SalesYtd { get; set; }
    public decimal SalesLastYear { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
