using System.Text;
using AWBlazorApp.Services.Jobs;
using AWBlazorApp.Features.Insights.Domain;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Data.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Insights.Services;

/// <summary>
/// Runs a scheduled report: loads the ReportSchedule + its SavedQuery, executes the query,
/// renders a CSV, and enqueues one SmtpEmailJob per recipient with the CSV attached.
/// Registered as a recurring Hangfire job keyed on "report-{id}" by <see cref="ReportScheduleRegistry"/>.
/// </summary>
public sealed class ReportDispatcher(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    SavedQueryRunner runner,
    IBackgroundJobClient jobs,
    ILogger<ReportDispatcher> logger)
{
    public async Task DispatchAsync(int scheduleId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var schedule = await db.ReportSchedules.FirstOrDefaultAsync(s => s.Id == scheduleId, ct);
        if (schedule is null || !schedule.IsActive)
        {
            logger.LogInformation("Report schedule {ScheduleId} is missing or inactive — skipping.", scheduleId);
            return;
        }

        var query = await db.SavedQueries.FirstOrDefaultAsync(q => q.Id == schedule.SavedQueryId, ct);
        if (query is null)
        {
            logger.LogWarning("Report schedule {ScheduleId} references missing SavedQuery {QueryId} — skipping.",
                scheduleId, schedule.SavedQueryId);
            return;
        }

        SavedQueryRunner.QueryResult result;
        try
        {
            result = await runner.RunAsync(query, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Report schedule {ScheduleId} query failed.", scheduleId);
            return;
        }

        var csv = BuildCsv(query, result);
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"{Sanitize(query.Name)}-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv";

        var recipients = schedule.Recipients
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var body = $"Attached: {query.Name} ({SavedQueryRunner.MetricLabel(query.Metric)}).\n"
                 + $"Schedule: {schedule.Cron}\n"
                 + $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC\n"
                 + $"Rows: {result.Buckets.Count}";

        foreach (var recipient in recipients)
        {
            jobs.Enqueue<SmtpEmailJob>(j => j.SendWithAttachmentAsync(
                recipient, null,
                $"[AWBlazor report] {schedule.Name}",
                body, isHtml: false,
                bytes, fileName));
        }

        schedule.LastRunDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Dispatched report {ScheduleId} ({Name}) to {Count} recipient(s).",
            scheduleId, schedule.Name, recipients.Length);
    }

    private static string BuildCsv(SavedQuery query, SavedQueryRunner.QueryResult result)
    {
        var sb = new StringBuilder();
        sb.Append("Period,").AppendLine(SavedQueryRunner.MetricLabel(query.Metric));
        foreach (var bucket in result.Buckets)
        {
            sb.Append(bucket.Period?.ToString("yyyy-MM-dd") ?? "all")
              .Append(',')
              .AppendLine(bucket.Value.ToString("0.####"));
        }
        return sb.ToString();
    }

    private static string Sanitize(string name)
    {
        var cleaned = new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        return cleaned.Trim('-').Length == 0 ? "report" : cleaned.Trim('-');
    }
}

/// <summary>
/// Registers / unregisters Hangfire recurring jobs for ReportSchedule rows. Keeps Hangfire in
/// sync when schedules are created, activated, deactivated, or deleted.
/// </summary>
public sealed class ReportScheduleRegistry(
    IRecurringJobManager recurring)
{
    public static string JobId(int scheduleId) => $"report-{scheduleId}";

    public void Register(ReportSchedule schedule)
    {
        recurring.AddOrUpdate<ReportDispatcher>(
            JobId(schedule.Id),
            d => d.DispatchAsync(schedule.Id, CancellationToken.None),
            schedule.Cron);
    }

    public void Remove(int scheduleId) => recurring.RemoveIfExists(JobId(scheduleId));
}
