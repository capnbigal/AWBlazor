using AWBlazorApp.Data;
using AWBlazorApp.Data.Entities;
using AWBlazorApp.Data.Entities.ProcessManagement;
using AWBlazorApp.Data.Entities.Forecasting;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Services;

/// <summary>
/// Evaluates every active NotificationRule on a recurring schedule. For each rule, computes the
/// target metric's current value, compares against the threshold, and — if the condition is met
/// and the per-rule cooldown has elapsed — pushes a SignalR notification to the rule's owner.
/// </summary>
public sealed class NotificationRuleEvaluator(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    NotificationService notifications,
    ILogger<NotificationRuleEvaluator> logger)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rules = await db.NotificationRules.Where(r => r.IsActive).ToListAsync(ct);
        if (rules.Count == 0) return;

        var now = DateTime.UtcNow;
        var metricCache = new Dictionary<NotificationMetric, double>();

        foreach (var rule in rules)
        {
            try
            {
                if (!metricCache.TryGetValue(rule.Metric, out var value))
                {
                    value = await ReadMetricAsync(db, rule.Metric, ct);
                    metricCache[rule.Metric] = value;
                }

                rule.LastValue = value;
                rule.LastEvaluatedDate = now;

                if (Matches(value, rule.Operator, rule.Threshold)
                    && (rule.LastTriggeredDate is null
                        || (now - rule.LastTriggeredDate.Value).TotalMinutes >= rule.CooldownMinutes))
                {
                    var verb = rule.Operator switch
                    {
                        NotificationOperator.GreaterThan => "exceeded",
                        NotificationOperator.LessThan    => "dropped below",
                        _                                => "hit",
                    };
                    var message = $"[{rule.Name}] {Label(rule.Metric)} = {value:N0} {verb} {rule.Threshold:N0}";
                    await notifications.NotifyUserAsync(rule.UserId, message);
                    rule.LastTriggeredDate = now;
                    logger.LogInformation("NotificationRule {RuleId} ({Name}) fired for user {UserId}: {Message}",
                        rule.Id, rule.Name, rule.UserId, message);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to evaluate NotificationRule {RuleId}", rule.Id);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static bool Matches(double value, NotificationOperator op, double threshold) => op switch
    {
        NotificationOperator.GreaterThan => value > threshold,
        NotificationOperator.LessThan    => value < threshold,
        NotificationOperator.EqualTo     => Math.Abs(value - threshold) < 0.0001,
        _                                => false,
    };

    private static async Task<double> ReadMetricAsync(ApplicationDbContext db, NotificationMetric metric, CancellationToken ct)
    {
        var yesterday = DateTime.UtcNow.AddHours(-24);
        return metric switch
        {
            NotificationMetric.OpenWorkOrders        => await db.WorkOrders.CountAsync(w => w.EndDate == null, ct),
            NotificationMetric.ActiveForecasts       => await db.ForecastDefinitions.CountAsync(f => f.DeletedDate == null && f.Status == ForecastStatus.Active, ct),
            NotificationMetric.ActiveProcesses       => await db.Processes.CountAsync(p => p.DeletedDate == null && p.Status == ProcessStatus.Active, ct),
            NotificationMetric.SalesOrdersLast24h    => await db.SalesOrderHeaders.CountAsync(o => o.OrderDate >= yesterday, ct),
            NotificationMetric.UsersRegistered       => await db.Users.CountAsync(ct),
            NotificationMetric.FailedLoginsLast24h   => await db.SecurityAuditLogs.CountAsync(a => a.EventType == "LoginFailed" && a.Timestamp >= yesterday, ct),
            _                                        => 0,
        };
    }

    public static string Label(NotificationMetric m) => m switch
    {
        NotificationMetric.OpenWorkOrders      => "Open work orders",
        NotificationMetric.ActiveForecasts     => "Active forecasts",
        NotificationMetric.ActiveProcesses     => "Active processes",
        NotificationMetric.SalesOrdersLast24h  => "Sales orders (24h)",
        NotificationMetric.UsersRegistered     => "Registered users",
        NotificationMetric.FailedLoginsLast24h => "Failed logins (24h)",
        _                                      => m.ToString(),
    };
}
