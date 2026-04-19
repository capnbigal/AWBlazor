using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Features.Performance.Dtos;
using AWBlazorApp.Features.Performance.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Performance.Api;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        var oee = app.MapGroup("/api/oee-snapshots")
            .WithTags("OeeSnapshots")
            .RequireAuthorization("ApiOrCookie");

        oee.MapGet("/", ListOeeAsync).WithName("ListOeeSnapshots");
        oee.MapPost("/compute", ComputeOeeAsync).WithName("ComputeOeeSnapshot")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        var prod = app.MapGroup("/api/production-metrics")
            .WithTags("ProductionMetrics")
            .RequireAuthorization("ApiOrCookie");

        prod.MapGet("/", ListProductionAsync).WithName("ListProductionDailyMetrics");
        prod.MapPost("/compute", ComputeProductionAsync).WithName("ComputeProductionDailyMetric")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        var maint = app.MapGroup("/api/maintenance-metrics")
            .WithTags("MaintenanceMetrics")
            .RequireAuthorization("ApiOrCookie");

        maint.MapGet("/", ListMaintAsync).WithName("ListMaintenanceMonthlyMetrics");
        maint.MapPost("/compute", ComputeMaintAsync).WithName("ComputeMaintenanceMonthlyMetric")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<OeeSnapshotDto>>> ListOeeAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? stationId = null,
        [FromQuery] PerformancePeriodKind? periodKind = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.OeeSnapshots.AsNoTracking();
        if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
        if (periodKind.HasValue) q = q.Where(x => x.PeriodKind == periodKind.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.PeriodStart)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<OeeSnapshotDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<OeeSnapshotDto>, ValidationProblem>> ComputeOeeAsync(
        ComputeOeeRequest request,
        IValidator<ComputeOeeRequest> validator,
        IOeeService svc, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var snap = await svc.ComputeAsync(
            request.StationId, request.PeriodKind,
            request.PeriodStart, request.PeriodEnd,
            request.IdealCycleSeconds, ct);
        return TypedResults.Ok(snap.ToDto());
    }

    private static async Task<Ok<PagedResult<ProductionDailyMetricDto>>> ListProductionAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? stationId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.ProductionDailyMetrics.AsNoTracking();
        if (stationId.HasValue) q = q.Where(x => x.StationId == stationId.Value);
        if (from.HasValue) q = q.Where(x => x.Date >= from.Value);
        if (to.HasValue) q = q.Where(x => x.Date <= to.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.Date)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductionDailyMetricDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductionDailyMetricDto>, ValidationProblem>> ComputeProductionAsync(
        ComputeProductionMetricRequest request,
        IValidator<ComputeProductionMetricRequest> validator,
        IProductionMetricsService svc, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var metric = await svc.ComputeDailyAsync(request.StationId, request.Date, ct);
        return TypedResults.Ok(metric.ToDto());
    }

    private static async Task<Ok<PagedResult<MaintenanceMonthlyMetricDto>>> ListMaintAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? assetId = null,
        [FromQuery] int? year = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.MaintenanceMonthlyMetrics.AsNoTracking();
        if (assetId.HasValue) q = q.Where(x => x.AssetId == assetId.Value);
        if (year.HasValue) q = q.Where(x => x.Year == year.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<MaintenanceMonthlyMetricDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<MaintenanceMonthlyMetricDto>, ValidationProblem>> ComputeMaintAsync(
        ComputeMaintenanceMetricRequest request,
        IValidator<ComputeMaintenanceMetricRequest> validator,
        IMaintenanceMetricsService svc, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var metric = await svc.ComputeMonthlyAsync(request.AssetId, request.Year, request.Month, ct);
        return TypedResults.Ok(metric.ToDto());
    }
}
