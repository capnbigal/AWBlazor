using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Enterprise.Domain;

/// <summary>
/// Top of the enterprise tree. One row is marked <see cref="IsPrimary"/> — that's us.
/// Sibling rows exist for "other orgs owned by the same parent" that we interact with for
/// external processes (AS2 partners, sister plants, etc.).
/// </summary>
[Table("Organization", Schema = "org")]
public class Organization
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Short uppercase code; unique across the table (e.g. "ALIBALIB", "SISTER-A").</summary>
    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Exactly one row should have this set. Enforced by a filtered unique index.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Self-reference for "owned by same parent" topology.</summary>
    public int? ParentOrganizationId { get; set; }

    /// <summary>Free-form external identifier (AS2 partner id, D-U-N-S, etc.).</summary>
    [MaxLength(128)]
    public string? ExternalRef { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
