using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Data.Entities.Auth;

/// <summary>
/// API key tied to an Identity user. Validated by <see cref="Authentication.ApiKeyAuthenticationHandler"/>
/// from the <c>X-Api-Key</c> request header.
/// </summary>
public class ApiKey
{
    public int Id { get; set; }

    /// <summary>SHA-256 hash of the API key. New keys are stored hashed; legacy plain-text keys
    /// (those starting with "ek_") are still accepted at the auth layer for backwards compat
    /// until they're rotated. See <see cref="Authentication.ApiKeyHasher"/>.</summary>
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
