using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AWBlazorApp.Features.Scheduling.Services;

public class SchedulingDispatcher : ISchedulingDispatcher
{
    private static readonly AsyncLocal<bool> _inFlight = new();
    public bool IsDispatching => _inFlight.Value;

    private readonly IFrozenWindowEvaluator _frozen;
    private readonly ISchedulingRuleResolver _resolver;
    private readonly IEnumerable<IRecalcAction> _actions;
    private readonly ILogger<SchedulingDispatcher> _log;

    public SchedulingDispatcher(IFrozenWindowEvaluator frozen,
        ISchedulingRuleResolver resolver, IEnumerable<IRecalcAction> actions,
        ILogger<SchedulingDispatcher> log)
        => (_frozen, _resolver, _actions, _log) = (frozen, resolver, actions, log);

    public async Task OnSalesOrderCreatedAsync(SalesOrderHeader soh, short locationId,
        ApplicationDbContext db, CancellationToken ct)
    {
        if (_inFlight.Value) return;

        var hasLine = await db.LineConfigurations.AsNoTracking()
            .AnyAsync(l => l.LocationId == locationId && l.IsActive, ct);
        if (!hasLine) { _log.LogDebug("No LineConfiguration for {Loc}; skipping dispatch.", locationId); return; }

        var inFrozen = await _frozen.EvaluateAsync(soh, DateTime.UtcNow, locationId, ct);
        var weekId = IsoWeekHelper.FromDate(soh.DueDate);

        var rules = await db.SchedulingRules.AsNoTracking().ToListAsync(ct);
        var candidates = _resolver.Resolve(rules, SchedulingEventType.NewSO, inFrozen).ToList();
        if (candidates.Count == 0)
        {
            _log.LogDebug("No rules for NewSO inFrozen={InFrozen}.", inFrozen);
            return;
        }

        _inFlight.Value = true;
        try
        {
            foreach (var rule in candidates)
            {
                var action = _actions.FirstOrDefault(a => a.ActionType == rule.Action);
                if (action is null)
                {
                    _log.LogWarning("No IRecalcAction for {Type}.", rule.Action);
                    continue;
                }

                var ctx = new RecalcContext(db, rule, soh, locationId, weekId, inFrozen, DateTime.UtcNow);
                var result = await action.ExecuteAsync(ctx, ct);
                if (result.Handled) return;
            }
            _log.LogInformation("No rule handled SO {Id}; falling through.", soh.Id);
        }
        finally { _inFlight.Value = false; }
    }
}
