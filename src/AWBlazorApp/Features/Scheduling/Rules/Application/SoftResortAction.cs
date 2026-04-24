using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public class SoftResortAction : IRecalcAction
{
    public RecalcActionType ActionType => RecalcActionType.SoftResort;

    public Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct)
    {
        ctx.Db.SchedulingAlerts.Add(new SchedulingAlert
        {
            CreatedAt = ctx.NowUtc,
            Severity = AlertSeverity.Info,
            EventType = ctx.Rule.EventType,
            WeekId = ctx.WeekId,
            LocationId = ctx.LocationId,
            SalesOrderId = ctx.Soh.Id,
            Message = $"New SO {ctx.Soh.Id} soft-resorted into {IsoWeekHelper.Format(ctx.WeekId)}.",
            PayloadJson = $"{{\"ruleId\":{ctx.Rule.Id}}}"
        });
        return Task.FromResult(new RecalcResult(Handled: true));
    }
}
