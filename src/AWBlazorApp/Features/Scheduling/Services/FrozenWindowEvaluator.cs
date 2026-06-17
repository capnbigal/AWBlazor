using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.Services;

public class FrozenWindowEvaluator : IFrozenWindowEvaluator
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    public FrozenWindowEvaluator(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

    public async Task<bool> EvaluateAsync(SalesOrderHeader soh, DateTime nowUtc, short locationId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var weekId = IsoWeekHelper.FromDate(soh.DueDate);
        var planExists = await db.WeeklyPlans.AsNoTracking()
            .AnyAsync(p => p.WeekId == weekId && p.LocationId == locationId, ct);
        if (!planExists) return false;

        var lookahead = await db.LineConfigurations.AsNoTracking()
            .Where(l => l.LocationId == locationId && l.IsActive)
            .Select(l => (int?)l.FrozenLookaheadHours)
            .SingleOrDefaultAsync(ct);
        if (lookahead is null) return false;

        var hoursUntilDue = (soh.DueDate - nowUtc).TotalHours;
        return hoursUntilDue < lookahead.Value;
    }
}
