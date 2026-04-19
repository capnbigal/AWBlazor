using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Features.Maintenance.AssetProfiles.Dtos; using AWBlazorApp.Features.Maintenance.PmSchedules.Dtos; using AWBlazorApp.Features.Maintenance.SpareParts.Dtos; using AWBlazorApp.Features.Maintenance.WorkOrders.Dtos; 
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Maintenance.AssetProfiles.Api;

public static class AssetMaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapAssetMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        var profiles = app.MapGroup("/api/asset-maintenance-profiles")
            .WithTags("AssetMaintenanceProfiles")
            .RequireAuthorization("ApiOrCookie");

        profiles.MapGet("/", ListProfilesAsync).WithName("ListAssetMaintenanceProfiles");
        profiles.MapGet("/{id:int}", GetProfileAsync).WithName("GetAssetMaintenanceProfile");
        profiles.MapPost("/", CreateProfileAsync).WithName("CreateAssetMaintenanceProfile")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        profiles.MapPatch("/{id:int}", UpdateProfileAsync).WithName("UpdateAssetMaintenanceProfile")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        profiles.MapDelete("/{id:int}", DeleteProfileAsync).WithName("DeleteAssetMaintenanceProfile")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        profiles.MapGet("/{id:int}/history", ProfileHistoryAsync).WithName("ListAssetMaintenanceProfileHistory");

        var meters = app.MapGroup("/api/meter-readings")
            .WithTags("MeterReadings")
            .RequireAuthorization("ApiOrCookie");

        meters.MapGet("/", ListMetersAsync).WithName("ListMeterReadings");
        meters.MapPost("/", CreateMeterAsync).WithName("CreateMeterReading");

        var logs = app.MapGroup("/api/maintenance-logs")
            .WithTags("MaintenanceLogs")
            .RequireAuthorization("ApiOrCookie");

        logs.MapGet("/", ListLogsAsync).WithName("ListMaintenanceLogs");
        logs.MapPost("/", CreateLogAsync).WithName("CreateMaintenanceLog");
        logs.MapDelete("/{id:long}", DeleteLogAsync).WithName("DeleteMaintenanceLog")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));

        return app;
    }

    private static async Task<Ok<PagedResult<AssetMaintenanceProfileDto>>> ListProfilesAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] AssetCriticality? criticality = null,
        [FromQuery] int? assetId = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.AssetMaintenanceProfiles.AsNoTracking();
        if (criticality.HasValue) q = q.Where(x => x.Criticality == criticality.Value);
        if (assetId.HasValue) q = q.Where(x => x.AssetId == assetId.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.AssetId)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AssetMaintenanceProfileDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<AssetMaintenanceProfileDto>, NotFound>> GetProfileAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.AssetMaintenanceProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, Conflict<string>, ValidationProblem>> CreateProfileAsync(
        CreateAssetMaintenanceProfileRequest request,
        IValidator<CreateAssetMaintenanceProfileRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var exists = await db.AssetMaintenanceProfiles.AnyAsync(p => p.AssetId == request.AssetId, ct);
        if (exists) return TypedResults.Conflict("This asset already has a maintenance profile.");
        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => AssetMaintenanceProfileAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/asset-maintenance-profiles/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateProfileAsync(
        int id, UpdateAssetMaintenanceProfileRequest request,
        IValidator<UpdateAssetMaintenanceProfileRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.AssetMaintenanceProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        var before = AssetMaintenanceProfileAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.AssetMaintenanceProfileAuditLogs.Add(AssetMaintenanceProfileAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteProfileAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.AssetMaintenanceProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        await db.DeleteWithAuditAsync(entity, AssetMaintenanceProfileAuditService.RecordDelete(entity, user.Identity?.Name), ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AssetMaintenanceProfileAuditLogDto>>> ProfileHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.AssetMaintenanceProfileAuditLogs.AsNoTracking().Where(a => a.AssetMaintenanceProfileId == id);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).Select(a => a.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AssetMaintenanceProfileAuditLogDto>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<MeterReadingDto>>> ListMetersAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? assetId = null,
        [FromQuery] MeterKind? kind = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.MeterReadings.AsNoTracking();
        if (assetId.HasValue) q = q.Where(x => x.AssetId == assetId.Value);
        if (kind.HasValue) q = q.Where(x => x.Kind == kind.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.RecordedAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<MeterReadingDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateMeterAsync(
        CreateMeterReadingRequest request,
        IValidator<CreateMeterReadingRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        db.MeterReadings.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/meter-readings/{entity.Id}", new IdResponse((int)entity.Id));
    }

    private static async Task<Ok<PagedResult<MaintenanceLogDto>>> ListLogsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? assetId = null,
        [FromQuery] MaintenanceLogKind? kind = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.MaintenanceLogs.AsNoTracking();
        if (assetId.HasValue) q = q.Where(x => x.AssetId == assetId.Value);
        if (kind.HasValue) q = q.Where(x => x.Kind == kind.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.AuthoredAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<MaintenanceLogDto>(rows, total, skip, take));
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateLogAsync(
        CreateMaintenanceLogRequest request,
        IValidator<CreateMaintenanceLogRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        db.MaintenanceLogs.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/maintenance-logs/{entity.Id}", new IdResponse((int)entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteLogAsync(
        long id, ApplicationDbContext db, CancellationToken ct)
    {
        var entity = await db.MaintenanceLogs.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.MaintenanceLogs.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
