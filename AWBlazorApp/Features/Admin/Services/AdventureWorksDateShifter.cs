using System.Data;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Admin.Services;

/// <summary>
/// Shifts every datetime-like column in the AdventureWorks business schemas
/// (<c>Sales</c>, <c>Purchasing</c>, <c>Production</c>, <c>HumanResources</c>, <c>Person</c>)
/// by a uniform day offset so the demo data stays "recent" relative to today.
/// </summary>
/// <remarks>
/// Uniform offset preserves every relative gap (order → ship lead time, hire age spread,
/// price-history windows, …) — only the absolute position on the calendar moves. The
/// operation is idempotent: if the most recent sales order is already within
/// <see cref="SkipThresholdDays"/> of today, the shift is a no-op. Runs against whichever
/// database the current connection points at (<c>AdventureWorks2022</c> in production,
/// <c>AdventureWorks2022_dev</c> in dev and tests) — gated by the <c>Demo:ShiftDates</c>
/// config flag at startup, mirroring the <c>Demo:AutofillLogin</c> pattern.
/// </remarks>
public sealed class AdventureWorksDateShifter(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<AdventureWorksDateShifter> logger)
{
    // Schemas owned by AdventureWorks — everything else (dbo app tables, Identity, Hangfire)
    // is intentionally excluded.
    private static readonly string[] TargetSchemas =
    [
        "Sales", "Purchasing", "Production", "HumanResources", "Person",
    ];

    // Shiftable SQL types. `time` is excluded — it's a time-of-day value and adding days
    // to it either wraps or errors.
    private static readonly string[] ShiftableTypes =
    [
        "datetime", "datetime2", "smalldatetime", "date",
    ];

    // If `today - maxOrderDate <= this`, skip. Prevents the shift from re-running on every
    // startup once we're in sync.
    private const int SkipThresholdDays = 7;

    public async Task<ShiftResult> ShiftAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var priorMax = await GetMaxOrderDateAsync(db, ct);
        if (priorMax is null)
        {
            logger.LogInformation("Date shift skipped — Sales.SalesOrderHeader is empty (target DB may not be AdventureWorks).");
            return ShiftResult.Skipped("Sales.SalesOrderHeader is empty");
        }

        var today = DateTime.Today;
        var offsetDays = (int)(today - priorMax.Value.Date).TotalDays;

        if (Math.Abs(offsetDays) <= SkipThresholdDays)
        {
            logger.LogInformation(
                "Date shift skipped — max OrderDate {Max:yyyy-MM-dd} is already within {Threshold} days of today.",
                priorMax.Value, SkipThresholdDays);
            return ShiftResult.Skipped($"Already within {SkipThresholdDays} days of today");
        }

        var columns = await DiscoverShiftableColumnsAsync(db, ct);
        if (columns.Count == 0)
        {
            logger.LogWarning("Date shift skipped — no shiftable columns discovered in target schemas.");
            return ShiftResult.Skipped("No shiftable columns found");
        }

        // Group by table so each table gets a single UPDATE with every shiftable column in the
        // SET clause. Necessary because AdventureWorks has `CHECK (EndDate >= StartDate)` style
        // constraints that get evaluated per-statement — shifting the columns one at a time
        // would violate the constraint on the intermediate state.
        var perTable = columns
            .GroupBy(c => (c.Schema, c.Table))
            .ToList();

        logger.LogInformation(
            "Shifting {ColumnCount} AdventureWorks date columns across {TableCount} tables by {OffsetDays} days " +
            "(prior max OrderDate = {PriorMax:yyyy-MM-dd}).",
            columns.Count, perTable.Count, offsetDays, priorMax.Value);

        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var tx = await conn.BeginTransactionAsync(ct);
        var totalRowsAffected = 0L;
        try
        {
            foreach (var group in perTable)
            {
                var (schema, table) = group.Key;
                var setClauses = string.Join(", ",
                    group.Select(c => $"[{c.Column}] = DATEADD(day, @offset, [{c.Column}])"));

                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = $"UPDATE [{schema}].[{table}] SET {setClauses}";
                var p = cmd.CreateParameter();
                p.ParameterName = "@offset";
                p.Value = offsetDays;
                cmd.Parameters.Add(p);
                totalRowsAffected += await cmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        var newMax = await GetMaxOrderDateAsync(db, ct);
        logger.LogInformation(
            "Date shift complete — applied offset {OffsetDays} days across {ColumnCount} columns, {Rows:N0} row updates. New max OrderDate = {NewMax:yyyy-MM-dd}.",
            offsetDays, columns.Count, totalRowsAffected, newMax);

        return ShiftResult.Applied(offsetDays, columns.Count, totalRowsAffected, priorMax.Value, newMax ?? priorMax.Value);
    }

    private static async Task<DateTime?> GetMaxOrderDateAsync(ApplicationDbContext db, CancellationToken ct)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT MAX(OrderDate) FROM Sales.SalesOrderHeader";
            var raw = await cmd.ExecuteScalarAsync(ct);
            return raw is DateTime d ? d : null;
        }
        catch
        {
            // Target DB may not have AdventureWorks tables at all (e.g. a fresh app DB with no AW data).
            return null;
        }
    }

    private static async Task<List<(string Schema, string Table, string Column)>> DiscoverShiftableColumnsAsync(
        ApplicationDbContext db, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        var schemaList = string.Join(",", TargetSchemas.Select(s => $"'{s}'"));
        var typeList = string.Join(",", ShiftableTypes.Select(t => $"'{t}'"));
        cmd.CommandText = $@"
            SELECT c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS c
            INNER JOIN INFORMATION_SCHEMA.TABLES t
                ON  t.TABLE_SCHEMA = c.TABLE_SCHEMA
                AND t.TABLE_NAME   = c.TABLE_NAME
                AND t.TABLE_TYPE   = 'BASE TABLE'
            WHERE c.TABLE_SCHEMA IN ({schemaList})
              AND c.DATA_TYPE    IN ({typeList})
            ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION";

        var result = new List<(string, string, string)>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        return result;
    }
}

public sealed record ShiftResult(
    bool DidShift,
    int OffsetDays,
    int ColumnsUpdated,
    long RowsUpdated,
    DateTime PriorMaxOrderDate,
    DateTime NewMaxOrderDate,
    string? SkipReason)
{
    public static ShiftResult Applied(int offset, int columns, long rows, DateTime prior, DateTime @new)
        => new(true, offset, columns, rows, prior, @new, null);

    public static ShiftResult Skipped(string reason)
        => new(false, 0, 0, 0, default, default, reason);
}
