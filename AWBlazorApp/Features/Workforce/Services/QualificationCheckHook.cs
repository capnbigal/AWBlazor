using AWBlazorApp.Features.Workforce.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.Services;

/// <summary>
/// Listens for operator clock-ins via <see cref="IPostingTriggerHook"/>. For each required
/// <see cref="StationQualification"/> at the station, checks whether the operator holds an
/// <see cref="EmployeeQualification"/> for that qual and whether it's still current. Missing
/// or expired quals raise a <see cref="QualificationAlert"/> for the manager's inbox. Per the
/// user's preference: clock-ins are never blocked — alerts are best-effort observations.
/// </summary>
public sealed class QualificationCheckHook(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<QualificationCheckHook> logger) : IPostingTriggerHook
{
    public Task AfterGoodsReceiptPostedAsync(GoodsReceiptLinePostedContext context, CancellationToken ct) => Task.CompletedTask;
    public Task AfterShipmentPostedAsync(ShipmentLinePostedContext context, CancellationToken ct) => Task.CompletedTask;
    public Task AfterProductionRunCompletedAsync(ProductionRunCompletedContext context, CancellationToken ct) => Task.CompletedTask;

    public async Task AfterOperatorClockedInAsync(OperatorClockedInContext ctx, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var requiredQualIds = await db.StationQualifications.AsNoTracking()
            .Where(sq => sq.StationId == ctx.StationId && sq.IsRequired)
            .Select(sq => sq.QualificationId)
            .ToListAsync(ct);

        if (requiredQualIds.Count == 0) return;

        var heldQuals = await db.EmployeeQualifications.AsNoTracking()
            .Where(eq => eq.BusinessEntityId == ctx.BusinessEntityId && requiredQualIds.Contains(eq.QualificationId))
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var alertsToInsert = new List<QualificationAlert>();
        foreach (var qualId in requiredQualIds)
        {
            var held = heldQuals.FirstOrDefault(eq => eq.QualificationId == qualId);
            QualificationAlertReason? reason = null;
            if (held is null) reason = QualificationAlertReason.Missing;
            else if (held.ExpiresOn is { } exp && exp < now) reason = QualificationAlertReason.Expired;
            if (reason is not { } r) continue;

            // De-dupe: if there's already an Open alert for this (employee, station, qual),
            // don't pile on. Acknowledged / Resolved / Dismissed alerts don't suppress new ones.
            var existingOpen = await db.QualificationAlerts.AsNoTracking()
                .AnyAsync(a => a.BusinessEntityId == ctx.BusinessEntityId
                            && a.StationId == ctx.StationId
                            && a.QualificationId == qualId
                            && a.Status == QualificationAlertStatus.Open, ct);
            if (existingOpen) continue;

            alertsToInsert.Add(new QualificationAlert
            {
                BusinessEntityId = ctx.BusinessEntityId,
                StationId = ctx.StationId,
                QualificationId = qualId,
                OperatorClockEventId = ctx.OperatorClockEventId,
                Reason = r,
                Status = QualificationAlertStatus.Open,
                RaisedAt = now,
                ModifiedDate = now,
            });
        }

        if (alertsToInsert.Count == 0) return;
        db.QualificationAlerts.AddRange(alertsToInsert);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Raised {Count} qualification alert(s) for employee {E} clocking in to station {S}",
            alertsToInsert.Count, ctx.BusinessEntityId, ctx.StationId);
    }
}
