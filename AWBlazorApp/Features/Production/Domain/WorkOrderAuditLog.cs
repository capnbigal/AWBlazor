using AWBlazorApp.Features.AdventureWorks.Domain;
namespace AWBlazorApp.Features.Production.Domain;

/// <summary>Audit log for <see cref="WorkOrder"/>. EF-managed table <c>dbo.WorkOrderAuditLogs</c>.</summary>
public class WorkOrderAuditLog : AdventureWorksAuditLogBase
{
    public int WorkOrderId { get; set; }

    public int ProductId { get; set; }
    public int OrderQty { get; set; }
    public int StockedQty { get; set; }
    public short ScrappedQty { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime DueDate { get; set; }
    public short? ScrapReasonId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
