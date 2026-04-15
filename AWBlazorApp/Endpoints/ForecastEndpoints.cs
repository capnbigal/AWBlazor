using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Data.Entities.Forecasting;
using AWBlazorApp.Models;
using AWBlazorApp.Services.Forecasting;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Endpoints;

public static class ForecastEndpoints
{
    public static IEndpointRouteBuilder MapForecastEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/forecasts")
            .WithTags("Forecasts")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", async (IDbContextFactory<ApplicationDbContext> dbFactory, int page = 0, int pageSize = 25) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var query = db.ForecastDefinitions.AsNoTracking().Where(f => f.DeletedDate == null);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.CreatedDate)
                .Skip(page * pageSize).Take(pageSize)
                .Select(f => f.ToDto())
                .ToListAsync();
            return Results.Ok(new { items, total });
        });

        group.MapGet("/{id:int}", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.ForecastDefinitions.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            return entity is null ? Results.NotFound() : Results.Ok(entity.ToDto());
        });

        group.MapGet("/{id:int}/datapoints", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var points = await db.ForecastDataPoints.AsNoTracking()
                .Where(p => p.ForecastDefinitionId == id)
                .OrderBy(p => p.PeriodDate)
                .Select(p => p.ToDto())
                .ToListAsync();
            return Results.Ok(points);
        });

        group.MapPost("/", async (CreateForecastRequest request,
            IValidator<CreateForecastRequest> validator,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = request.ToEntity();
            db.ForecastDefinitions.Add(entity);
            await db.SaveChangesAsync();
            return Results.Created($"/api/forecasts/{entity.Id}", entity.ToDto());
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPost("/{id:int}/compute", async (int id, IForecastComputationService computeService) =>
        {
            try
            {
                var points = await computeService.ComputeAndSaveAsync(id);
                return Results.Ok(points.Select(p => p.ToDto()));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPost("/preview", async (CreateForecastRequest request, IForecastComputationService computeService) =>
        {
            try
            {
                var preview = await computeService.PreviewAsync(
                    request.DataSource, request.Method, request.Granularity,
                    request.LookbackMonths, request.HorizonPeriods, request.MethodParametersJson);
                return Results.Ok(preview);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));

        group.MapPatch("/{id:int}", async (int id, UpdateForecastRequest request,
            IValidator<UpdateForecastRequest> validator,
            IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.ForecastDefinitions.FirstOrDefaultAsync(f => f.Id == id);
            if (entity is null) return Results.NotFound();

            request.ApplyTo(entity);
            await db.SaveChangesAsync();
            return Results.Ok(entity.ToDto());
        });

        group.MapDelete("/{id:int}", async (int id, IDbContextFactory<ApplicationDbContext> dbFactory) =>
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.ForecastDefinitions.FirstOrDefaultAsync(f => f.Id == id);
            if (entity is null) return Results.NotFound();

            entity.DeletedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }
}
