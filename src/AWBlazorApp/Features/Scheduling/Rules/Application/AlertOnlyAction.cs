using System.Text.Json;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public class AlertOnlyAction : IRecalcAction
{
    public RecalcActionType ActionType => RecalcActionType.AlertOnly;

    public Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct)
    {
        var threshold = ReadMinOrderValue(ctx.Rule.ParametersJson);
        if (threshold.HasValue && ctx.Soh.TotalDue < threshold.Value)
            return Task.FromResult(new RecalcResult(Handled: false,
                Note: $"Below minOrderValue={threshold}"));

        ctx.Db.SchedulingAlerts.Add(new SchedulingAlert
        {
            CreatedAt = ctx.NowUtc,
            Severity = AlertSeverity.Warning,
            EventType = ctx.Rule.EventType,
            WeekId = ctx.WeekId,
            LocationId = ctx.LocationId,
            SalesOrderId = ctx.Soh.Id,
            Message = $"Frozen-window SO {ctx.Soh.Id} (${ctx.Soh.TotalDue:F2}) due {ctx.Soh.DueDate:u} in {IsoWeekHelper.Format(ctx.WeekId)}.",
            PayloadJson = $"{{\"ruleId\":{ctx.Rule.Id},\"totalDue\":{ctx.Soh.TotalDue}}}"
        });
        return Task.FromResult(new RecalcResult(Handled: true));
    }

    private static decimal? ReadMinOrderValue(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("minOrderValue", out var el) && el.TryGetDecimal(out var v))
                return v;
        }
        catch (JsonException) { }
        return null;
    }
}
