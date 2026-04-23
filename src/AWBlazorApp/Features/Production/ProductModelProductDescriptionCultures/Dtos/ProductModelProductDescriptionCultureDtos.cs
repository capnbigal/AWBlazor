using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Dtos;

public sealed record ProductModelProductDescriptionCultureDto(
    int ProductModelId, int ProductDescriptionId, string CultureId, DateTime ModifiedDate);

public sealed record CreateProductModelProductDescriptionCultureRequest
{
    public int ProductModelId { get; set; }
    public int ProductDescriptionId { get; set; }
    public string CultureId { get; set; } = string.Empty;
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateProductModelProductDescriptionCultureRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public static class ProductModelProductDescriptionCultureMappings
{
    public static ProductModelProductDescriptionCultureDto ToDto(this ProductModelProductDescriptionCulture e) => new(
        e.ProductModelId, e.ProductDescriptionId, e.CultureId, e.ModifiedDate);

    public static ProductModelProductDescriptionCulture ToEntity(this CreateProductModelProductDescriptionCultureRequest r) => new()
    {
        ProductModelId = r.ProductModelId,
        ProductDescriptionId = r.ProductDescriptionId,
        CultureId = r.CultureId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductModelProductDescriptionCultureRequest _, ProductModelProductDescriptionCulture e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
