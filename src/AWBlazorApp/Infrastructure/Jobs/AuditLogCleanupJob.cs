using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that purges audit-log rows older than the configured retention period
/// (default 365 days). Covers the consolidated <c>audit.AuditLog</c> table (populated by
/// <see cref="Persistence.AuditLogInterceptor"/>) and <c>SecurityAuditLogs</c>.
///
/// Configure retention via Features:AuditLogRetentionDays in appsettings (default: 365).
/// Registered daily at 03:30 UTC in MiddlewarePipeline.cs.
/// </summary>
public sealed class AuditLogCleanupJob(
    IConfiguration configuration,
    IDbContextFactory<ApplicationDbContext> dbFactory)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    private readonly int _retentionDays = configuration.GetValue("Features:AuditLogRetentionDays", defaultValue: 365);

    [AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync()
    {
        // Enumerate every audit log table — the consolidated audit.AuditLog plus any entity
        // whose table name ends in "AuditLogs" (currently SecurityAuditLogs). Raw SQL so we
        // issue one DELETE per table rather than a round-trip per row.
        await using var db = await dbFactory.CreateDbContextAsync();
        var auditTables = db.Model.GetEntityTypes()
            .Where(e =>
            {
                var name = e.GetTableName();
                return name is not null
                    && (name.EndsWith("AuditLogs", StringComparison.Ordinal) || name == "AuditLog");
            })
            .Select(e => new
            {
                Schema = e.GetSchema() ?? "dbo",
                Table = e.GetTableName()!,
                DateColumn = e.FindProperty("ChangedDate") is not null ? "ChangedDate"
                           : e.FindProperty("Timestamp") is not null ? "Timestamp"
                           : null,
            })
            .Where(t => t.DateColumn is not null)
            .ToList();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var totalDeleted = 0;
        foreach (var t in auditTables)
        {
            var sql = $"DELETE FROM [{t.Schema}].[{t.Table}] WHERE [{t.DateColumn}] < DATEADD(day, -{_retentionDays}, GETUTCDATE())";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 300; // 5 min for large tables
            try
            {
                var deleted = await cmd.ExecuteNonQueryAsync();
                if (deleted > 0)
                {
                    Serilog.Log.Information(
                        "AuditLogCleanupJob: deleted {Count} rows from {Table} older than {Days} days",
                        deleted, t.Table, _retentionDays);
                }
                totalDeleted += deleted;
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex,
                    "AuditLogCleanupJob: failed to clean {Table} (continuing with remaining tables)", t.Table);
            }
        }

        Serilog.Log.Information(
            "AuditLogCleanupJob: complete — {TotalDeleted} rows deleted across {TableCount} audit tables",
            totalDeleted, auditTables.Count);
    }
}
