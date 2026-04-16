using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Models;

public sealed record ProductSubcategoryDto(int Id, int ProductCategoryId, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateProductSubcategoryRequest
{
    public int ProductCategoryId { get; set; }
    public string? Name { get; set; }
}

public sealed record UpdateProductSubcategoryRequest
{
    public int? ProductCategoryId { get; set; }
    public string? Name { get; set; }
}

public sealed record ProductSubcategoryAuditLogDto(
    int Id, int ProductSubcategoryId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int ProductCategoryId, string? Name, Guid RowGuid, DateTime SourceModifiedDate);

public static class ProductSubcategoryMappings
{
    public static ProductSubcategoryDto ToDto(this ProductSubcategory e)
        => new(e.Id, e.ProductCategoryId, e.Name, e.RowGuid, e.ModifiedDate);

    public static ProductSubcategory ToEntity(this CreateProductSubcategoryRequest r) => new()
    {
        ProductCategoryId = r.ProductCategoryId,
        Name = (r.Name ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductSubcategoryRequest r, ProductSubcategory e)
    {
        if (r.ProductCategoryId.HasValue) e.ProductCategoryId = r.ProductCategoryId.Value;
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductSubcategoryAuditLogDto ToDto(this ProductSubcategoryAuditLog a) => new(
        a.Id, a.ProductSubcategoryId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ProductCategoryId, a.Name, a.RowGuid, a.SourceModifiedDate);
}
