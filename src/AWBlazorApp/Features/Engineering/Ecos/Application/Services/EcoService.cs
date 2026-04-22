using AWBlazorApp.Features.Engineering.Audit;
using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Engineering.Ecos.Application.Services;

public sealed class EcoService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<EcoService> logger) : IEcoService
{

    public Task SubmitForReviewAsync(int ecoId, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(ecoId, EcoStatus.UnderReview, userId, decisionNotes: null, cancellationToken);

    public Task ApproveAsync(int ecoId, string? decisionNotes, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(ecoId, EcoStatus.Approved, userId, decisionNotes, cancellationToken);

    public Task RejectAsync(int ecoId, string? decisionNotes, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(ecoId, EcoStatus.Rejected, userId, decisionNotes, cancellationToken);

    public Task CancelAsync(int ecoId, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(ecoId, EcoStatus.Cancelled, userId, decisionNotes: null, cancellationToken);

    private async Task TransitionAsync(
        int ecoId, EcoStatus target, string? userId, string? decisionNotes, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var eco = await db.EngineeringChangeOrders.FirstOrDefaultAsync(e => e.Id == ecoId, cancellationToken)
            ?? throw new KeyNotFoundException($"ECO {ecoId} not found.");

        Guard(eco.Status, target);

        var before = EngineeringChangeOrderAuditService.CaptureSnapshot(eco);
        var now = DateTime.UtcNow;

        eco.Status = target;
        eco.ModifiedDate = now;

        switch (target)
        {
            case EcoStatus.UnderReview:
                eco.SubmittedAt ??= now;
                break;

            case EcoStatus.Approved:
                eco.DecidedAt = now;
                eco.DecidedByUserId = userId;
                eco.DecisionNotes = decisionNotes?.Trim();
                db.EcoApprovals.Add(new EcoApproval
                {
                    EngineeringChangeOrderId = eco.Id,
                    ApproverUserId = userId,
                    Decision = EcoApprovalDecision.Approved,
                    DecidedAt = now,
                    Notes = decisionNotes?.Trim(),
                    ModifiedDate = now,
                });
                await ActivateAffectedAsync(db, eco.Id, userId, cancellationToken);
                break;

            case EcoStatus.Rejected:
                eco.DecidedAt = now;
                eco.DecidedByUserId = userId;
                eco.DecisionNotes = decisionNotes?.Trim();
                db.EcoApprovals.Add(new EcoApproval
                {
                    EngineeringChangeOrderId = eco.Id,
                    ApproverUserId = userId,
                    Decision = EcoApprovalDecision.Rejected,
                    DecidedAt = now,
                    Notes = decisionNotes?.Trim(),
                    ModifiedDate = now,
                });
                break;
        }

        db.EngineeringChangeOrderAuditLogs.Add(
            EngineeringChangeOrderAuditService.RecordUpdate(before, eco, userId));

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private static void Guard(EcoStatus from, EcoStatus to)
    {
        var allowed = to switch
        {
            EcoStatus.UnderReview => from == EcoStatus.Draft,
            EcoStatus.Approved or EcoStatus.Rejected => from == EcoStatus.UnderReview,
            EcoStatus.Cancelled => from is EcoStatus.Draft or EcoStatus.UnderReview,
            _ => false,
        };
        if (!allowed)
            throw new InvalidOperationException($"Cannot transition ECO from {from} to {to}.");
    }

    /// <summary>
    /// For every BOM / Routing affected item, activates the referenced row and deactivates
    /// any other currently-active row for the same ProductId. Other affected-kinds (Product,
    /// Document) are informational — no state change.
    /// </summary>
    private async Task ActivateAffectedAsync(
        ApplicationDbContext db, int ecoId, string? userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var affected = await db.EcoAffectedItems.AsNoTracking()
            .Where(a => a.EngineeringChangeOrderId == ecoId)
            .ToListAsync(cancellationToken);

        foreach (var item in affected)
        {
            switch (item.AffectedKind)
            {
                case EcoAffectedKind.Bom:
                    await ActivateBomAsync(db, item.TargetId, userId, now, cancellationToken);
                    break;
                case EcoAffectedKind.Routing:
                    await ActivateRoutingAsync(db, item.TargetId, userId, now, cancellationToken);
                    break;
                case EcoAffectedKind.Product:
                case EcoAffectedKind.Document:
                    // No auto-activation for these.
                    break;
            }
        }
    }

    private async Task ActivateBomAsync(
        ApplicationDbContext db, int bomHeaderId, string? userId, DateTime now, CancellationToken cancellationToken)
    {
        var target = await db.BomHeaders.FirstOrDefaultAsync(b => b.Id == bomHeaderId, cancellationToken);
        if (target is null)
        {
            logger.LogWarning("ECO activation skipped: BomHeader {Id} not found.", bomHeaderId);
            return;
        }

        var priors = await db.BomHeaders
            .Where(b => b.ProductId == target.ProductId && b.IsActive && b.Id != target.Id)
            .ToListAsync(cancellationToken);

        foreach (var prior in priors)
        {
            var before = BomHeaderAuditService.CaptureSnapshot(prior);
            prior.IsActive = false;
            prior.ModifiedDate = now;
            db.BomHeaderAuditLogs.Add(BomHeaderAuditService.RecordUpdate(before, prior, userId));
        }

        if (!target.IsActive)
        {
            var before = BomHeaderAuditService.CaptureSnapshot(target);
            target.IsActive = true;
            target.ModifiedDate = now;
            db.BomHeaderAuditLogs.Add(BomHeaderAuditService.RecordUpdate(before, target, userId));
        }
    }

    private async Task ActivateRoutingAsync(
        ApplicationDbContext db, int routingId, string? userId, DateTime now, CancellationToken cancellationToken)
    {
        var target = await db.ManufacturingRoutings.FirstOrDefaultAsync(r => r.Id == routingId, cancellationToken);
        if (target is null)
        {
            logger.LogWarning("ECO activation skipped: ManufacturingRouting {Id} not found.", routingId);
            return;
        }

        var priors = await db.ManufacturingRoutings
            .Where(r => r.ProductId == target.ProductId && r.IsActive && r.Id != target.Id)
            .ToListAsync(cancellationToken);

        foreach (var prior in priors)
        {
            var before = ManufacturingRoutingAuditService.CaptureSnapshot(prior);
            prior.IsActive = false;
            prior.ModifiedDate = now;
            db.ManufacturingRoutingAuditLogs.Add(ManufacturingRoutingAuditService.RecordUpdate(before, prior, userId));
        }

        if (!target.IsActive)
        {
            var before = ManufacturingRoutingAuditService.CaptureSnapshot(target);
            target.IsActive = true;
            target.ModifiedDate = now;
            db.ManufacturingRoutingAuditLogs.Add(ManufacturingRoutingAuditService.RecordUpdate(before, target, userId));
        }
    }
}
