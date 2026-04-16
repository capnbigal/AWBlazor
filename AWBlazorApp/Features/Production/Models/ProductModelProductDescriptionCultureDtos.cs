using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Models;

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

public sealed record ProductModelProductDescriptionCultureAuditLogDto(
    int Id, int ProductModelId, int ProductDescriptionId, string CultureId,
    string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

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

    public static ProductModelProductDescriptionCultureAuditLogDto ToDto(this ProductModelProductDescriptionCultureAuditLog a) => new(
        a.Id, a.ProductModelId, a.ProductDescriptionId, a.CultureId, a.Action, a.ChangedBy, a.ChangedDate,
        a.ChangeSummary, a.SourceModifiedDate);
}
