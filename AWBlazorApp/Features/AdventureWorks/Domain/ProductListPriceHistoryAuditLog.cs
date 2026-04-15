namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="ProductListPriceHistory"/>. EF-managed table <c>dbo.ProductListPriceHistoryAuditLogs</c>. Carries both composite-key components.</summary>
public class ProductListPriceHistoryAuditLog : AdventureWorksAuditLogBase
{
    public int ProductId { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    public decimal ListPrice { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
