using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.ScrapReasons.Dtos;

public sealed record ScrapReasonDto(short Id, string Name, DateTime ModifiedDate);

public sealed record CreateScrapReasonRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateScrapReasonRequest
{
    public string? Name { get; set; }
}

public sealed record ScrapReasonAuditLogDto(
    int Id, short ScrapReasonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class ScrapReasonMappings
{
    public static ScrapReasonDto ToDto(this ScrapReason e) => new(e.Id, e.Name, e.ModifiedDate);

    public static ScrapReason ToEntity(this CreateScrapReasonRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateScrapReasonRequest r, ScrapReason e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ScrapReasonAuditLogDto ToDto(this ScrapReasonAuditLog a) => new(
        a.Id, a.ScrapReasonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
