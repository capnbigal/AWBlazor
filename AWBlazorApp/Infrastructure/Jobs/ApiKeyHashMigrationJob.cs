using AWBlazorApp.Infrastructure.Authentication;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Infrastructure.Jobs;

/// <summary>
/// One-time Hangfire job that migrates remaining plain-text API keys to SHA-256 hashes.
/// Safe to run multiple times — it only touches keys that still have the <c>ek_</c> prefix.
/// </summary>
public sealed class ApiKeyHashMigrationJob(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<ApiKeyHashMigrationJob> logger)
{
    public async Task ExecuteAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var plainTextKeys = await db.ApiKeys
            .Where(k => k.Key.StartsWith("ek_"))
            .ToListAsync();

        if (plainTextKeys.Count == 0)
        {
            logger.LogInformation("No plain-text API keys found — nothing to migrate.");
            return;
        }

        logger.LogInformation("Found {Count} plain-text API key(s) to hash.", plainTextKeys.Count);

        foreach (var key in plainTextKeys)
        {
            key.Key = ApiKeyHasher.Hash(key.Key);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Successfully hashed {Count} API key(s).", plainTextKeys.Count);
    }
}
