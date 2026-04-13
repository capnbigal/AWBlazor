using AWBlazorApp.Data.Entities.AdventureWorks;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record ProductDocumentDto(int ProductId, string DocumentNode, DateTime ModifiedDate);

public sealed record CreateProductDocumentRequest
{
    public int ProductId { get; set; }
    /// <summary>Hierarchyid path, e.g. "/1/", "/1/2/".</summary>
    public string? DocumentNode { get; set; }
}

/// <summary>Pure junction — no non-key columns to update beyond ModifiedDate.</summary>
public sealed record UpdateProductDocumentRequest;

public sealed record ProductDocumentAuditLogDto(
    int Id, int ProductId, string DocumentNode, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

public static class ProductDocumentMappings
{
    public static ProductDocumentDto ToDto(this ProductDocument e) => new(
        e.ProductId, e.DocumentNode.ToString(), e.ModifiedDate);

    public static ProductDocument ToEntity(this CreateProductDocumentRequest r) => new()
    {
        ProductId = r.ProductId,
        DocumentNode = HierarchyId.Parse(r.DocumentNode ?? "/"),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductDocumentRequest _, ProductDocument e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductDocumentAuditLogDto ToDto(this ProductDocumentAuditLog a) => new(
        a.Id, a.ProductId, a.DocumentNode, a.Action, a.ChangedBy, a.ChangedDate,
        a.ChangeSummary, a.SourceModifiedDate);
}
