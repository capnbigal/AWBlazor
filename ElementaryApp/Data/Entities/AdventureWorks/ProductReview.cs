using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>
/// Customer-submitted product reviews. Maps onto the pre-existing
/// <c>Production.ProductReview</c> table. SQL CHECK constraint restricts <c>Rating</c> to 1–5.
/// </summary>
[Table("ProductReview", Schema = "Production")]
public class ProductReview
{
    [Key]
    [Column("ProductReviewID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("ReviewerName")]
    [MaxLength(50)]
    public string ReviewerName { get; set; } = string.Empty;

    [Column("ReviewDate")]
    public DateTime ReviewDate { get; set; }

    [Column("EmailAddress")]
    [MaxLength(50)]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>1 (worst) – 5 (best). SQL CHECK constraint enforces this range.</summary>
    [Column("Rating")]
    public int Rating { get; set; }

    [Column("Comments")]
    [MaxLength(3850)]
    public string? Comments { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
