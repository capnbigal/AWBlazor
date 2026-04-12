using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record ProductCategoryDto(int Id, string Name, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateProductCategoryRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateProductCategoryRequest
{
    public string? Name { get; set; }
}

public sealed record ProductCategoryAuditLogDto(
    int Id, int ProductCategoryId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, Guid RowGuid, DateTime SourceModifiedDate);

public static class ProductCategoryMappings
{
    public static ProductCategoryDto ToDto(this ProductCategory e)
        => new(e.Id, e.Name, e.RowGuid, e.ModifiedDate);

    public static ProductCategory ToEntity(this CreateProductCategoryRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductCategoryRequest r, ProductCategory e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductCategoryAuditLogDto ToDto(this ProductCategoryAuditLog a) => new(
        a.Id, a.ProductCategoryId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.RowGuid, a.SourceModifiedDate);
}
