using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record ProductDescriptionDto(int Id, string Description, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateProductDescriptionRequest
{
    public string? Description { get; set; }
}

public sealed record UpdateProductDescriptionRequest
{
    public string? Description { get; set; }
}

public sealed record ProductDescriptionAuditLogDto(
    int Id, int ProductDescriptionId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Description, Guid RowGuid, DateTime SourceModifiedDate);

public static class ProductDescriptionMappings
{
    public static ProductDescriptionDto ToDto(this ProductDescription e)
        => new(e.Id, e.Description, e.RowGuid, e.ModifiedDate);

    public static ProductDescription ToEntity(this CreateProductDescriptionRequest r) => new()
    {
        Description = (r.Description ?? string.Empty).Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductDescriptionRequest r, ProductDescription e)
    {
        if (r.Description is not null) e.Description = r.Description.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductDescriptionAuditLogDto ToDto(this ProductDescriptionAuditLog a) => new(
        a.Id, a.ProductDescriptionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Description, a.RowGuid, a.SourceModifiedDate);
}
