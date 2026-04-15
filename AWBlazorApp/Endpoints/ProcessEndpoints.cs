using System.Security.Claims;
using Cronos;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Data.Entities.ProcessManagement;
using AWBlazorApp.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints;

public static class ProcessEndpoints
{
    public static IEndpointRouteBuilder MapProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/processes")
            .WithTags("Processes")
            .RequireAuthorization("ApiOrCookie");

        // GET / — list processes (paged)
        group.MapGet("/", async (IDbContextFactory<ApplicationDbContext> dbFactory, int page = 0, int pageSize = 25) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var query = db.Processes.AsNoTracking().Where(p => p.DeletedDate == null);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip(page * pageSize).Take(pageSize)
                .Select(p => p.ToDto())
                .ToListAsync();
            return Results.Ok(new { items, total });
        });

        // GET /{id} — single process
        group.MapGet("/{id:int}", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.Processes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.DeletedDate == null);
            return entity is null ? Results.NotFound() : Results.Ok(entity.ToDto());
        });

        // GET /{id}/steps — list steps for a process
        group.MapGet("/{id:int}/steps", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var steps = await db.ProcessSteps.AsNoTracking()
                .Where(s => s.ProcessId == id)
                .OrderBy(s => s.SequenceNumber)
                .Select(s => s.ToDto())
                .ToListAsync();
            return Results.Ok(steps);
        });

        // POST / — create process
        group.MapPost("/", async (CreateProcessRequest request,
            IValidator<CreateProcessRequest> validator,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = request.ToEntity();

            if (entity.IsRecurring && !string.IsNullOrWhiteSpace(entity.CronSchedule))
            {
                var cron = CronExpression.Parse(entity.CronSchedule);
                entity.NextRunDate = cron.GetNextOccurrence(DateTime.UtcNow);
            }

            db.Processes.Add(entity);
            await db.SaveChangesAsync();
            return Results.Created($"/api/processes/{entity.Id}", entity.ToDto());
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        // PATCH /{id} — update process
        group.MapPatch("/{id:int}", async (int id, UpdateProcessRequest request,
            IValidator<UpdateProcessRequest> validator,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.Processes.FirstOrDefaultAsync(p => p.Id == id && p.DeletedDate == null);
            if (entity is null) return Results.NotFound();

            request.ApplyTo(entity);

            // Recompute NextRunDate if cron or recurring changed
            if (entity.IsRecurring && !string.IsNullOrWhiteSpace(entity.CronSchedule))
            {
                var cron = CronExpression.Parse(entity.CronSchedule);
                entity.NextRunDate = cron.GetNextOccurrence(DateTime.UtcNow);
            }
            else
            {
                entity.NextRunDate = null;
            }

            await db.SaveChangesAsync();
            return Results.Ok(entity.ToDto());
        });

        // DELETE /{id} — soft-delete
        group.MapDelete("/{id:int}", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.Processes.FirstOrDefaultAsync(p => p.Id == id && p.DeletedDate == null);
            if (entity is null) return Results.NotFound();

            entity.DeletedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        // POST /{id}/steps — add step
        group.MapPost("/{id:int}/steps", async (int id, CreateProcessStepRequest request,
            IValidator<CreateProcessStepRequest> validator,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            await using var db = await dbFactory.CreateDbContextAsync();
            var process = await db.Processes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.DeletedDate == null);
            if (process is null) return Results.NotFound();

            var entity = request.ToEntity(id);
            db.ProcessSteps.Add(entity);
            await db.SaveChangesAsync();
            return Results.Created($"/api/processes/{id}/steps/{entity.Id}", entity.ToDto());
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        // PATCH /{id}/steps/{stepId} — update step
        group.MapPatch("/{id:int}/steps/{stepId:int}", async (int id, int stepId, UpdateProcessStepRequest request,
            IValidator<UpdateProcessStepRequest> validator,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.ProcessSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.ProcessId == id);
            if (entity is null) return Results.NotFound();

            request.ApplyTo(entity);
            await db.SaveChangesAsync();
            return Results.Ok(entity.ToDto());
        });

        // DELETE /{id}/steps/{stepId} — delete step
        group.MapDelete("/{id:int}/steps/{stepId:int}", async (int id, int stepId, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.ProcessSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.ProcessId == id);
            if (entity is null) return Results.NotFound();

            db.ProcessSteps.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        // POST /{id}/trigger — create a new execution
        group.MapPost("/{id:int}/trigger", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var process = await db.Processes
                .Include(p => p.Steps)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedDate == null);
            if (process is null) return Results.NotFound();

            var execution = new ProcessExecution
            {
                ProcessId = process.Id,
                ExecutionDate = DateTime.UtcNow,
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

            // If recurring, compute next run date
            if (process.IsRecurring && !string.IsNullOrWhiteSpace(process.CronSchedule))
            {
                var cron = CronExpression.Parse(process.CronSchedule);
                process.NextRunDate = cron.GetNextOccurrence(DateTime.UtcNow);
            }

            await db.SaveChangesAsync();

            // Reload with step executions for the DTO
            await db.Entry(execution).Collection(e => e.StepExecutions).LoadAsync();
            foreach (var se in execution.StepExecutions)
                await db.Entry(se).Reference(s => s.ProcessStep).LoadAsync();

            return Results.Created($"/api/processes/executions/{execution.Id}", execution.ToDto());
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        // GET /{id}/executions — list executions for a process
        group.MapGet("/{id:int}/executions", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory, int page = 0, int pageSize = 25) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var query = db.ProcessExecutions.AsNoTracking()
                .Include(e => e.StepExecutions)
                .Where(e => e.ProcessId == id);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(e => e.ExecutionDate)
                .Skip(page * pageSize).Take(pageSize)
                .ToListAsync();
            return Results.Ok(new { items = items.Select(e => e.ToDto()), total });
        });

        // GET /executions/{execId} — single execution with step executions
        group.MapGet("/executions/{execId:int}", async (int execId, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var execution = await db.ProcessExecutions.AsNoTracking()
                .Include(e => e.StepExecutions).ThenInclude(se => se.ProcessStep)
                .FirstOrDefaultAsync(e => e.Id == execId);
            if (execution is null) return Results.NotFound();

            return Results.Ok(new
            {
                execution = execution.ToDto(),
                steps = execution.StepExecutions
                    .OrderBy(se => se.ProcessStep.SequenceNumber)
                    .Select(se => se.ToDto()),
            });
        });

        // PATCH /executions/{execId}/steps/{stepExecId} — complete/skip a step execution
        group.MapPatch("/executions/{execId:int}/steps/{stepExecId:int}", async (
            int execId, int stepExecId,
            UpdateStepExecutionRequest request,
            HttpContext httpContext,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var stepExec = await db.ProcessStepExecutions
                .FirstOrDefaultAsync(se => se.Id == stepExecId && se.ProcessExecutionId == execId);
            if (stepExec is null) return Results.NotFound();

            stepExec.Status = request.Status;
            stepExec.Notes = request.Notes;

            if (request.Status is ProcessStepExecutionStatus.Completed or ProcessStepExecutionStatus.Skipped)
            {
                stepExec.CompletedByUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                stepExec.CompletedDate = DateTime.UtcNow;
            }

            // Check if all steps in the execution are done
            var allSteps = await db.ProcessStepExecutions
                .Where(se => se.ProcessExecutionId == execId)
                .ToListAsync();

            var allDone = allSteps.All(se =>
                se.Id == stepExecId
                    ? request.Status is ProcessStepExecutionStatus.Completed or ProcessStepExecutionStatus.Skipped
                    : se.Status is ProcessStepExecutionStatus.Completed or ProcessStepExecutionStatus.Skipped);

            if (allDone)
            {
                var execution = await db.ProcessExecutions.FirstAsync(e => e.Id == execId);
                execution.Status = ProcessExecutionStatus.Completed;
                execution.CompletedDate = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            // Reload for DTO
            await db.Entry(stepExec).Reference(se => se.ProcessStep).LoadAsync();
            return Results.Ok(stepExec.ToDto());
        });

        return app;
    }
}
