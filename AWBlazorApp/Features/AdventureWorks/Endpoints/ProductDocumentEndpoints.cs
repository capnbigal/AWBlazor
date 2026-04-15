using System.Security.Claims;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using AWBlazorApp.Features.AdventureWorks.Models;
using AWBlazorApp.Features.AdventureWorks.Audit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Endpoints;

public static class ProductDocumentEndpoints
{
    public static IEndpointRouteBuilder MapProductDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/product-documents")
            .WithTags("ProductDocuments")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListProductDocuments")
            .WithSummary("List Production.ProductDocument rows. Composite PK = (ProductID, DocumentNode hierarchyid).");
        group.MapGet("/by-key", GetAsync).WithName("GetProductDocument");
        group.MapPost("/", CreateAsync).WithName("CreateProductDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateProductDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteProductDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListProductDocumentHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<ProductDocumentDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] int? productId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.ProductDocuments.AsNoTracking();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.ProductId).ThenBy(x => x.DocumentNode)
            .Skip(skip).Take(take)
            .Select(x => new ProductDocumentDto(x.ProductId, x.DocumentNode.ToString()!, x.ModifiedDate))
            .ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<ProductDocumentDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<ProductDocumentDto>, NotFound>> GetAsync(
        [FromQuery] int productId, [FromQuery] string documentNode,
        ApplicationDbContext db, CancellationToken ct)
    {
        var node = HierarchyId.Parse(documentNode);
        var row = await db.ProductDocuments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.DocumentNode == node, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<CompositeKeyResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateProductDocumentRequest request, IValidator<CreateProductDocumentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var node = HierarchyId.Parse(request.DocumentNode!);
        if (await db.ProductDocuments.AnyAsync(x =>
                x.ProductId == request.ProductId && x.DocumentNode == node, ct))
        {
            return TypedResults.Conflict($"Junction row ({request.ProductId}, {request.DocumentNode}) already exists.");
        }

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.ProductDocuments.Add(entity);
        await db.SaveChangesAsync(ct);
        db.ProductDocumentAuditLogs.Add(ProductDocumentAuditService.RecordCreate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/product-documents/by-key?productId={entity.ProductId}&documentNode={Uri.EscapeDataString(entity.DocumentNode.ToString())}",
            new CompositeKeyResponse(new Dictionary<string, object>
            {
                ["productId"] = entity.ProductId,
                ["documentNode"] = entity.DocumentNode.ToString(),
            }));
    }

    private static async Task<Results<Ok<CompositeKeyResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] int productId, [FromQuery] string documentNode,
        UpdateProductDocumentRequest request, IValidator<UpdateProductDocumentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var node = HierarchyId.Parse(documentNode);
        var entity = await db.ProductDocuments
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.DocumentNode == node, ct);
        if (entity is null) return TypedResults.NotFound();

        request.ApplyTo(entity);
        db.ProductDocumentAuditLogs.Add(ProductDocumentAuditService.RecordUpdate(entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new CompositeKeyResponse(new Dictionary<string, object>
        {
            ["productId"] = entity.ProductId,
            ["documentNode"] = entity.DocumentNode.ToString(),
        }));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] int productId, [FromQuery] string documentNode,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var node = HierarchyId.Parse(documentNode);
        var entity = await db.ProductDocuments
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.DocumentNode == node, ct);
        if (entity is null) return TypedResults.NotFound();

        db.ProductDocumentAuditLogs.Add(ProductDocumentAuditService.RecordDelete(entity, user.Identity?.Name));
        db.ProductDocuments.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<ProductDocumentAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] int? productId = null,
        [FromQuery] string? documentNode = null,
        CancellationToken ct = default)
    {
        var query = db.ProductDocumentAuditLogs.AsNoTracking();
        if (productId.HasValue) query = query.Where(a => a.ProductId == productId.Value);
        if (!string.IsNullOrWhiteSpace(documentNode)) query = query.Where(a => a.DocumentNode == documentNode);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
