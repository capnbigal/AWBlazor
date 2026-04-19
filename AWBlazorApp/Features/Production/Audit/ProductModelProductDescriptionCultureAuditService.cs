using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Audit;

public static class ProductModelProductDescriptionCultureAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static ProductModelProductDescriptionCultureAuditLog RecordCreate(ProductModelProductDescriptionCulture e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductModelProductDescriptionCultureAuditLog RecordUpdate(ProductModelProductDescriptionCulture e, string? by)
        => BuildLog(e, ActionUpdated, by, "Touched");

    public static ProductModelProductDescriptionCultureAuditLog RecordDelete(ProductModelProductDescriptionCulture e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductModelProductDescriptionCultureAuditLog BuildLog(
        ProductModelProductDescriptionCulture e, string action, string? by, string? summary)
        => new()
        {
            ProductModelId = e.ProductModelId,
            ProductDescriptionId = e.ProductDescriptionId,
            CultureId = e.CultureId,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            SourceModifiedDate = e.ModifiedDate,
        };
}
