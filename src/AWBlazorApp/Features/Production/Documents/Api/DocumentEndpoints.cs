using System.Security.Claims;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Dtos;
using AWBlazorApp.Features.Production.Cultures.Dtos; using AWBlazorApp.Features.Production.Documents.Dtos; using AWBlazorApp.Features.Production.Illustrations.Dtos; using AWBlazorApp.Features.Production.Locations.Dtos; using AWBlazorApp.Features.Production.ProductCategories.Dtos; using AWBlazorApp.Features.Production.ProductCostHistories.Dtos; using AWBlazorApp.Features.Production.ProductDescriptions.Dtos; using AWBlazorApp.Features.Production.ProductDocuments.Dtos; using AWBlazorApp.Features.Production.ProductInventories.Dtos; using AWBlazorApp.Features.Production.ProductListPriceHistories.Dtos; using AWBlazorApp.Features.Production.ProductModels.Dtos; using AWBlazorApp.Features.Production.ProductModelIllustrations.Dtos; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos; using AWBlazorApp.Features.Production.ProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductProductPhotos.Dtos; using AWBlazorApp.Features.Production.ProductReviews.Dtos; using AWBlazorApp.Features.Production.Products.Dtos; using AWBlazorApp.Features.Production.ProductSubcategories.Dtos; using AWBlazorApp.Features.Production.ScrapReasons.Dtos; using AWBlazorApp.Features.Production.TransactionHistories.Dtos; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Dtos; using AWBlazorApp.Features.Production.UnitMeasures.Dtos; using AWBlazorApp.Features.Production.WorkOrders.Dtos; using AWBlazorApp.Features.Production.WorkOrderRoutings.Dtos; 
using AWBlazorApp.Features.Production.Cultures.Application.Services; using AWBlazorApp.Features.Production.Documents.Application.Services; using AWBlazorApp.Features.Production.Illustrations.Application.Services; using AWBlazorApp.Features.Production.Locations.Application.Services; using AWBlazorApp.Features.Production.ProductCategories.Application.Services; using AWBlazorApp.Features.Production.ProductCostHistories.Application.Services; using AWBlazorApp.Features.Production.ProductDescriptions.Application.Services; using AWBlazorApp.Features.Production.ProductDocuments.Application.Services; using AWBlazorApp.Features.Production.ProductInventories.Application.Services; using AWBlazorApp.Features.Production.ProductListPriceHistories.Application.Services; using AWBlazorApp.Features.Production.ProductModels.Application.Services; using AWBlazorApp.Features.Production.ProductModelIllustrations.Application.Services; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Application.Services; using AWBlazorApp.Features.Production.ProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductProductPhotos.Application.Services; using AWBlazorApp.Features.Production.ProductReviews.Application.Services; using AWBlazorApp.Features.Production.Products.Application.Services; using AWBlazorApp.Features.Production.ProductSubcategories.Application.Services; using AWBlazorApp.Features.Production.ScrapReasons.Application.Services; using AWBlazorApp.Features.Production.TransactionHistories.Application.Services; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Application.Services; using AWBlazorApp.Features.Production.UnitMeasures.Application.Services; using AWBlazorApp.Features.Production.WorkOrders.Application.Services; using AWBlazorApp.Features.Production.WorkOrderRoutings.Application.Services; 
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Documents.Api;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/aw/documents")
            .WithTags("Documents")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/", ListAsync).WithName("ListDocuments")
            .WithSummary("List Production.Document rows. PK is a hierarchyid (DocumentNode).");
        group.MapGet("/by-key", GetAsync).WithName("GetDocument");
        group.MapPost("/", CreateAsync).WithName("CreateDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapPatch("/by-key", UpdateAsync).WithName("UpdateDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin));
        group.MapDelete("/by-key", DeleteAsync).WithName("DeleteDocument")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Manager, AppRoles.Admin));
        group.MapGet("/history", HistoryAsync).WithName("ListDocumentHistory");
        return app;
    }

    private static async Task<Ok<PagedResult<DocumentDto>>> ListAsync(
        ApplicationDbContext db, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? title = null, [FromQuery] int? owner = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var query = db.Documents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(title)) query = query.Where(x => x.Title.Contains(title));
        if (owner.HasValue) query = query.Where(x => x.Owner == owner.Value);
        var total = await query.CountAsync(ct);
        // Don't pull document bytes back — project to DTO inside the SQL projection.
        var rows = await query.OrderBy(x => x.DocumentNode)
            .Skip(skip).Take(take)
            .Select(x => new DocumentDto(
                x.DocumentNode.ToString()!, x.DocumentLevel, x.Title, x.Owner, x.FolderFlag,
                x.FileName, x.FileExtension, x.Revision, x.ChangeNumber, x.Status,
                x.DocumentSummary, x.DocumentContent != null && x.DocumentContent.Length > 0,
                x.RowGuid, x.ModifiedDate))
            .ToListAsync(ct);
        return TypedResults.Ok(new PagedResult<DocumentDto>(rows, total, skip, take));
    }

    private static async Task<Results<Ok<DocumentDto>, NotFound>> GetAsync(
        [FromQuery] string documentNode, ApplicationDbContext db, CancellationToken ct)
    {
        var node = HierarchyId.Parse(documentNode);
        var row = await db.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.DocumentNode == node, ct);
        return row is null ? TypedResults.NotFound() : TypedResults.Ok(row.ToDto());
    }

    private static async Task<Results<Created<StringIdResponse>, Conflict<string>, ValidationProblem>> CreateAsync(
        CreateDocumentRequest request, IValidator<CreateDocumentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var node = HierarchyId.Parse(request.DocumentNode!);
        if (await db.Documents.AnyAsync(x => x.DocumentNode == node, ct))
            return TypedResults.Conflict($"Document with DocumentNode '{request.DocumentNode}' already exists.");

        var entity = request.ToEntity();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Documents.Add(entity);
        await db.SaveChangesAsync(ct);

        // Re-query so computed DocumentLevel is populated.
        var reloaded = await db.Documents.AsNoTracking().FirstAsync(x => x.DocumentNode == entity.DocumentNode, ct);
        db.DocumentAuditLogs.Add(DocumentAuditService.RecordCreate(reloaded, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return TypedResults.Created(
            $"/api/aw/documents/by-key?documentNode={Uri.EscapeDataString(entity.DocumentNode.ToString())}",
            new StringIdResponse(entity.DocumentNode.ToString()));
    }

    private static async Task<Results<Ok<StringIdResponse>, NotFound, ValidationProblem>> UpdateAsync(
        [FromQuery] string documentNode,
        UpdateDocumentRequest request, IValidator<UpdateDocumentRequest> validator,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request, ct);
        if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());

        var node = HierarchyId.Parse(documentNode);
        var entity = await db.Documents.FirstOrDefaultAsync(x => x.DocumentNode == node, ct);
        if (entity is null) return TypedResults.NotFound();

        var before = DocumentAuditService.CaptureSnapshot(entity);
        request.ApplyTo(entity);
        db.DocumentAuditLogs.Add(DocumentAuditService.RecordUpdate(before, entity, user.Identity?.Name));
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(new StringIdResponse(entity.DocumentNode.ToString()));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromQuery] string documentNode,
        ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct)
    {
        var node = HierarchyId.Parse(documentNode);
        var entity = await db.Documents.FirstOrDefaultAsync(x => x.DocumentNode == node, ct);
        if (entity is null) return TypedResults.NotFound();

        db.DocumentAuditLogs.Add(DocumentAuditService.RecordDelete(entity, user.Identity?.Name));
        db.Documents.Remove(entity);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<DocumentAuditLogDto>>> HistoryAsync(
        ApplicationDbContext db,
        [FromQuery] string? documentNode = null,
        CancellationToken ct = default)
    {
        var query = db.DocumentAuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(documentNode))
            query = query.Where(a => a.DocumentNode == documentNode);

        var rows = await query
            .OrderByDescending(a => a.ChangedDate).ThenByDescending(a => a.Id)
            .Select(a => a.ToDto())
            .ToListAsync(ct);
        return TypedResults.Ok(rows);
    }
}
