namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="WorkOrderRouting"/>. EF-managed table <c>dbo.WorkOrderRoutingAuditLogs</c>.</summary>
public class WorkOrderRoutingAuditLog : AdventureWorksAuditLogBase
{
    public int WorkOrderId { get; set; }
    public int ProductId { get; set; }
    public short OperationSequence { get; set; }

    public short LocationId { get; set; }
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal? ActualResourceHrs { get; set; }
    public decimal PlannedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
