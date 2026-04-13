using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities;

/// <summary>
/// API key tied to an Identity user. Validated by <see cref="Authentication.ApiKeyAuthenticationHandler"/>
/// from the <c>X-Api-Key</c> request header.
/// </summary>
public class ApiKey
{
    public int Id { get; set; }

    /// <summary>The plain key value sent in the X-Api-Key header. (Stored in plain text for now;
    /// rotate to a hash if/when keys must be reissue-once.)</summary>
    [Required]
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;

    /// <summary>Friendly name shown in the management UI.</summary>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? ExpiresDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public DateTime? LastUsedDate { get; set; }

    public bool IsActive => RevokedDate is null && (ExpiresDate is null || ExpiresDate > DateTime.UtcNow);
}
