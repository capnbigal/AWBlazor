using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.Illustrations.Dtos;

public sealed record IllustrationDto(int Id, DateTime ModifiedDate);

/// <summary>
/// Illustration has no editable data of its own — its real Diagram XML column is intentionally
/// not mapped. Creating one allocates a new id and stamps ModifiedDate; the Diagram column
/// stays NULL on insert from this app.
/// </summary>
public sealed record CreateIllustrationRequest;

public sealed record UpdateIllustrationRequest;

public sealed record IllustrationAuditLogDto(
    int Id, int IllustrationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

public static class IllustrationMappings
{
    public static IllustrationDto ToDto(this Illustration e) => new(e.Id, e.ModifiedDate);

    public static Illustration ToEntity(this CreateIllustrationRequest _) => new()
    {
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateIllustrationRequest _, Illustration e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static IllustrationAuditLogDto ToDto(this IllustrationAuditLog a) => new(
        a.Id, a.IllustrationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.SourceModifiedDate);
}
