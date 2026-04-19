using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Audit;

public static class ProductDocumentAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static ProductDocumentAuditLog RecordCreate(ProductDocument e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductDocumentAuditLog RecordUpdate(ProductDocument e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static ProductDocumentAuditLog RecordDelete(ProductDocument e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductDocumentAuditLog BuildLog(
        ProductDocument e, string action, string? by, string? summary)
        => new()
        {
            ProductId = e.ProductId,
            DocumentNode = e.DocumentNode.ToString(),
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
