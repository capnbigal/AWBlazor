using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Models;

public sealed record WorkOrderRoutingDto(
    int WorkOrderId, int ProductId, short OperationSequence,
    short LocationId, DateTime ScheduledStartDate, DateTime ScheduledEndDate,
    DateTime? ActualStartDate, DateTime? ActualEndDate, decimal? ActualResourceHrs,
    decimal PlannedCost, decimal? ActualCost, DateTime ModifiedDate);

public sealed record CreateWorkOrderRoutingRequest
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
}

public sealed record UpdateWorkOrderRoutingRequest
{
    public short? LocationId { get; set; }
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal? ActualResourceHrs { get; set; }
    public decimal? PlannedCost { get; set; }
    public decimal? ActualCost { get; set; }
}

public sealed record WorkOrderRoutingAuditLogDto(
    int Id, int WorkOrderId, int ProductId, short OperationSequence,
    string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    short LocationId, DateTime ScheduledStartDate, DateTime ScheduledEndDate,
    DateTime? ActualStartDate, DateTime? ActualEndDate, decimal? ActualResourceHrs,
    decimal PlannedCost, decimal? ActualCost, DateTime SourceModifiedDate);

public static class WorkOrderRoutingMappings
{
    public static WorkOrderRoutingDto ToDto(this WorkOrderRouting e) => new(
        e.WorkOrderId, e.ProductId, e.OperationSequence,
        e.LocationId, e.ScheduledStartDate, e.ScheduledEndDate,
        e.ActualStartDate, e.ActualEndDate, e.ActualResourceHrs,
        e.PlannedCost, e.ActualCost, e.ModifiedDate);

    public static WorkOrderRouting ToEntity(this CreateWorkOrderRoutingRequest r) => new()
    {
        WorkOrderId = r.WorkOrderId,
        ProductId = r.ProductId,
        OperationSequence = r.OperationSequence,
        LocationId = r.LocationId,
        ScheduledStartDate = r.ScheduledStartDate,
        ScheduledEndDate = r.ScheduledEndDate,
        ActualStartDate = r.ActualStartDate,
        ActualEndDate = r.ActualEndDate,
        ActualResourceHrs = r.ActualResourceHrs,
        PlannedCost = r.PlannedCost,
        ActualCost = r.ActualCost,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateWorkOrderRoutingRequest r, WorkOrderRouting e)
    {
        if (r.LocationId.HasValue) e.LocationId = r.LocationId.Value;
        if (r.ScheduledStartDate.HasValue) e.ScheduledStartDate = r.ScheduledStartDate.Value;
        if (r.ScheduledEndDate.HasValue) e.ScheduledEndDate = r.ScheduledEndDate.Value;
        if (r.ActualStartDate.HasValue) e.ActualStartDate = r.ActualStartDate.Value;
        if (r.ActualEndDate.HasValue) e.ActualEndDate = r.ActualEndDate.Value;
        if (r.ActualResourceHrs.HasValue) e.ActualResourceHrs = r.ActualResourceHrs.Value;
        if (r.PlannedCost.HasValue) e.PlannedCost = r.PlannedCost.Value;
        if (r.ActualCost.HasValue) e.ActualCost = r.ActualCost.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static WorkOrderRoutingAuditLogDto ToDto(this WorkOrderRoutingAuditLog a) => new(
        a.Id, a.WorkOrderId, a.ProductId, a.OperationSequence,
        a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.LocationId, a.ScheduledStartDate, a.ScheduledEndDate,
        a.ActualStartDate, a.ActualEndDate, a.ActualResourceHrs,
        a.PlannedCost, a.ActualCost, a.SourceModifiedDate);
}
