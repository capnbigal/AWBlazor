using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public class HardReplanAction : IRecalcAction
{
    public RecalcActionType ActionType => RecalcActionType.HardReplan;

    public async Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct)
    {
        // Flip BaselineDiverged on the latest plan for this (WeekId, LocationId)
        var plan = await ctx.Db.WeeklyPlans
            .Where(p => p.WeekId == ctx.WeekId && p.LocationId == ctx.LocationId)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(ct);
        if (plan is not null) plan.BaselineDiverged = true;

        ctx.Db.SchedulingAlerts.Add(new SchedulingAlert
        {
            CreatedAt = ctx.NowUtc,
            Severity = AlertSeverity.Critical,
            EventType = ctx.Rule.EventType,
            WeekId = ctx.WeekId,
            LocationId = ctx.LocationId,
            SalesOrderId = ctx.Soh.Id,
            Message = $"Frozen-window SO {ctx.Soh.Id} triggered HARD REPLAN in {IsoWeekHelper.Format(ctx.WeekId)}.",
            PayloadJson = $"{{\"ruleId\":{ctx.Rule.Id},\"planFound\":{(plan is null ? "false" : "true")}}}"
        });
        return new RecalcResult(Handled: true);
    }
}
