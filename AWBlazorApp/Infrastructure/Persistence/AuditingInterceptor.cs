using AWBlazorApp.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AWBlazorApp.Infrastructure.Persistence;

/// <summary>
/// Populates audit fields (CreatedBy/CreatedDate/ModifiedBy/ModifiedDate/DeletedDate)
/// on <see cref="AuditableEntity"/> entries during SaveChanges.
/// </summary>
public sealed class AuditingInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var user = httpContextAccessor.HttpContext?.User?.Identity?.Name;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = now;
                    entry.Entity.CreatedBy ??= user;
                    entry.Entity.ModifiedDate = now;
                    entry.Entity.ModifiedBy ??= user;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = now;
                    entry.Entity.ModifiedBy = user;
                    // Preserve original CreatedBy / CreatedDate.
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedDate)).IsModified = false;
                    break;
            }
        }
    }
}
