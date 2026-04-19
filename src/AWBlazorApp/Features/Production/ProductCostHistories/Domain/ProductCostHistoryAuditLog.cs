using AWBlazorApp.Shared.Domain;
namespace AWBlazorApp.Features.Production.ProductCostHistories.Domain;

/// <summary>Audit log for <see cref="ProductCostHistory"/>. EF-managed table <c>dbo.ProductCostHistoryAuditLogs</c>.</summary>
public class ProductCostHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    public decimal StandardCost { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
