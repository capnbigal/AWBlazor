namespace AWBlazorApp.Features.Maintenance.Services;

/// <summary>
/// PM schedule orchestration. The main load-bearing method is
/// <see cref="GenerateDueWorkOrdersAsync"/> — walks active schedules, and for each one whose
/// interval has lapsed (days since last completion for time-based, or meter delta for
/// meter-based), creates a Draft work order, copies over the schedule's tasks, and links it
/// back via <c>MaintenanceWorkOrder.PmScheduleId</c>. Idempotent: running it twice without a
/// fresh completion in between yields zero new WOs the second time, because a Draft WO
/// already exists for that schedule.
/// </summary>
public interface IPmScheduleService
{
    /// <summary>
    /// Generates Draft work orders for any active PM schedules that are due. Returns the count
    /// of newly generated WOs. Optionally scoped to a single schedule via <paramref name="pmScheduleId"/>.
    /// </summary>
    Task<int> GenerateDueWorkOrdersAsync(int? pmScheduleId, string? userId, CancellationToken cancellationToken);
}
