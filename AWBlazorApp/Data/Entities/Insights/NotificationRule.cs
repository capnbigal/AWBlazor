using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.Insights;

/// <summary>
/// User-defined alert rule evaluated by <see cref="Services.NotificationRuleEvaluator"/> on a
/// recurring Hangfire schedule. When a rule's metric crosses its threshold and the cooldown
/// window has elapsed, <see cref="Services.NotificationService"/> pushes a SignalR message to
/// the owning user's active connections.
/// </summary>
public class NotificationRule
{
    public int Id { get; set; }

    /// <summary>FK to AspNetUsers.Id — the user who owns this rule and receives the alerts.</summary>
    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public NotificationMetric Metric { get; set; }

    public NotificationOperator Operator { get; set; } = NotificationOperator.GreaterThan;

    public double Threshold { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Last time this rule's condition was evaluated (regardless of trigger).</summary>
    public DateTime? LastEvaluatedDate { get; set; }

    /// <summary>Most recent metric value observed, for display in the UI.</summary>
    public double? LastValue { get; set; }

    /// <summary>Last time this rule fired a notification (used for cooldown).</summary>
    public DateTime? LastTriggeredDate { get; set; }

    /// <summary>Minimum minutes between consecutive triggers for this rule.</summary>
    public int CooldownMinutes { get; set; } = 60;
}

public enum NotificationMetric
{
    OpenWorkOrders,
    ActiveForecasts,
    ActiveProcesses,
    SalesOrdersLast24h,
    UsersRegistered,
    FailedLoginsLast24h,
}

public enum NotificationOperator
{
    GreaterThan,
    LessThan,
    EqualTo,
}
