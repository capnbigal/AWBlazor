using System.Text.Json;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public class WeeklyPlanGenerator : IWeeklyPlanGenerator
{
    private static readonly IReadOnlySet<byte> DefaultPlannable =
        new HashSet<byte> { 1, 2 }; // InProcess, Approved

    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public WeeklyPlanGenerator(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

    public async Task<WeeklyPlanGenerationResult> GenerateAsync(
        int weekId, short locationId, WeeklyPlanGenerationOptions options,
        string requestedBy, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var line = await db.LineConfigurations.AsNoTracking()
            .SingleOrDefaultAsync(l => l.LocationId == locationId && l.IsActive, ct)
            ?? throw new InvalidOperationException($"Line {locationId} not configured.");

        var weekStart = IsoWeekHelper.ToMondayUtc(weekId);
        var weekEnd = weekStart.AddDays(7);
        var plannable = options.PlannableSalesOrderStatuses ?? DefaultPlannable;
        var warnings = new List<string>();

        var assignedModelIds = await db.LineProductAssignments.AsNoTracking()
            .Where(a => a.LocationId == locationId && a.IsActive)
            .Select(a => a.ProductModelId).ToListAsync(ct);

        if (assignedModelIds.Count == 0)
            warnings.Add("No LineProductAssignments for location; plan will be empty.");

        var candidateQ = from sod in db.SalesOrderDetails
                         join soh in db.SalesOrderHeaders on sod.SalesOrderId equals soh.Id
                         join prod in db.Products on sod.ProductId equals prod.Id
                         where soh.DueDate >= weekStart && soh.DueDate < weekEnd
                            && plannable.Contains(soh.Status)
                            && prod.ProductModelId != null
                            && assignedModelIds.Contains(prod.ProductModelId!.Value)
                         select new
                         {
                             SohId = soh.Id,
                             soh.DueDate,
                             soh.OnlineOrderFlag,
                             soh.TotalDue,
                             soh.ModifiedDate,
                             ProdModelId = prod.ProductModelId!.Value,
                             sod.ProductId,
                             sod.SalesOrderDetailId,
                             sod.OrderQty
                         };

        var raw = await candidateQ.AsNoTracking().ToListAsync(ct);

        var sorted = raw
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => !x.OnlineOrderFlag) // dealer (non-online) ranks higher
            .ThenBy(x => x.ProdModelId)
            .ThenByDescending(x => x.TotalDue)
            .ThenBy(x => x.ModifiedDate)
            .ThenBy(x => x.SalesOrderDetailId)
            .ToList();

        var workingSecondsPerDay = (long)line.ShiftsPerDay * line.MinutesPerShift * 60;
        var weekCapacitySeconds = workingSecondsPerDay * 7;
        DateTime cursor = weekStart;
        long secondsUsedOnDay = 0;

        var items = new List<WeeklyPlanItem>();
        int overCap = 0;

        for (int k = 0; k < sorted.Count; k++)
        {
            var row = sorted[k];
            var duration = (long)row.OrderQty * line.TaktSeconds;

            if (secondsUsedOnDay >= workingSecondsPerDay)
            {
                cursor = cursor.Date.AddDays(1);
                secondsUsedOnDay = 0;
            }

            var plannedStart = cursor;
            var plannedEnd = cursor.AddSeconds(duration);
            var isOver = plannedStart >= weekEnd;
            if (isOver) overCap++;

            items.Add(new WeeklyPlanItem
            {
                SalesOrderId = row.SohId,
                SalesOrderDetailId = row.SalesOrderDetailId,
                ProductId = row.ProductId,
                PlannedSequence = k + 1,
                PlannedStart = plannedStart,
                PlannedEnd = plannedEnd,
                PlannedQty = row.OrderQty,
                OverCapacity = isOver
            });

            cursor = plannedEnd;
            secondsUsedOnDay += duration;
        }

        if (options.StrictCapacity && overCap > 0)
        {
            var excessSeconds = items
                .Where(i => i.OverCapacity)
                .Sum(i => (i.PlannedEnd - i.PlannedStart).TotalSeconds);
            throw new CapacityExceededException(excessSeconds / 3600.0);
        }

        var utilPct = weekCapacitySeconds == 0 ? 0m
            : (decimal)Math.Min(1.0, items.Sum(i => (i.PlannedEnd - i.PlannedStart).TotalSeconds) / weekCapacitySeconds) * 100m;

        if (options.DryRun)
            return new WeeklyPlanGenerationResult(0, 0, items.Count, overCap, utilPct, warnings);

        var existing = await db.WeeklyPlans
            .Where(p => p.WeekId == weekId && p.LocationId == locationId)
            .OrderByDescending(p => p.Version)
            .ToListAsync(ct);

        int nextVersion;
        if (existing.Count == 0)
        {
            nextVersion = 1;
        }
        else if (weekStart > DateTime.UtcNow.Date)
        {
            // Future week: hard-delete old plans and restart versioning
            var ids = existing.Select(p => p.Id).ToList();
            await db.WeeklyPlanItems.Where(i => ids.Contains(i.WeeklyPlanId)).ExecuteDeleteAsync(ct);
            await db.WeeklyPlans.Where(p => ids.Contains(p.Id)).ExecuteDeleteAsync(ct);
            nextVersion = 1;
        }
        else
        {
            // Past / current week: version bump
            nextVersion = existing[0].Version + 1;
        }

        var plan = new WeeklyPlan
        {
            WeekId = weekId,
            LocationId = locationId,
            Version = nextVersion,
            PublishedAt = DateTime.UtcNow,
            PublishedBy = requestedBy,
            BaselineDiverged = false,
            GenerationOptionsJson = JsonSerializer.Serialize(options)
        };
        db.WeeklyPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        foreach (var item in items) item.WeeklyPlanId = plan.Id;
        db.WeeklyPlanItems.AddRange(items);
        await db.SaveChangesAsync(ct);

        return new WeeklyPlanGenerationResult(plan.Id, plan.Version, items.Count, overCap, utilPct, warnings);
    }
}
