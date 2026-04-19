using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Application.Services;

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
