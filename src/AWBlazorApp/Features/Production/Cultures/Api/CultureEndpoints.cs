using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Dtos; using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using AWBlazorApp.Features.Production.Audit; using AWBlazorApp.Features.Production.Cultures.Application.Services; using AWBlazorApp.Features.Production.Documents.Application.Services; using AWBlazorApp.Features.Production.Illustrations.Application.Services; using AWBlazorApp.Features.Production.Locations.Application.Services; using AWBlazorApp.Features.Production.ProductCategories.Application.Services; using AWBlazorApp.Features.Production.ProductCostHistories.Application.Services; using AWBlazorApp.Features.Production.ProductDescriptions.Application.Services; using AWBlazorApp.Features.Production.ProductDocuments.Application.Services; using AWBlazorApp.Features.Production.ProductInventories.Application.Services; using AWBlazorApp.Features.Production.ProductListPriceHistories.Application.Services; using AWBlazorApp.Features.Production.ProductModels.Application.Services; using AWBlazorApp.Features.Production.ProductModelIllustrations.Application.Services; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Application.Services; using AWBlazorApp.Features.Production.ProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductReviews.Application.Services; using AWBlazorApp.Features.Production.Products.Application.Services; using AWBlazorApp.Features.Production.ProductSubcategories.Application.Services; using AWBlazorApp.Features.Production.ScrapReasons.Application.Services; using AWBlazorApp.Features.Production.TransactionHistories.Application.Services; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Application.Services; using AWBlazorApp.Features.Production.UnitMeasures.Application.Services; using AWBlazorApp.Features.Production.WorkOrders.Application.Services; using AWBlazorApp.Features.Production.WorkOrderRoutings.Application.Services; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Cultures.Api;

public static class CultureEndpoints
{
    public static IEndpointRouteBuilder MapCultureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/cultures")
            .WithTags("Cultures")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListCultures").WithSummary("List Production.Culture rows.");
        group.MapGet("/{id}", GetAsync).WithName("GetCulture");
        group.MapPost("/", CreateAsync).WithName("CreateCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/{id}", UpdateAsync).WithName("UpdateCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/{id}", DeleteAsync).WithName("DeleteCulture")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/{id}/history", HistoryAsync).WithName("ListCultureHistory");
        return app;
    }

    private static async Task<Ok<AWBlazorApp.Shared.Dtos.PagedResult<CultureDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? name = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Cultures.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(x => x.Name.Contains(name));
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.CultureId).Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync(ct);
        return TypedResults.Ok(new AWBlazorApp.Shared.Dtos.PagedResult<CultureDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<CultureDto>, NotFound>> GetAsync(string id, ApplicationDbContext db, CancellationToken ct)
    {
        var row = await db.Cultures.AsNoTracking().FirstOrDefaultAsync(x => x.CultureId == id, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<StringIdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateCultureRequest request, IValidator<CreateCultureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var id = (request.CultureId ?? string.Empty).Trim();
        if (await db.Cultures.AnyAsync(x => x.CultureId == id, ct))
            return TypedResults.Conflict($"Culture ID '{id}' already exists.");

        var entity = request.ToEntity();
        await db.AddWithAuditAsync(entity, e => CultureAuditService.RecordCreate(e, user.Identity?.Name), ct);
        return TypedResults.Created($"/api/aw/cultures/{entity.CultureId}", new StringIdResponse(entity.CultureId));
    }

    private static async Task<Results<Ok<StringIdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        string id, UpdateCultureRequest request, IValidator<UpdateCultureRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var entity = await db.Cultures.FirstOrDefaultAsync(x => x.CultureId == id, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = CultureAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.CultureAuditLogs.Add(CultureAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new StringIdResponse(entity.CultureId));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        string id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var entity = await db.Cultures.FirstOrDefaultAsync(x => x.CultureId == id, ct);
        if (entity is null) return TypedResults.NotFound();

        db.CultureAuditLogs.Add(CultureAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Cultures.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<CultureAuditLogDto>>> HistoryAsync(string id, ApplicationDbContext db, CancellationToken ct)
    {
        var rows = await db.CultureAuditLogs.AsNoTracking()
            .Where(a => a.CultureId == id)
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
