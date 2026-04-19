using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductPhotos.Application.Services;

public static class ProductPhotoAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ProductPhoto e) => new(e);

    public static ProductPhotoAuditLog RecordCreate(ProductPhoto e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ProductPhotoAuditLog RecordUpdate(Snapshot before, ProductPhoto after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ProductPhotoAuditLog RecordDelete(ProductPhoto e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ProductPhotoAuditLog BuildLog(ProductPhoto e, string action, string? by, string? summary)
        => new()
        {
            ProductPhotoId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ThumbnailPhotoFileName = e.ThumbnailPhotoFileName,
            LargePhotoFileName = e.LargePhotoFileName,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ProductPhoto after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ThumbnailPhotoFileName", before.ThumbnailPhotoFileName, after.ThumbnailPhotoFileName);
        AuditDiffHelpers.AppendIfChanged(sb, "LargePhotoFileName", before.LargePhotoFileName, after.LargePhotoFileName);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string? ThumbnailPhotoFileName, string? LargePhotoFileName)
    {
        public Snapshot(ProductPhoto e) : this(e.ThumbnailPhotoFileName, e.LargePhotoFileName) { }
    }
}
