using AWBlazorApp.Features.AdventureWorks.Domain;
namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="SalesTerritoryHistory"/>. EF-managed table <c>dbo.SalesTerritoryHistoryAuditLogs</c>.</summary>
public class SalesTerritoryHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int BusinessEntityId { get; set; }
    public int TerritoryId { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
