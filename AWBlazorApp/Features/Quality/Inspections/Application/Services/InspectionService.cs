using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Quality.Inspections.Application.Services;

/// <inheritdoc />
public sealed class InspectionService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<InspectionService> logger) : IInspectionService
{
    public async Task<int> CreateAsync(CreateInspectionInput input, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var plan = await db.InspectionPlans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == input.InspectionPlanId, ct)
            ?? throw new InvalidOperationException($"InspectionPlan {input.InspectionPlanId} not found.");
        if (!plan.IsActive)
            throw new InvalidOperationException($"InspectionPlan {plan.PlanCode} is inactive.");

        var inspection = new Inspection
        {
            InspectionNumber = $"INS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            InspectionPlanId = input.InspectionPlanId,
            Status = InspectionStatus.Pending,
            SourceKind = input.SourceKind,
            SourceId = input.SourceId,
            InventoryItemId = input.InventoryItemId,
            LotId = input.LotId,
            Quantity = input.Quantity,
            UnitMeasureCode = input.UnitMeasureCode ?? "EA",
            Notes = input.Notes,
            PostedByUserId = userId,
            ModifiedDate = DateTime.UtcNow,
        };
        db.Inspections.Add(inspection);
        await db.SaveChangesAsync(ct);
        return inspection.Id;
    }

    public async Task<InspectionStatus> StartAsync(int id, int? inspectorBusinessEntityId, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var inspection = await db.Inspections.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException($"Inspection {id} not found.");
        if (inspection.Status != InspectionStatus.Pending)
            throw new InvalidOperationException($"Inspection is {inspection.Status}; only Pending can start.");
        inspection.Status = InspectionStatus.InProgress;
        inspection.InspectorBusinessEntityId = inspectorBusinessEntityId;
        inspection.InspectedAt = DateTime.UtcNow;
        inspection.ModifiedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        _ = userId;
        return inspection.Status;
    }

    public async Task<InspectionResult> RecordResultAsync(RecordResultInput input, int? recordedBy, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var inspection = await db.Inspections.AsNoTracking().FirstOrDefaultAsync(i => i.Id == input.InspectionId, ct)
            ?? throw new InvalidOperationException($"Inspection {input.InspectionId} not found.");
        if (inspection.Status is not (InspectionStatus.Pending or InspectionStatus.InProgress))
            throw new InvalidOperationException($"Cannot record results on a {inspection.Status} inspection.");

        var characteristic = await db.InspectionPlanCharacteristics.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == input.InspectionPlanCharacteristicId, ct)
            ?? throw new InvalidOperationException($"Characteristic {input.InspectionPlanCharacteristicId} not found.");

        var passed = EvaluatePass(characteristic, input);

        var result = new InspectionResult
        {
            InspectionId = input.InspectionId,
            InspectionPlanCharacteristicId = input.InspectionPlanCharacteristicId,
            NumericResult = input.NumericResult,
            AttributeResult = input.AttributeResult,
            Passed = passed,
            Notes = input.Notes,
            RecordedAt = DateTime.UtcNow,
            RecordedByBusinessEntityId = recordedBy,
        };
        db.InspectionResults.Add(result);
        await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<CompleteResult> CompleteAsync(int id, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var inspection = await db.Inspections.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException($"Inspection {id} not found.");
        if (inspection.Status is InspectionStatus.Pass or InspectionStatus.Fail)
            throw new InvalidOperationException($"Inspection already {inspection.Status}.");

        var results = await db.InspectionResults.AsNoTracking()
            .Where(r => r.InspectionId == id).ToListAsync(ct);
        var characteristics = await db.InspectionPlanCharacteristics.AsNoTracking()
            .Where(c => c.InspectionPlanId == inspection.InspectionPlanId).ToListAsync(ct);

        // Pass requires: every characteristic on the plan has at least one result, and every
        // result row passed. Otherwise Fail.
        var resultsByCharacteristic = results.GroupBy(r => r.InspectionPlanCharacteristicId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var allCharacteristicsHaveResult = characteristics.All(c => resultsByCharacteristic.ContainsKey(c.Id));
        var allResultsPassed = results.Count > 0 && results.All(r => r.Passed);
        var finalStatus = (allCharacteristicsHaveResult && allResultsPassed)
            ? InspectionStatus.Pass
            : InspectionStatus.Fail;

        inspection.Status = finalStatus;
        inspection.PostedByUserId = userId;
        inspection.ModifiedDate = DateTime.UtcNow;
        if (inspection.InspectedAt is null) inspection.InspectedAt = DateTime.UtcNow;

        int? autoNcrId = null;
        if (finalStatus == InspectionStatus.Fail && inspection.InventoryItemId is not null)
        {
            var ncr = new NonConformance
            {
                NcrNumber = $"NCR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                InspectionId = inspection.Id,
                InventoryItemId = inspection.InventoryItemId.Value,
                LotId = inspection.LotId,
                Quantity = inspection.Quantity,
                UnitMeasureCode = inspection.UnitMeasureCode,
                Description = $"Auto-opened from inspection {inspection.InspectionNumber}: {results.Count(r => !r.Passed)} of {results.Count} results failed.",
                Status = NonConformanceStatus.Open,
                ModifiedDate = DateTime.UtcNow,
            };
            db.NonConformances.Add(ncr);
            await db.SaveChangesAsync(ct);
            autoNcrId = ncr.Id;
            logger.LogInformation("Inspection {Number} failed → auto-opened NCR {Ncr}",
                inspection.InspectionNumber, ncr.NcrNumber);
        }
        else
        {
            await db.SaveChangesAsync(ct);
        }

        return new CompleteResult(inspection.Id, finalStatus, autoNcrId);
    }

    private static bool EvaluatePass(InspectionPlanCharacteristic c, RecordResultInput r)
    {
        if (c.Kind == CharacteristicKind.Numeric)
        {
            if (r.NumericResult is not { } v) return false;
            if (c.MinValue is { } min && v < min) return false;
            if (c.MaxValue is { } max && v > max) return false;
            return true;
        }
        // Attribute: case-insensitive match against ExpectedValue (or any non-empty result if no expected set).
        if (string.IsNullOrWhiteSpace(c.ExpectedValue)) return !string.IsNullOrWhiteSpace(r.AttributeResult);
        return string.Equals(r.AttributeResult?.Trim(), c.ExpectedValue.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
