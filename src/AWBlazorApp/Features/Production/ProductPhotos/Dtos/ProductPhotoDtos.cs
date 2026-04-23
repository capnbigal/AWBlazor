using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductPhotos.Dtos;

public sealed record ProductPhotoDto(
    int Id, string? ThumbnailPhotoFileName, string? LargePhotoFileName,
    bool HasThumbnail, bool HasLargePhoto, DateTime ModifiedDate);

public sealed record CreateProductPhotoRequest
{
    public string? ThumbnailPhotoFileName { get; set; }
    public string? LargePhotoFileName { get; set; }
}

public sealed record UpdateProductPhotoRequest
{
    public string? ThumbnailPhotoFileName { get; set; }
    public string? LargePhotoFileName { get; set; }
}

public static class ProductPhotoMappings
{
    public static ProductPhotoDto ToDto(this ProductPhoto e) => new(
        e.Id, e.ThumbnailPhotoFileName, e.LargePhotoFileName,
        e.ThumbNailPhoto is { Length: > 0 }, e.LargePhoto is { Length: > 0 }, e.ModifiedDate);

    public static ProductPhoto ToEntity(this CreateProductPhotoRequest r) => new()
    {
        ThumbnailPhotoFileName = TrimToNull(r.ThumbnailPhotoFileName),
        LargePhotoFileName = TrimToNull(r.LargePhotoFileName),
        // Image bytes are not editable through this CRUD UI; they stay NULL on insert.
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductPhotoRequest r, ProductPhoto e)
    {
        if (r.ThumbnailPhotoFileName is not null) e.ThumbnailPhotoFileName = TrimToNull(r.ThumbnailPhotoFileName);
        if (r.LargePhotoFileName is not null) e.LargePhotoFileName = TrimToNull(r.LargePhotoFileName);
        e.ModifiedDate = DateTime.UtcNow;
    }

    private static string? TrimToNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
