using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 

namespace AWBlazorApp.Features.Production.Locations.Dtos;

public sealed record LocationDto(short Id, string Name, decimal CostRate, decimal Availability, DateTime ModifiedDate);

public sealed record CreateLocationRequest
{
    public string? Name { get; set; }
    public decimal CostRate { get; set; }
    public decimal Availability { get; set; }
}

public sealed record UpdateLocationRequest
{
    public string? Name { get; set; }
    public decimal? CostRate { get; set; }
    public decimal? Availability { get; set; }
}

public sealed record LocationAuditLogDto(
    int Id, short LocationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, decimal CostRate, decimal Availability, DateTime SourceModifiedDate);

public static class LocationMappings
{
    public static LocationDto ToDto(this Location e) => new(e.Id, e.Name, e.CostRate, e.Availability, e.ModifiedDate);

    public static Location ToEntity(this CreateLocationRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        CostRate = r.CostRate,
        Availability = r.Availability,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateLocationRequest r, Location e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.CostRate.HasValue) e.CostRate = r.CostRate.Value;
        if (r.Availability.HasValue) e.Availability = r.Availability.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static LocationAuditLogDto ToDto(this LocationAuditLog a) => new(
        a.Id, a.LocationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.CostRate, a.Availability, a.SourceModifiedDate);
}
