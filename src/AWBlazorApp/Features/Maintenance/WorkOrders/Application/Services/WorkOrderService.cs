using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Maintenance.WorkOrders.Application.Services;

public sealed class WorkOrderService(IDbContextFactory<ApplicationDbContext> dbFactory) : IWorkOrderService
{

    public Task ScheduleAsync(int workOrderId, DateTime scheduledFor, int? assigneeBusinessEntityId, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(workOrderId, WorkOrderStatus.Scheduled, userId, wo =>
        {
            wo.ScheduledFor = scheduledFor;
            wo.AssignedBusinessEntityId = assigneeBusinessEntityId;
        }, cancellationToken);

    public Task StartAsync(int workOrderId, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(workOrderId, WorkOrderStatus.InProgress, userId, wo =>
        {
            wo.StartedAt ??= DateTime.UtcNow;
        }, cancellationToken);

    public Task HoldAsync(int workOrderId, string? reason, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(workOrderId, WorkOrderStatus.OnHold, userId, wo =>
        {
            wo.HeldAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(reason)) wo.CompletionNotes = reason.Trim();
        }, cancellationToken);

    public Task ResumeAsync(int workOrderId, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(workOrderId, WorkOrderStatus.InProgress, userId, wo =>
        {
            wo.HeldAt = null;
        }, cancellationToken);

    public async Task CompleteAsync(int workOrderId, string? completionNotes, decimal? completedMeterValue, string? userId, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var wo = await db.MaintenanceWorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order {workOrderId} not found.");

        Guard(wo.Status, WorkOrderStatus.Completed);

        var before = MaintenanceWorkOrderAuditService.CaptureSnapshot(wo);
        var now = DateTime.UtcNow;

        wo.Status = WorkOrderStatus.Completed;
        wo.CompletedAt = now;
        wo.StartedAt ??= now;
        wo.CompletionNotes = completionNotes?.Trim();
        wo.CompletedMeterValue = completedMeterValue;
        wo.ModifiedDate = now;

        db.MaintenanceWorkOrderAuditLogs.Add(
            MaintenanceWorkOrderAuditService.RecordUpdate(before, wo, userId));

        // If this WO came from a PM schedule, update the schedule's cached last-completed stamps.
        if (wo.PmScheduleId.HasValue)
        {
            var schedule = await db.PmSchedules.FirstOrDefaultAsync(s => s.Id == wo.PmScheduleId.Value, cancellationToken);
            if (schedule is not null)
            {
                var scheduleBefore = PmScheduleAuditService.CaptureSnapshot(schedule);
                schedule.LastCompletedAt = now;
                if (completedMeterValue.HasValue) schedule.LastCompletedMeterValue = completedMeterValue;
                schedule.ModifiedDate = now;
                db.PmScheduleAuditLogs.Add(PmScheduleAuditService.RecordUpdate(scheduleBefore, schedule, userId));
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public Task CancelAsync(int workOrderId, string? reason, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(workOrderId, WorkOrderStatus.Cancelled, userId, wo =>
        {
            wo.CancelledAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(reason)) wo.CompletionNotes = reason.Trim();
        }, cancellationToken);

    private async Task TransitionAsync(
        int workOrderId, WorkOrderStatus target, string? userId,
        Action<MaintenanceWorkOrder> apply, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var wo = await db.MaintenanceWorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order {workOrderId} not found.");

        Guard(wo.Status, target);

        var before = MaintenanceWorkOrderAuditService.CaptureSnapshot(wo);
        wo.Status = target;
        wo.ModifiedDate = DateTime.UtcNow;
        apply(wo);

        db.MaintenanceWorkOrderAuditLogs.Add(
            MaintenanceWorkOrderAuditService.RecordUpdate(before, wo, userId));

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private static void Guard(WorkOrderStatus from, WorkOrderStatus to)
    {
        var allowed = to switch
        {
            WorkOrderStatus.Scheduled => from == WorkOrderStatus.Draft,
            WorkOrderStatus.InProgress => from is WorkOrderStatus.Scheduled or WorkOrderStatus.OnHold,
            WorkOrderStatus.OnHold => from == WorkOrderStatus.InProgress,
            WorkOrderStatus.Completed => from == WorkOrderStatus.InProgress,
            WorkOrderStatus.Cancelled => from is WorkOrderStatus.Draft or WorkOrderStatus.Scheduled or WorkOrderStatus.InProgress or WorkOrderStatus.OnHold,
            _ => false,
        };
        if (!allowed)
            throw new InvalidOperationException($"Cannot transition work order from {from} to {to}.");
    }
}
