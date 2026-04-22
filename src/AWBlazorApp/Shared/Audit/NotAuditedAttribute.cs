namespace AWBlazorApp.Shared.Audit;

/// <summary>
/// Marks an entity type as excluded from automatic auditing by
/// <see cref="Infrastructure.Persistence.AuditingInterceptor"/>.
///
/// Apply to entities whose changes aren't interesting to track (request logs, background-
/// job queues, heartbeat tables, etc.). The interceptor already excludes the legacy
/// <c>*AuditLog</c> classes by naming convention and the new <see cref="AuditLog"/> itself.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NotAuditedAttribute : Attribute;
