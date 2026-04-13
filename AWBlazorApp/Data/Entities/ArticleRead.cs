using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities;

/// <summary>
/// Tracks when a user first reads a user-guide article. One row per user per slug.
/// Re-visits do not create additional rows — only the first read is recorded.
/// </summary>
public class ArticleRead
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(200)]
    public string ArticleSlug { get; set; } = string.Empty;

    public DateTime ReadDate { get; set; }
}
