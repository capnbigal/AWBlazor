using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.Insights;

/// <summary>
/// Scheduled email delivery of a SavedQuery result as a CSV attachment. A Hangfire recurring job
/// is registered/removed per schedule keyed on "report-{Id}" — so creating/activating a schedule
/// adds the job, deactivating or deleting removes it.
/// </summary>
public class ReportSchedule
{
    public int Id { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SavedQueryId { get; set; }

    /// <summary>Cron expression (Hangfire-style, 5 fields). Example: "0 8 * * 1" = 8am every Monday.</summary>
    [Required, MaxLength(100)]
    public string Cron { get; set; } = "0 8 * * 1";

    /// <summary>Comma-separated list of recipient emails.</summary>
    [Required, MaxLength(500)]
    public string Recipients { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastRunDate { get; set; }
}
