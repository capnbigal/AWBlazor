using Cronos;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Features.ProcessManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.ProcessManagement.Services;

public sealed class ProcessSchedulerJob(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<ProcessSchedulerJob> logger)
{
    public async Task ExecuteAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;

        var dueProcesses = await db.Processes
            .Include(p => p.Steps)
            .Where(p => p.Status == ProcessStatus.Active
                     && p.DeletedDate == null
                     && p.IsRecurring
                     && p.NextRunDate != null
                     && p.NextRunDate <= now)
            .ToListAsync();

        if (dueProcesses.Count == 0)
        {
            logger.LogInformation("ProcessSchedulerJob: no recurring processes due.");
            return;
        }

        var triggered = 0;
        foreach (var process in dueProcesses)
        {
            try
            {
                var execution = new ProcessExecution
                {
                    ProcessId = process.Id,
                    ExecutionDate = now,
                    AssignedUserId = process.DefaultProcessorUserId,
                    Status = ProcessExecutionStatus.Pending,
                };
                db.ProcessExecutions.Add(execution);

                foreach (var step in process.Steps.OrderBy(s => s.SequenceNumber))
                {
                    db.ProcessStepExecutions.Add(new ProcessStepExecution
                    {
                        ProcessExecution = execution,
                        ProcessStepId = step.Id,
                        Status = ProcessStepExecutionStatus.Pending,
                    });
                }

                // Compute next run date
                if (!string.IsNullOrWhiteSpace(process.CronSchedule))
                {
                    var cron = CronExpression.Parse(process.CronSchedule);
                    process.NextRunDate = cron.GetNextOccurrence(now);
                }

                triggered++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "ProcessSchedulerJob: failed to trigger process {ProcessId} '{ProcessName}'.",
                    process.Id, process.Name);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("ProcessSchedulerJob: triggered {Count} of {Total} due recurring processes.",
            triggered, dueProcesses.Count);
    }
}
