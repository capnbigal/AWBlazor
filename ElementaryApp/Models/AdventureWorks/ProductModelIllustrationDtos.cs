using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record ProductModelIllustrationDto(
    int ProductModelId, int IllustrationId, DateTime ModifiedDate);

public sealed record CreateProductModelIllustrationRequest
{
    public int ProductModelId { get; set; }
    public int IllustrationId { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateProductModelIllustrationRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public sealed record ProductModelIllustrationAuditLogDto(
    int Id, int ProductModelId, int IllustrationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

public static class ProductModelIllustrationMappings
{
    public static ProductModelIllustrationDto ToDto(this ProductModelIllustration e) => new(
        e.ProductModelId, e.IllustrationId, e.ModifiedDate);

    public static ProductModelIllustration ToEntity(this CreateProductModelIllustrationRequest r) => new()
    {
        ProductModelId = r.ProductModelId,
        IllustrationId = r.IllustrationId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductModelIllustrationRequest _, ProductModelIllustration e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductModelIllustrationAuditLogDto ToDto(this ProductModelIllustrationAuditLog a) => new(
        a.Id, a.ProductModelId, a.IllustrationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.SourceModifiedDate);
}
