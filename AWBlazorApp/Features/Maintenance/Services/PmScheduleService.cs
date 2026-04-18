using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Maintenance.Services;

public sealed class PmScheduleService : IPmScheduleService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<PmScheduleService> _logger;

    public PmScheduleService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<PmScheduleService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<int> GenerateDueWorkOrdersAsync(int? pmScheduleId, string? userId, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var q = db.PmSchedules.Where(s => s.IsActive);
        if (pmScheduleId.HasValue) q = q.Where(s => s.Id == pmScheduleId.Value);

        var schedules = await q.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var generated = 0;

        foreach (var schedule in schedules)
        {
            // Skip if there's already an open (non-terminal) WO for this schedule —
            // regenerating would churn duplicates. Completed WOs don't block regeneration
            // because that's how the next cycle gets created.
            var hasOpen = await db.MaintenanceWorkOrders.AnyAsync(
                w => w.PmScheduleId == schedule.Id
                  && w.Status != WorkOrderStatus.Completed
                  && w.Status != WorkOrderStatus.Cancelled,
                cancellationToken);
            if (hasOpen) continue;

            if (!await IsDueAsync(db, schedule, now, cancellationToken)) continue;

            var wo = new MaintenanceWorkOrder
            {
                WorkOrderNumber = $"PM-{schedule.Code}-{now:yyyyMMddHHmmss}",
                Title = $"PM: {schedule.Name}",
                Description = schedule.Description,
                AssetId = schedule.AssetId,
                Type = WorkOrderType.Preventive,
                Status = WorkOrderStatus.Draft,
                Priority = schedule.DefaultPriority,
                PmScheduleId = schedule.Id,
                ScheduledFor = now,
                RaisedByUserId = userId,
                RaisedAt = now,
                ModifiedDate = now,
            };
            db.MaintenanceWorkOrders.Add(wo);
            await db.SaveChangesAsync(cancellationToken);

            // Copy the schedule's tasks onto the new WO.
            var tasks = await db.PmScheduleTasks.AsNoTracking()
                .Where(t => t.PmScheduleId == schedule.Id)
                .OrderBy(t => t.SequenceNumber)
                .ToListAsync(cancellationToken);

            foreach (var t in tasks)
            {
                db.MaintenanceWorkOrderTasks.Add(new MaintenanceWorkOrderTask
                {
                    MaintenanceWorkOrderId = wo.Id,
                    SequenceNumber = t.SequenceNumber,
                    TaskName = t.TaskName,
                    Instructions = t.Instructions,
                    EstimatedMinutes = t.EstimatedMinutes,
                    RequiresSignoff = t.RequiresSignoff,
                    ModifiedDate = now,
                });
            }

            db.MaintenanceWorkOrderAuditLogs.Add(
                MaintenanceWorkOrderAuditService.RecordCreate(wo, userId));

            // Update the AssetMaintenanceProfile's NextPmDueAt hint so the asset page shows it.
            var profile = await db.AssetMaintenanceProfiles.FirstOrDefaultAsync(p => p.AssetId == schedule.AssetId, cancellationToken);
            if (profile is not null && (profile.NextPmDueAt is null || profile.NextPmDueAt > now))
            {
                var profileBefore = AssetMaintenanceProfileAuditService.CaptureSnapshot(profile);
                profile.NextPmDueAt = now;
                profile.ModifiedDate = now;
                db.AssetMaintenanceProfileAuditLogs.Add(AssetMaintenanceProfileAuditService.RecordUpdate(profileBefore, profile, userId));
            }

            await db.SaveChangesAsync(cancellationToken);
            generated++;
        }

        await tx.CommitAsync(cancellationToken);

        if (generated > 0)
            _logger.LogInformation("Generated {Count} PM work order(s) from {Total} active schedules.", generated, schedules.Count);

        return generated;
    }

    private static async Task<bool> IsDueAsync(ApplicationDbContext db, PmSchedule schedule, DateTime now, CancellationToken cancellationToken)
    {
        switch (schedule.IntervalKind)
        {
            case PmIntervalKind.Days:
            {
                if (schedule.LastCompletedAt is null) return true; // never run — due
                var daysSince = (now - schedule.LastCompletedAt.Value).TotalDays;
                return daysSince >= schedule.IntervalValue;
            }
            case PmIntervalKind.RuntimeHours:
            case PmIntervalKind.Cycles:
            {
                var meterKind = schedule.IntervalKind == PmIntervalKind.RuntimeHours
                    ? MeterKind.RuntimeHours
                    : MeterKind.Cycles;
                var latest = await db.MeterReadings.AsNoTracking()
                    .Where(m => m.AssetId == schedule.AssetId && m.Kind == meterKind)
                    .OrderByDescending(m => m.RecordedAt)
                    .Select(m => (decimal?)m.Value)
                    .FirstOrDefaultAsync(cancellationToken);
                if (latest is null) return false; // can't decide without a reading
                if (schedule.LastCompletedMeterValue is null) return true; // never completed on meter — due now
                var delta = latest.Value - schedule.LastCompletedMeterValue.Value;
                return delta >= schedule.IntervalValue;
            }
            default:
                return false;
        }
    }
}
