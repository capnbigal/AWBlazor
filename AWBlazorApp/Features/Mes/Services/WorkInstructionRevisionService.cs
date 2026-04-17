using AWBlazorApp.Features.Mes.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Mes.Services;

/// <inheritdoc />
public sealed class WorkInstructionRevisionService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<WorkInstructionRevisionService> logger) : IWorkInstructionRevisionService
{
    public async Task<int> CreateNewRevisionAsync(int workInstructionId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var instruction = await db.WorkInstructions.FirstOrDefaultAsync(w => w.Id == workInstructionId, ct)
            ?? throw new InvalidOperationException($"WorkInstruction {workInstructionId} not found.");

        var nextRevisionNumber = (await db.WorkInstructionRevisions.AsNoTracking()
            .Where(r => r.WorkInstructionId == workInstructionId)
            .Select(r => (int?)r.RevisionNumber).MaxAsync(ct) ?? 0) + 1;

        var now = DateTime.UtcNow;
        var revision = new WorkInstructionRevision
        {
            WorkInstructionId = workInstructionId,
            RevisionNumber = nextRevisionNumber,
            Status = WorkInstructionRevisionStatus.Draft,
            CreatedByUserId = userId,
            CreatedDate = now,
            ModifiedDate = now,
        };
        db.WorkInstructionRevisions.Add(revision);
        await db.SaveChangesAsync(ct);

        // Copy steps from the previous active revision (or the most recent one if no active).
        var sourceRevisionId = instruction.ActiveRevisionId
            ?? await db.WorkInstructionRevisions.AsNoTracking()
                .Where(r => r.WorkInstructionId == workInstructionId && r.Id != revision.Id)
                .OrderByDescending(r => r.RevisionNumber)
                .Select(r => (int?)r.Id).FirstOrDefaultAsync(ct);

        if (sourceRevisionId.HasValue)
        {
            var sourceSteps = await db.WorkInstructionSteps.AsNoTracking()
                .Where(s => s.WorkInstructionRevisionId == sourceRevisionId.Value)
                .OrderBy(s => s.SequenceNumber)
                .ToListAsync(ct);

            foreach (var s in sourceSteps)
            {
                db.WorkInstructionSteps.Add(new WorkInstructionStep
                {
                    WorkInstructionRevisionId = revision.Id,
                    SequenceNumber = s.SequenceNumber,
                    Title = s.Title,
                    Body = s.Body,
                    AttachmentUrl = s.AttachmentUrl,
                    EstimatedDurationMinutes = s.EstimatedDurationMinutes,
                    ModifiedDate = now,
                });
            }
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation("Created revision {Number} on WorkInstruction {Id} (copied {Count} steps)",
            nextRevisionNumber, workInstructionId,
            sourceRevisionId.HasValue ? await db.WorkInstructionSteps.CountAsync(s => s.WorkInstructionRevisionId == revision.Id, ct) : 0);
        return revision.Id;
    }

    public async Task PublishAsync(int revisionId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var revision = await db.WorkInstructionRevisions.FirstOrDefaultAsync(r => r.Id == revisionId, ct)
            ?? throw new InvalidOperationException($"WorkInstructionRevision {revisionId} not found.");
        if (revision.Status != WorkInstructionRevisionStatus.Draft)
            throw new InvalidOperationException($"Revision is {revision.Status}; only Draft revisions can be published.");

        var instruction = await db.WorkInstructions.FirstAsync(w => w.Id == revision.WorkInstructionId, ct);

        // Supersede the prior active revision, if any.
        if (instruction.ActiveRevisionId.HasValue)
        {
            var prior = await db.WorkInstructionRevisions.FirstAsync(r => r.Id == instruction.ActiveRevisionId, ct);
            prior.Status = WorkInstructionRevisionStatus.Superseded;
            prior.ModifiedDate = DateTime.UtcNow;
        }

        var now = DateTime.UtcNow;
        revision.Status = WorkInstructionRevisionStatus.Published;
        revision.PublishedAt = now;
        revision.ModifiedDate = now;
        instruction.ActiveRevisionId = revision.Id;
        instruction.ModifiedDate = now;
        await db.SaveChangesAsync(ct);
        _ = userId;

        logger.LogInformation("Published WorkInstruction {Id} revision {Number}",
            instruction.Id, revision.RevisionNumber);
    }
}
