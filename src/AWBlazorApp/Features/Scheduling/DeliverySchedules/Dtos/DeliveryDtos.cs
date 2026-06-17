using AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;

namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Dtos;

public sealed record CurrentDeliveryRowDto(
    int WeekId, short LocationId, int SalesOrderId, int SalesOrderDetailId, int ProductId,
    int? PlannedSequence, DateTime? PlannedStart, DateTime? PlannedEnd, short? PlannedQty,
    int? CurrentSequence, DateTime? CurrentStart, DateTime? CurrentEnd, short? CurrentQty,
    int? SequenceDrift, int? StartDriftMinutes,
    DateTime? PromiseDate, int? PromiseDriftMinutes,
    byte? ExceptionType, string? ExceptionReason,
    byte? SoStatus, bool IsCancelled, bool IsHotOrder);

public sealed record SchedulingExceptionDto(
    int Id, int WeekId, short LocationId, int SalesOrderDetailId,
    byte ExceptionType, int? PinnedSequence, string Reason,
    DateTime CreatedAt, string CreatedBy,
    DateTime? ResolvedAt, string? ResolvedBy);

public sealed record CreateSchedulingExceptionRequest
{
    public int WeekId { get; set; }
    public short LocationId { get; set; }
    public int SalesOrderDetailId { get; set; }
    public byte ExceptionType { get; set; } // 1=ManualSequencePin, 2=KittingHold, 3=HotOrderBump
    public int? PinnedSequence { get; set; }
    public string Reason { get; set; } = "";
}

public static class DeliveryMappings
{
    public static CurrentDeliveryRowDto ToDto(this CurrentDeliveryScheduleRow e) =>
        new(e.WeekId, e.LocationId, e.SalesOrderId, e.SalesOrderDetailId, e.ProductId,
            e.PlannedSequence, e.PlannedStart, e.PlannedEnd, e.PlannedQty,
            e.CurrentSequence, e.CurrentStart, e.CurrentEnd, e.CurrentQty,
            e.SequenceDrift, e.StartDriftMinutes,
            e.PromiseDate, e.PromiseDriftMinutes,
            e.ExceptionType, e.ExceptionReason,
            e.SoStatus, e.IsCancelled, e.IsHotOrder);

    public static SchedulingExceptionDto ToDto(this SchedulingException e) =>
        new(e.Id, e.WeekId, e.LocationId, e.SalesOrderDetailId,
            (byte)e.ExceptionType, e.PinnedSequence, e.Reason,
            e.CreatedAt, e.CreatedBy, e.ResolvedAt, e.ResolvedBy);

    public static SchedulingException ToEntity(this CreateSchedulingExceptionRequest r, string createdBy) => new()
    {
        WeekId = r.WeekId,
        LocationId = r.LocationId,
        SalesOrderDetailId = r.SalesOrderDetailId,
        ExceptionType = (ExceptionType)r.ExceptionType,
        PinnedSequence = r.PinnedSequence,
        Reason = r.Reason,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = createdBy,
    };
}
