using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

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
