using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.Cultures.Dtos;

public sealed record CultureDto(string CultureId, string Name, DateTime ModifiedDate);

public sealed record CreateCultureRequest
{
    public string? CultureId { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateCultureRequest
{
    public string? Name { get; set; }
}

public sealed record CultureAuditLogDto(
    int Id, string CultureId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class CultureMappings
{
    public static CultureDto ToDto(this Culture e) => new(e.CultureId, e.Name, e.ModifiedDate);

    public static Culture ToEntity(this CreateCultureRequest r) => new()
    {
        CultureId = (r.CultureId ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateCultureRequest r, Culture e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CultureAuditLogDto ToDto(this CultureAuditLog a) => new(
        a.Id, a.CultureId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
