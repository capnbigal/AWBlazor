using Hangfire;
using Microsoft.Data.SqlClient;

namespace AWBlazorApp.Services.Jobs;

/// <summary>
/// Hangfire recurring job that purges <c>dbo.RequestLogs</c> rows older than 30 days.
/// Registered as daily at 03:00 UTC in Program.cs.
/// </summary>
public sealed class RequestLogCleanupJob(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    [AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(
            "DELETE FROM dbo.RequestLogs WHERE [TimeStamp] < DATEADD(day, -30, GETUTCDATE())", conn);
        var deleted = await cmd.ExecuteNonQueryAsync();
        Serilog.Log.Information("RequestLogCleanupJob: deleted {Count} rows older than 30 days", deleted);
    }
}
