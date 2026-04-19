using System.Security.Cryptography;
using System.Text;

namespace AWBlazorApp.Infrastructure.Authentication;

/// <summary>
/// SHA-256 hash utility for API keys. New keys are stored as hashes;
/// legacy plain-text keys are detected by checking if the stored value
/// matches the <c>ek_</c> prefix (unhashed keys start with that prefix,
/// SHA-256 hex strings never will).
/// </summary>
public static class ApiKeyHasher
{
    private const string KeyPrefix = "ek_";

    public static string Hash(string plainKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainKey));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>Returns true if the stored key value is a plain-text key (not yet hashed).</summary>
    public static bool IsPlainText(string storedKey) => storedKey.StartsWith(KeyPrefix, StringComparison.Ordinal);

    /// <summary>
    /// Compares a provided plain-text key against a stored value.
    /// Supports both legacy plain-text storage and SHA-256 hashed storage.
    /// </summary>
    public static bool Verify(string providedKey, string storedKey)
    {
        if (IsPlainText(storedKey))
            return string.Equals(providedKey, storedKey, StringComparison.Ordinal);

        return string.Equals(Hash(providedKey), storedKey, StringComparison.OrdinalIgnoreCase);
    }
}
