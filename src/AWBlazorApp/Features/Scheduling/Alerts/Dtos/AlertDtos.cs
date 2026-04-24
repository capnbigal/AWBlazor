using AWBlazorApp.Features.Scheduling.Alerts.Domain;

namespace AWBlazorApp.Features.Scheduling.Alerts.Dtos;

public sealed record SchedulingAlertDto(
    int Id, DateTime CreatedAt, byte Severity, byte EventType,
    int WeekId, short LocationId, int? SalesOrderId,
    string Message, string? PayloadJson,
    DateTime? AcknowledgedAt, string? AcknowledgedBy);

public static class AlertMappings
{
    public static SchedulingAlertDto ToDto(this SchedulingAlert e) =>
        new(e.Id, e.CreatedAt, (byte)e.Severity, (byte)e.EventType,
            e.WeekId, e.LocationId, e.SalesOrderId,
            e.Message, e.PayloadJson,
            e.AcknowledgedAt, e.AcknowledgedBy);
}
