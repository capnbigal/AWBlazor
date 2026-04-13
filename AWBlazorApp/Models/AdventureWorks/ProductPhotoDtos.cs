using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record ProductPhotoDto(
    int Id, string? ThumbnailPhotoFileName, string? LargePhotoFileName,
    bool HasThumbnail, bool HasLargePhoto, DateTime ModifiedDate);

public sealed record CreateProductPhotoRequest
{
    public string? ThumbnailPhotoFileName { get; set; }
    public string? LargePhotoFileName { get; set; }
}

public sealed record UpdateProductPhotoRequest
{
    public string? ThumbnailPhotoFileName { get; set; }
    public string? LargePhotoFileName { get; set; }
}

public sealed record ProductPhotoAuditLogDto(
    int Id, int ProductPhotoId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? ThumbnailPhotoFileName, string? LargePhotoFileName,
    DateTime SourceModifiedDate);

public static class ProductPhotoMappings
{
    public static ProductPhotoDto ToDto(this ProductPhoto e) => new(
        e.Id, e.ThumbnailPhotoFileName, e.LargePhotoFileName,
        e.ThumbNailPhoto is { Length: > 0 }, e.LargePhoto is { Length: > 0 }, e.ModifiedDate);

    public static ProductPhoto ToEntity(this CreateProductPhotoRequest r) => new()
    {
        ThumbnailPhotoFileName = TrimToNull(r.ThumbnailPhotoFileName),
        LargePhotoFileName = TrimToNull(r.LargePhotoFileName),
        // Image bytes are not editable through this CRUD UI; they stay NULL on insert.
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductPhotoRequest r, ProductPhoto e)
    {
        if (r.ThumbnailPhotoFileName is not null) e.ThumbnailPhotoFileName = TrimToNull(r.ThumbnailPhotoFileName);
        if (r.LargePhotoFileName is not null) e.LargePhotoFileName = TrimToNull(r.LargePhotoFileName);
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductPhotoAuditLogDto ToDto(this ProductPhotoAuditLog a) => new(
        a.Id, a.ProductPhotoId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ThumbnailPhotoFileName, a.LargePhotoFileName, a.SourceModifiedDate);

    private static string? TrimToNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
