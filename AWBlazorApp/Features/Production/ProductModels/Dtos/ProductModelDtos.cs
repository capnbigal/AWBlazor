using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ProductModels.Dtos;

public sealed record ProductModelDto(int Id, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateProductModelRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateProductModelRequest
{
    public string? Name { get; set; }
}

public sealed record ProductModelAuditLogDto(
    int Id, int ProductModelId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, Guid RowGuid, DateTime SourceModifiedDate);

public static class ProductModelMappings
{
    public static ProductModelDto ToDto(this ProductModel e) => new(e.Id, e.Name, e.RowGuid, e.ModifiedDate);

    public static ProductModel ToEntity(this CreateProductModelRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductModelRequest r, ProductModel e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductModelAuditLogDto ToDto(this ProductModelAuditLog a) => new(
        a.Id, a.ProductModelId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.RowGuid, a.SourceModifiedDate);
}
