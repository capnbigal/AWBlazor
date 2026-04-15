using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Gallery.Endpoints;

public static class PhotoGalleryEndpoints
{
    /// <summary>
    /// Streams raw ProductPhoto image bytes for the gallery page. The ProductPhoto CRUD grid
    /// only flags presence of bytes; these endpoints actually serve them so MudCarousel /
    /// &lt;img&gt; tags can display the photos.
    /// </summary>
    public static IEndpointRouteBuilder MapPhotoGalleryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gallery")
            .WithTags("Gallery")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/photos/{id:int}/large", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var photo = await db.ProductPhotos.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new { p.LargePhoto, p.LargePhotoFileName })
                .FirstOrDefaultAsync(ct);
            return photo?.LargePhoto is null
                ? Results.NotFound()
                : Results.File(photo.LargePhoto, MimeFromFileName(photo.LargePhotoFileName));
        })
        .WithName("GetProductPhotoLarge")
        .WithSummary("Stream the large image bytes for a ProductPhoto row.");

        group.MapGet("/photos/{id:int}/thumb", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var photo = await db.ProductPhotos.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new { p.ThumbNailPhoto, p.ThumbnailPhotoFileName })
                .FirstOrDefaultAsync(ct);
            return photo?.ThumbNailPhoto is null
                ? Results.NotFound()
                : Results.File(photo.ThumbNailPhoto, MimeFromFileName(photo.ThumbnailPhotoFileName));
        })
        .WithName("GetProductPhotoThumb")
        .WithSummary("Stream the thumbnail image bytes for a ProductPhoto row.");

        return app;
    }

    private static string MimeFromFileName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return "application/octet-stream";
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png"           => "image/png",
            "gif"           => "image/gif",
            "webp"          => "image/webp",
            "bmp"           => "image/bmp",
            _               => "application/octet-stream",
        };
    }
}
