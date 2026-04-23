using AWBlazorApp.Features.Engineering.Dtos;
using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain;
using AWBlazorApp.Features.Engineering.Deviations.Application.Services;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Engineering.Api;

public static class DocumentDeviationEndpoints
{
    public static IEndpointRouteBuilder MapDocumentDeviationEndpoints(this IEndpointRouteBuilder app)
    {
        var docs = app.MapGroup("/api/engineering-documents")
            .WithTags("EngineeringDocuments")
            .RequireAuthorization("ApiOrCookie");

        docs.MapGet("/", ListDocsAsync).WithName("ListEngineeringDocuments");
        docs.MapGet("/{id:int}", GetDocAsync).WithName("GetEngineeringDocument");
        docs.MapPost("/", CreateDocAsync).WithName("CreateEngineeringDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        docs.MapPatch("/{id:int}", UpdateDocAsync).WithName("UpdateEngineeringDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        docs.MapDelete("/{id:int}", DeleteDocAsync).WithName("DeleteEngineeringDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));
        docs.MapGet("/{id:int}/history", DocHistoryAsync).WithName("ListEngineeringDocumentHistory");

        var dev = app.MapGroup("/api/deviation-requests")
            .WithTags("DeviationRequests")
            .RequireAuthorization("ApiOrCookie");

        dev.MapGet("/", ListDevAsync).WithName("ListDeviationRequests");
        dev.MapGet("/{id:int}", GetDevAsync).WithName("GetDeviationRequest");
        dev.MapPost("/", CreateDevAsync).WithName("CreateDeviationRequest");
        dev.MapPost("/{id:int}/approve", ApproveDevAsync).WithName("ApproveDeviationRequest")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        dev.MapPost("/{id:int}/reject", RejectDevAsync).WithName("RejectDeviationRequest")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        dev.MapPost("/{id:int}/cancel", CancelDevAsync).WithName("CancelDeviationRequest");
        dev.MapGet("/{id:int}/history", DevHistoryAsync).WithName("ListDeviationRequestHistory");

        return app;
    }

    private static async Task<Ok<PagedResult<EngineeringDocumentDto>>> ListDocsAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null,
        [FromQuery] EngineeringDocumentKind? kind = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.EngineeringDocuments.AsNoTracking();
        if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
        if (kind.HasValue) q = q.Where(x => x.Kind == kind.Value);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Code)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<EngineeringDocumentDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<EngineeringDocumentDto>, NotFound>> GetDocAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.EngineeringDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateDocAsync(
        CreateEngineeringDocumentRequest request,
        IValidator<CreateEngineeringDocumentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity();
        db.EngineeringDocuments.Add(entity);
        // AuditLogInterceptor writes the audit row inside this SaveChangesAsync.
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/engineering-documents/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<Ok<IdResponse>, NotFound, ValidationProblem>> UpdateDocAsync(
        int id, UpdateEngineeringDocumentRequest request,
        IValidator<UpdateEngineeringDocumentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = await db.EngineeringDocuments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        request.ApplyTo(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteDocAsync(
        int id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.EngineeringDocuments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return TypedResults.NotFound();
        db.EngineeringDocuments.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<AuditLog>>> DocHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "EngineeringDocument" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }

    private static async Task<Ok<PagedResult<DeviationRequestDto>>> ListDevAsync(
        ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null,
        [FromQuery] DeviationStatus? status = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var q = db.DeviationRequests.AsNoTracking();
        if (productId.HasValue) q = q.Where(x => x.ProductId == productId.Value);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.RaisedAt)
            .Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<DeviationRequestDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<DeviationRequestDto>, NotFound>> GetDevAsync(
        int id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.DeviationRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateDevAsync(
        CreateDeviationRequestRequest request,
        IValidator<CreateDeviationRequestRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
        var entity = request.ToEntity(user.Identity?.Name);
        db.DeviationRequests.Add(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/api/deviation-requests/{entity.Id}", new IdResponse(entity.Id));
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> ApproveDevAsync(
        int id, ReviewDeviationRequest request, IDeviationService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.ApproveAsync(id, request.Notes, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> RejectDevAsync(
        int id, ReviewDeviationRequest request, IDeviationService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.RejectAsync(id, request.Notes, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> CancelDevAsync(
        int id, IDeviationService svc, ClaimsPrincipal user, CancellationToken ct)
        => await TransitionAsync(() => svc.CancelAsync(id, user.Identity?.Name, ct));

    private static async Task<Results<NoContent, NotFound, BadRequest<string>>> TransitionAsync(Func<Task> action)
    {
        try
        {
            await action();
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException) { return TypedResults.NotFound(); }
        catch (InvalidOperationException ex) { return TypedResults.BadRequest(ex.Message); }
    }

    private static async Task<Ok<PagedResult<AuditLog>>> DevHistoryAsync(
        int id, ApplicationDbContext db,
        [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        var idStr = id.ToString();
        var q = db.AuditLogs.AsNoTracking().Where(a => a.EntityType == "DeviationRequest" && a.EntityId == idStr);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<AuditLog>(rows, total, skip, take));
    }
}
