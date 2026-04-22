using System.Text;
using System.Text.Json;
using AWBlazorApp.Shared.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AWBlazorApp.Infrastructure.Persistence;

/// <summary>
/// EF Core SaveChanges interceptor that emits an <see cref="AuditLog"/> row for every
/// Added / Modified / Deleted entity on each call to SaveChangesAsync. Replaces the
/// hand-rolled <c>*AuditService</c> classes and per-entity audit tables.
/// </summary>
/// <remarks>
/// <para>
/// Runs alongside <see cref="AuditingInterceptor"/> (which populates CreatedBy /
/// ModifiedBy timestamps on <c>AuditableEntity</c> rows — different responsibility).
/// The current user is read from <see cref="IHttpContextAccessor"/> at call time.
/// </para>
/// <para>
/// Exclusions:
/// <list type="bullet">
///   <item><see cref="AuditLog"/> itself (recursion)</item>
///   <item>Any type whose name ends in <c>AuditLog</c> — legacy per-entity audit tables
///     still written by existing <c>*AuditService</c> call sites during this transition</item>
///   <item>Any type decorated with <see cref="NotAuditedAttribute"/></item>
///   <item>ASP.NET Core Identity types</item>
/// </list>
/// </para>
/// </remarks>
public sealed class AuditLogInterceptor(IHttpContextAccessor httpContext) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context is null) return;

        var user = httpContext.HttpContext?.User?.Identity?.IsAuthenticated == true
            ? httpContext.HttpContext.User.Identity.Name
            : null;
        var now = DateTime.UtcNow;

        // Snapshot — adding AuditLog entries inside the loop would mutate Entries().
        var candidates = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => ShouldAudit(e.Entity.GetType()))
            .ToList();

        foreach (var entry in candidates)
        {
            var log = BuildLog(entry, user, now);
            if (log is not null)
            {
                context.Add(log);
            }
        }
    }

    private static bool ShouldAudit(Type t)
    {
        if (t == typeof(AuditLog)) return false;
        if (t.Name.EndsWith("AuditLog", StringComparison.Ordinal)) return false;
        if (Attribute.IsDefined(t, typeof(NotAuditedAttribute))) return false;
        if (t.Namespace?.StartsWith("Microsoft.AspNetCore.Identity", StringComparison.Ordinal) == true) return false;
        return true;
    }

    private static AuditLog? BuildLog(EntityEntry entry, string? user, DateTime now)
    {
        var action = entry.State switch
        {
            EntityState.Added    => "Created",
            EntityState.Modified => "Updated",
            EntityState.Deleted  => "Deleted",
            _                    => null,
        };
        if (action is null) return null;

        var (json, summary) = entry.State switch
        {
            EntityState.Added    => BuildCreatedPayload(entry),
            EntityState.Modified => BuildUpdatedPayload(entry),
            EntityState.Deleted  => BuildDeletedPayload(entry),
            _                    => (null, null),
        };

        // No-op updates (e.g. entity marked Modified but all props unchanged) don't earn a row.
        if (entry.State == EntityState.Modified && json is null) return null;

        return new AuditLog
        {
            EntityType = entry.Entity.GetType().Name,
            EntityId = ExtractPrimaryKey(entry),
            Action = action,
            ChangedBy = user,
            ChangedDate = now,
            ChangesJson = json,
            Summary = AuditDiffHelpers.Truncate(summary, 2048),
        };
    }

    private static string ExtractPrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return "";

        // Added rows may still have default PK values here — SaveChanges hasn't assigned
        // the identity yet. Readers can still query by EntityType + ChangedDate.
        var parts = key.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "")
            .ToArray();

        return parts.Length == 1 ? parts[0] : string.Join('|', parts);
    }

    private static (string? Json, string? Summary) BuildCreatedPayload(EntityEntry entry)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey()) continue;
            payload[prop.Metadata.Name] = prop.CurrentValue;
        }
        return (Serialize(payload), "Created");
    }

    private static (string? Json, string? Summary) BuildDeletedPayload(EntityEntry entry)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey()) continue;
            payload[prop.Metadata.Name] = prop.OriginalValue;
        }
        return (Serialize(payload), "Deleted");
    }

    private static (string? Json, string? Summary) BuildUpdatedPayload(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>(StringComparer.Ordinal);
        var summary = new StringBuilder();

        foreach (var prop in entry.Properties)
        {
            if (!prop.IsModified) continue;
            if (prop.Metadata.IsPrimaryKey()) continue;
            var before = prop.OriginalValue;
            var after = prop.CurrentValue;
            if (Equals(before, after)) continue;

            changes[prop.Metadata.Name] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["before"] = before,
                ["after"] = after,
            };
            AuditDiffHelpers.AppendIfChanged(summary, prop.Metadata.Name, before, after);
        }

        return changes.Count == 0
            ? (null, null)
            : (Serialize(changes), summary.ToString());
    }

    private static string Serialize(Dictionary<string, object?> payload)
    {
        try
        {
            return JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch
        {
            // Fallback: if any value isn't serializable, stringify at the top level.
            var safe = payload.ToDictionary(
                kv => kv.Key,
                kv => (object?)(kv.Value?.ToString()),
                StringComparer.Ordinal);
            return JsonSerializer.Serialize(safe, JsonOptions);
        }
    }
}
