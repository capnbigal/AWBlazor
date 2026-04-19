using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Dtos;

public sealed record SpecialOfferProductDto(
    int SpecialOfferId, int ProductId, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSpecialOfferProductRequest
{
    public int SpecialOfferId { get; set; }
    public int ProductId { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateSpecialOfferProductRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public sealed record SpecialOfferProductAuditLogDto(
    int Id, int SpecialOfferId, int ProductId, string Action,
    string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class SpecialOfferProductMappings
{
    public static SpecialOfferProductDto ToDto(this SpecialOfferProduct e) => new(
        e.SpecialOfferId, e.ProductId, e.RowGuid, e.ModifiedDate);

    public static SpecialOfferProduct ToEntity(this CreateSpecialOfferProductRequest r) => new()
    {
        SpecialOfferId = r.SpecialOfferId,
        ProductId = r.ProductId,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSpecialOfferProductRequest _, SpecialOfferProduct e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SpecialOfferProductAuditLogDto ToDto(this SpecialOfferProductAuditLog a) => new(
        a.Id, a.SpecialOfferId, a.ProductId, a.Action, a.ChangedBy, a.ChangedDate,
        a.ChangeSummary, a.RowGuid, a.SourceModifiedDate);
}
