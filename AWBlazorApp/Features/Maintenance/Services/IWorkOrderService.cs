namespace AWBlazorApp.Features.Maintenance.Services;

/// <summary>
/// Maintenance work order state machine:
///   Draft → Scheduled → InProgress → Completed
/// with OnHold (resume back to InProgress) and Cancelled side paths from any non-terminal state.
/// On Complete, the caller can pass a meter reading which is cached on the WO and used by
/// <see cref="IPmScheduleService"/> to update the triggering schedule's last-completed stamp.
/// </summary>
public interface IWorkOrderService
{
    Task ScheduleAsync(int workOrderId, DateTime scheduledFor, int? assigneeBusinessEntityId, string? userId, CancellationToken cancellationToken);
    Task StartAsync(int workOrderId, string? userId, CancellationToken cancellationToken);
    Task HoldAsync(int workOrderId, string? reason, string? userId, CancellationToken cancellationToken);
    Task ResumeAsync(int workOrderId, string? userId, CancellationToken cancellationToken);
    Task CompleteAsync(int workOrderId, string? completionNotes, decimal? completedMeterValue, string? userId, CancellationToken cancellationToken);
    Task CancelAsync(int workOrderId, string? reason, string? userId, CancellationToken cancellationToken);
}
