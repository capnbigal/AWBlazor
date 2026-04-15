using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ProductModelIllustrationAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static ProductModelIllustrationAuditLog RecordCreate(ProductModelIllustration e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductModelIllustrationAuditLog RecordUpdate(ProductModelIllustration e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static ProductModelIllustrationAuditLog RecordDelete(ProductModelIllustration e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductModelIllustrationAuditLog BuildLog(
        ProductModelIllustration e, string action, string? by, string? summary)
        => new()
        {
            ProductModelId = e.ProductModelId,
            IllustrationId = e.IllustrationId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
