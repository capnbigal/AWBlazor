using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AWBlazorApp.Data;

namespace AWBlazorApp.Services;

/// <summary>
/// Hangfire recurring job that purges audit-log rows older than the configured retention period
/// (default 365 days). Audit logs grow unbounded otherwise — every entity write produces one
/// audit row across 67 AdventureWorks tables plus ToolSlot, SecurityAuditLog, and others.
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
        // Enumerate every audit log table (PascalCase entity name + "AuditLogs" suffix) and
        // delete rows older than the cutoff. We use raw SQL because there are 70+ audit tables
        // and EF would issue one DELETE per row otherwise.
        await using var db = await dbFactory.CreateDbContextAsync();
        var auditTables = db.Model.GetEntityTypes()
            .Where(e => e.GetTableName()?.EndsWith("AuditLogs", StringComparison.Ordinal) == true)
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
