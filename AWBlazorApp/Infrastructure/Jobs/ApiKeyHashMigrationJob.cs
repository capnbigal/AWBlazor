using AWBlazorApp.Infrastructure.Authentication;
using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Infrastructure.Jobs;

/// <summary>
/// One-time Hangfire job that migrates remaining plain-text API keys to SHA-256 hashes.
/// Safe to run multiple times — it only touches keys that still have the <c>ek_</c> prefix,
/// and it tolerates duplicate rows (e.g. a plain-text row whose hashed form already exists
/// in another row) by dropping the plain-text duplicate rather than UPDATE-ing into a
/// unique-index collision.
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

        // Track hashes claimed in this batch so two plain-text rows sharing the same raw key
        // (which would hash to the same value) don't both try to UPDATE into that hash.
        var claimedHashes = new HashSet<string>(StringComparer.Ordinal);
        var hashed = 0;
        var dropped = 0;

        foreach (var key in plainTextKeys)
        {
            var targetHash = ApiKeyHasher.Hash(key.Key);

            // Collision outside this batch: another row already stores the hashed form of this key.
            // Use AsNoTracking so the check reads from SQL and isn't confused by in-memory updates.
            var existingHashRowId = await db.ApiKeys
                .AsNoTracking()
                .Where(k => k.Id != key.Id && k.Key == targetHash)
                .Select(k => (int?)k.Id)
                .FirstOrDefaultAsync();

            // Collision inside this batch: an earlier iteration already claimed targetHash.
            var inBatchCollision = !claimedHashes.Add(targetHash);

            if (existingHashRowId is not null || inBatchCollision)
            {
                logger.LogWarning(
                    "Dropping duplicate plain-text ApiKey {Id} ({Name}) — hashed form {Hash} already stored" +
                    (existingHashRowId is not null ? " in row {ExistingId}." : " by an earlier iteration in this batch."),
                    key.Id, key.Name, targetHash, existingHashRowId);
                db.ApiKeys.Remove(key);
                dropped++;
            }
            else
            {
                key.Key = targetHash;
                hashed++;
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation(
            "ApiKey hash migration complete — hashed {Hashed} key(s), removed {Dropped} duplicate row(s).",
            hashed, dropped);
    }
}
