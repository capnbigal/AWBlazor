using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Infrastructure.Authentication;

/// <summary>Canonical <see cref="SecurityAuditLog.EventType"/> values. Kept in one place so the
/// writers here and the <c>FailedLoginsLast24h</c> notification metric agree on the exact strings.</summary>
public static class SecurityEventTypes
{
    public const string LoginSucceeded = "LoginSucceeded";
    public const string LoginFailed    = "LoginFailed";
    public const string LockedOut      = "LockedOut";
    public const string PasswordChanged = "PasswordChanged";
    public const string ApiKeyGenerated = "ApiKeyGenerated";
    public const string ApiKeyRevoked   = "ApiKeyRevoked";
    public const string RoleGranted     = "RoleGranted";
    public const string RoleRevoked     = "RoleRevoked";
}

/// <summary>
/// Writes security-relevant events to the <see cref="SecurityAuditLog"/> table. Before this existed
/// the table was never written to, so the <c>FailedLoginsLast24h</c> notification metric
/// (NotificationRuleEvaluator) was permanently zero — a dead alert. Auditing is best-effort: a
/// failure here must never break the user-facing flow (especially login), so all writes are wrapped
/// in a try/catch that swallows.
/// </summary>
public interface ISecurityAuditService
{
    Task LogAsync(string eventType, string? userId, string? userEmail, string? details, string? ipAddress, CancellationToken ct = default);
}

/// <summary>
/// <see cref="ISecurityAuditService"/> backed by <see cref="IDbContextFactory{TContext}"/> so it can
/// be a stateless singleton and safely create a fresh context per write (no shared-DbContext hazard
/// from the Blazor circuit). See <see cref="SecurityEventTypes"/> for the event vocabulary.
/// </summary>
public sealed class SecurityAuditService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<SecurityAuditService> logger) : ISecurityAuditService
{
    public async Task LogAsync(string eventType, string? userId, string? userEmail, string? details, string? ipAddress, CancellationToken ct = default)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);
            db.SecurityAuditLogs.Add(new SecurityAuditLog
            {
                EventType = eventType,
                UserId = userId ?? string.Empty,
                UserEmail = userEmail,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Never let auditing break the calling flow (login, password change, etc.).
            logger.LogWarning(ex, "Failed to write security audit event {EventType} for {Email}", eventType, userEmail);
        }
    }
}
