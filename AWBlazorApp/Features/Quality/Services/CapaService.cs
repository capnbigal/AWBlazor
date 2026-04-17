using AWBlazorApp.Features.Quality.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Services;

/// <inheritdoc />
public sealed class CapaService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<CapaService> logger) : ICapaService
{
    public async Task<int> OpenAsync(string title, int? ownerBusinessEntityId, IEnumerable<int> linkedNcrIds, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var now = DateTime.UtcNow;
        var capa = new CapaCase
        {
            CaseNumber = $"CAPA-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            Title = title.Trim(),
            Status = CapaStatus.Open,
            OwnerBusinessEntityId = ownerBusinessEntityId,
            OpenedAt = now,
            ModifiedDate = now,
        };
        db.CapaCases.Add(capa);
        await db.SaveChangesAsync(ct);

        foreach (var ncrId in linkedNcrIds.Distinct())
        {
            db.CapaCaseNonConformances.Add(new CapaCaseNonConformance
            {
                CapaCaseId = capa.Id,
                NonConformanceId = ncrId,
                LinkedAt = now,
            });
        }
        await db.SaveChangesAsync(ct);
        _ = userId;

        logger.LogInformation("Opened CAPA {Number} with {Count} linked NCRs", capa.CaseNumber, linkedNcrIds.Count());
        return capa.Id;
    }

    public async Task LinkNcrAsync(int capaCaseId, int nonConformanceId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var exists = await db.CapaCaseNonConformances.AnyAsync(l => l.CapaCaseId == capaCaseId && l.NonConformanceId == nonConformanceId, ct);
        if (exists) return;
        db.CapaCaseNonConformances.Add(new CapaCaseNonConformance
        {
            CapaCaseId = capaCaseId,
            NonConformanceId = nonConformanceId,
            LinkedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task UnlinkNcrAsync(int capaCaseId, int nonConformanceId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var link = await db.CapaCaseNonConformances.FirstOrDefaultAsync(l => l.CapaCaseId == capaCaseId && l.NonConformanceId == nonConformanceId, ct);
        if (link is null) return;
        db.CapaCaseNonConformances.Remove(link);
        await db.SaveChangesAsync(ct);
    }

    public async Task TransitionAsync(int capaCaseId, CapaStatus target, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var capa = await db.CapaCases.FirstOrDefaultAsync(c => c.Id == capaCaseId, ct)
            ?? throw new InvalidOperationException($"CapaCase {capaCaseId} not found.");
        if (capa.Status == CapaStatus.Closed)
            throw new InvalidOperationException("CAPA case is already closed.");

        // Linear progression — only one step at a time, but Closed can be reached from any stage.
        var legal = capa.Status switch
        {
            CapaStatus.Open               => target is CapaStatus.Investigation or CapaStatus.Closed,
            CapaStatus.Investigation      => target is CapaStatus.CorrectiveAction or CapaStatus.Closed,
            CapaStatus.CorrectiveAction   => target is CapaStatus.Verification or CapaStatus.Closed,
            CapaStatus.Verification       => target is CapaStatus.Closed,
            _ => false,
        };
        if (!legal) throw new InvalidOperationException($"Cannot transition CAPA {capa.Status} → {target}.");

        capa.Status = target;
        if (target == CapaStatus.Closed) capa.ClosedAt = DateTime.UtcNow;
        capa.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        _ = userId;
    }
}
