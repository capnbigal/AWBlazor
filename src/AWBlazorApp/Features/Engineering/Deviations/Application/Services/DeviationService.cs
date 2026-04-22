using AWBlazorApp.Features.Engineering.Audit;
using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Engineering.Deviations.Application.Services;

public sealed class DeviationService(IDbContextFactory<ApplicationDbContext> dbFactory) : IDeviationService
{

    public Task ApproveAsync(int deviationId, string? decisionNotes, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(deviationId, DeviationStatus.Approved, userId, decisionNotes, cancellationToken);

    public Task RejectAsync(int deviationId, string? decisionNotes, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(deviationId, DeviationStatus.Rejected, userId, decisionNotes, cancellationToken);

    public Task CancelAsync(int deviationId, string? userId, CancellationToken cancellationToken)
        => TransitionAsync(deviationId, DeviationStatus.Cancelled, userId, decisionNotes: null, cancellationToken);

    private async Task TransitionAsync(
        int deviationId, DeviationStatus target, string? userId, string? decisionNotes, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var d = await db.DeviationRequests.FirstOrDefaultAsync(x => x.Id == deviationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deviation {deviationId} not found.");

        if (d.Status != DeviationStatus.Pending)
            throw new InvalidOperationException($"Cannot transition deviation from {d.Status} to {target}.");

        var before = DeviationRequestAuditService.CaptureSnapshot(d);
        var now = DateTime.UtcNow;

        d.Status = target;
        d.DecidedAt = now;
        d.DecidedByUserId = userId;
        d.DecisionNotes = decisionNotes?.Trim();
        d.ModifiedDate = now;

        db.DeviationRequestAuditLogs.Add(DeviationRequestAuditService.RecordUpdate(before, d, userId));

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
