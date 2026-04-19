using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Infrastructure.Persistence;

/// <summary>
/// Helpers for the create-entity-then-write-audit-log pattern that appears in 67+ endpoints
/// and dialog handlers across the codebase. Wraps both writes in a single transaction so the
/// audit log can never be missing if the entity insert succeeds (and vice versa).
///
/// New CRUD endpoints should prefer these helpers over open-coding the pattern.
/// </summary>
public static class AuditedSaveExtensions
{
    /// <summary>
    /// Persist <paramref name="entity"/>, then immediately persist an audit-log row in the same
    /// transaction. The audit-log builder receives the saved entity (with its identity-generated
    /// key) so it can capture the post-save state.
    /// </summary>
    public static async Task AddWithAuditAsync<TEntity, TAuditLog>(
        this DbContext db,
        TEntity entity,
        Func<TEntity, TAuditLog> buildAuditLog,
        CancellationToken ct = default)
        where TEntity : class
        where TAuditLog : class
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Set<TEntity>().Add(entity);
        await db.SaveChangesAsync(ct);
        db.Set<TAuditLog>().Add(buildAuditLog(entity));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    /// <summary>
    /// Persist an audit-log row, then remove <paramref name="entity"/> in the same transaction.
    /// Order matters: writing the audit row first (while the entity still exists) lets the audit
    /// log capture the final state of the deleted row.
    /// </summary>
    public static async Task DeleteWithAuditAsync<TEntity, TAuditLog>(
        this DbContext db,
        TEntity entity,
        TAuditLog auditLog,
        CancellationToken ct = default)
        where TEntity : class
        where TAuditLog : class
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Set<TAuditLog>().Add(auditLog);
        db.Set<TEntity>().Remove(entity);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
