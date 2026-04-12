using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

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
