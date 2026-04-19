using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductProductPhotos.Dtos;

public sealed record ProductProductPhotoDto(
    int ProductId, int ProductPhotoId, bool Primary, DateTime ModifiedDate);

public sealed record CreateProductProductPhotoRequest
{
    public int ProductId { get; set; }
    public int ProductPhotoId { get; set; }
    public bool Primary { get; set; }
}

public sealed record UpdateProductProductPhotoRequest
{
    public bool? Primary { get; set; }
}

public sealed record ProductProductPhotoAuditLogDto(
    int Id, int ProductId, int ProductPhotoId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, bool Primary, DateTime SourceModifiedDate);

public static class ProductProductPhotoMappings
{
    public static ProductProductPhotoDto ToDto(this ProductProductPhoto e) => new(
        e.ProductId, e.ProductPhotoId, e.Primary, e.ModifiedDate);

    public static ProductProductPhoto ToEntity(this CreateProductProductPhotoRequest r) => new()
    {
        ProductId = r.ProductId,
        ProductPhotoId = r.ProductPhotoId,
        Primary = r.Primary,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductProductPhotoRequest r, ProductProductPhoto e)
    {
        if (r.Primary.HasValue) e.Primary = r.Primary.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductProductPhotoAuditLogDto ToDto(this ProductProductPhotoAuditLog a) => new(
        a.Id, a.ProductId, a.ProductPhotoId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Primary, a.SourceModifiedDate);
}
