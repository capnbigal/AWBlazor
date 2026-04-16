using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Production.Domain;

/// <summary>
/// Product photo binaries. Maps onto the pre-existing <c>Production.ProductPhoto</c> table.
/// The two image columns (<c>LargePhoto</c>, <c>ThumbNailPhoto</c>) are mapped as
/// <see cref="byte"/>[] but the dialog UI does NOT expose them for editing — they would
/// require multipart upload handling which is outside the scope of the reference-data CRUD.
/// </summary>
[Table("ProductPhoto", Schema = "Production")]
public class ProductPhoto
{
    [Key]
    [Column("ProductPhotoID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("ThumbNailPhoto")]
    public byte[]? ThumbNailPhoto { get; set; }

    [Column("ThumbnailPhotoFileName")]
    [MaxLength(50)]
    public string? ThumbnailPhotoFileName { get; set; }

    [Column("LargePhoto")]
    public byte[]? LargePhoto { get; set; }

    [Column("LargePhotoFileName")]
    [MaxLength(50)]
    public string? LargePhotoFileName { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
