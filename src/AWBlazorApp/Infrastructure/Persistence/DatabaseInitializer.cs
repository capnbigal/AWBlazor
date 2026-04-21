using System.Data;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Shared.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace AWBlazorApp.Infrastructure.Persistence;

/// <summary>
/// Applies pending EF Core migrations on startup and seeds initial roles and users.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Entry point — applies pending migrations and seeds. Used by Program.cs.
    /// AWBlazorApp only supports SQL Server (production = <c>AdventureWorks2022</c>, dev/test =
    /// <c>AdventureWorks2022_dev</c>, both on <c>ELITE</c>). If the configured provider is
    /// anything else the call throws — there is no longer a SQLite fallback.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var db = sp.GetRequiredService<ApplicationDbContext>();

        var provider = db.Database.ProviderName ?? string.Empty;
        if (!provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"AWBlazorApp only supports the SQL Server EF provider; current provider is '{provider}'. " +
                "Production points at AdventureWorks2022 on ELITE; dev and tests point at AdventureWorks2022_dev.");
        }

        // The target database may already contain Identity / Forecasting tables that were created
        // out-of-band (or by an earlier prerelease that used different migration IDs). If so,
        // MigrateAsync will fail with "object already exists". Reconcile the __EFMigrationsHistory
        // table against the live schema first so MigrateAsync only applies migrations whose
        // tables really are missing.
        await ReconcileMigrationHistoryAsync(db, logger, cancellationToken);

        logger.LogInformation("Applying EF Core SQL Server migrations...");
        await db.Database.MigrateAsync(cancellationToken);

        // The reconciler stamps a whole migration as applied based on a single marker
        // table — but the migration may create multiple tables. If only some of them
        // exist in the live database, we need to create the missing ones now (before
        // the seeder runs queries against them). Uses EF Core's own diff + SQL generator
        // so the CREATE TABLE statements come from the same source as the migrations.
        await EnsureMissingTablesAsync(db, logger, cancellationToken);

        // After tables exist, walk the model and ALTER TABLE ADD any nullable columns the
        // model expects but the existing tables are missing. Necessary when a table was
        // stamped as already-applied above but its column shape predates our extended
        // ApplicationUser (e.g. missing FirstName/LastName/DisplayName/ProfileUrl).
        await PatchMissingColumnsAsync(db, logger, cancellationToken);

        // Non-nullable columns that the generic patcher can't auto-add (because it only
        // handles NULL columns). Add them explicitly with DEFAULTs.
        await EnsureRequiredColumnsAsync(db, logger, cancellationToken);

        // Composite indexes that improve dashboard / per-user-history query performance. These
        // can't be added via a regular EF migration because the codebase uses runtime model
        // diffing (EnsureMissingTablesAsync) instead of full migrations — a generated migration
        // would re-include every model-only table.
        await EnsureCompositeIndexesAsync(db, logger, cancellationToken);

        // /analytics/geo plots Person.Address.SpatialLocation. Some AdventureWorks restores
        // ship the column with NULLs; seed country-centroid + jitter so the map has data.
        await EnsureSpatialLocationSeededAsync(db, logger, cancellationToken);

        await SeedAsync(sp, cancellationToken);

        // Demo convenience: when Demo:ShiftDates is true, slide every AdventureWorks date
        // column forward so the canned 2011-2014 sample looks current. Mirrors the
        // Demo:AutofillLogin pattern — never flip on in real prod. Idempotent: re-running
        // no-ops once the data is already close to today.
        var shiftDates = sp.GetRequiredService<IConfiguration>().GetValue("Demo:ShiftDates", defaultValue: false);
        if (shiftDates)
        {
            try
            {
                var shifter = sp.GetRequiredService<Features.Admin.Services.AdventureWorksDateShifter>();
                await shifter.ShiftAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Non-fatal — a shift failure shouldn't break app startup.
                logger.LogWarning(ex, "AdventureWorks date shift failed (continuing startup)");
            }
        }
    }

    /// <summary>
    /// If fewer than ~10% of Person.Address rows have a populated SpatialLocation, populate the
    /// remainder with country-centroid + wide random jitter based on the row's StateProvince →
    /// CountryRegionCode. Read-only for databases where SpatialLocation is already seeded.
    /// </summary>
    private static async Task EnsureSpatialLocationSeededAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var probe = await db.Database.SqlQueryRaw<SpatialProbe>(@"
                SELECT
                    CAST(COUNT(*) AS bigint)                                          AS Total,
                    CAST(SUM(CASE WHEN SpatialLocation IS NULL THEN 1 ELSE 0 END) AS bigint) AS NullCount
                FROM Person.Address").ToListAsync(cancellationToken);

            var row = probe.FirstOrDefault();
            if (row is null || row.Total == 0)
            {
                logger.LogInformation("Skipping SpatialLocation seed — Person.Address is empty.");
                return;
            }

            // >10% already populated → assume the data is intact, skip.
            var populated = row.Total - row.NullCount;
            if (populated * 10 >= row.Total)
            {
                logger.LogInformation("SpatialLocation already populated on {Populated}/{Total} rows; skipping seed.",
                    populated, row.Total);
                return;
            }

            // Wide jitter so markers spread across each country instead of piling up on the centroid.
            // Coordinates are rough (no degrees-per-km normalization) — good enough for a POC map.
            const string seedSql = @"
WITH Centroids AS (
    SELECT CountryRegionCode, Lat, Lng, LatJitter, LngJitter FROM (VALUES
        ('US', 39.0,  -98.0, 12.0, 30.0),
        ('CA', 56.0, -106.0, 10.0, 35.0),
        ('GB', 54.0,   -2.0,  3.0,  3.0),
        ('DE', 51.0,   10.0,  3.0,  5.0),
        ('FR', 46.0,    2.0,  4.0,  5.0),
        ('AU',-25.0,  135.0, 14.0, 22.0),
        ('JP', 36.0,  138.0,  6.0,  8.0),
        ('BR',-14.0,  -55.0, 14.0, 16.0),
        ('ES', 40.0,   -4.0,  3.0,  5.0),
        ('IT', 42.0,   12.0,  4.0,  4.0),
        ('NL', 52.0,    5.0,  1.0,  1.5),
        ('MX', 23.0, -102.0,  8.0, 12.0),
        ('KR', 36.0,  128.0,  2.5,  2.0)
    ) AS t(CountryRegionCode, Lat, Lng, LatJitter, LngJitter)
)
UPDATE a
SET SpatialLocation = geography::Point(
    c.Lat + ((CAST(ABS(CHECKSUM(NEWID())) % 2000 AS float) / 1000.0) - 1.0) * c.LatJitter,
    c.Lng + ((CAST(ABS(CHECKSUM(NEWID())) % 2000 AS float) / 1000.0) - 1.0) * c.LngJitter,
    4326)
FROM Person.Address a
INNER JOIN Person.StateProvince sp ON sp.StateProvinceID = a.StateProvinceID
INNER JOIN Centroids c ON c.CountryRegionCode = sp.CountryRegionCode
WHERE a.SpatialLocation IS NULL;";

            var affected = await db.Database.ExecuteSqlRawAsync(seedSql, cancellationToken);
            logger.LogInformation("Seeded SpatialLocation on {Affected} Person.Address rows.", affected);
        }
        catch (Exception ex)
        {
            // Non-fatal — the map will just show "no geo-tagged addresses" if this fails.
            logger.LogWarning(ex, "SpatialLocation seed failed (continuing startup)");
        }
    }

    private sealed class SpatialProbe
    {
        public long Total { get; set; }
        public long NullCount { get; set; }
    }

    /// <summary>
    /// Idempotently creates the composite indexes flagged in the database review. Uses
    /// IF NOT EXISTS guards so it's safe to run on every startup.
    /// </summary>
    private static async Task EnsureCompositeIndexesAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            ("IX_ForecastDefinitions_Status_DeletedDate",
             "CREATE NONCLUSTERED INDEX [IX_ForecastDefinitions_Status_DeletedDate] " +
             "ON [dbo].[ForecastDefinitions]([Status], [DeletedDate]) " +
             "INCLUDE ([CreatedDate])"),

            ("IX_SecurityAuditLogs_UserId_Timestamp",
             "CREATE NONCLUSTERED INDEX [IX_SecurityAuditLogs_UserId_Timestamp] " +
             "ON [dbo].[SecurityAuditLogs]([UserId], [Timestamp] DESC)"),
        };

        foreach (var (name, createSql) in indexes)
        {
            try
            {
                var sql = $"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = '{name}') {createSql}";
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to create index {IndexName} (continuing startup)", name);
            }
        }
    }

    /// <summary>
    /// Maps each migration to a "marker" table created by that migration. If the marker table
    /// is already present in the database AND the migration is missing from
    /// <c>__EFMigrationsHistory</c>, the migration is stamped as applied so EF won't try to
    /// recreate the schema. New migrations should append to this map.
    /// </summary>
    private static readonly (string MigrationSuffix, string MarkerTable)[] MigrationMarkers =
    [
        ("_InitialSchema",              "AspNetRoles"),
        ("_AddApiKeys",                 "ApiKeys"),
        ("_AddToolSlotAuditLogs",       "ToolSlotAuditLogs"),
        // 2026-04 — AddEnterpriseMasterData: creates org.Organization/OrgUnit/Station/Asset/
        // CostCenter/ProductLine + their audit log tables, AND formalizes the Insights tables
        // (DashboardItems/KpiSnapshots/etc.) that EnsureMissingTablesAsync had been fabricating.
        // On fresh databases: DashboardItems doesn't exist → migration runs and creates everything.
        // On dev/existing databases: DashboardItems already exists → migration stamped as applied,
        // then EnsureMissingTablesAsync creates the new org.* tables (with indexes + FKs from the
        // design-time model). Either path produces the same final schema.
        ("_AddEnterpriseMasterData",    "DashboardItems"),
        // 2026-04 — AddAdvancedInventory: creates inv.InventoryLocation/InventoryItem/Lot/
        // SerialUnit/InventoryBalance/InventoryTransactionType/InventoryTransaction/
        // InventoryAdjustment/InventoryTransactionOutbox/InventoryTransactionQueue and five
        // master-data audit log tables. Marker table is InventoryItem — if it already exists
        // someone ran the migration out of band; otherwise the migration lands cleanly.
        ("_AddAdvancedInventory",       "InventoryItem"),
        // 2026-04 — AddLogistics: creates lgx.GoodsReceipt/GoodsReceiptLine/Shipment/
        // ShipmentLine/StockTransfer/StockTransferLine + six audit log tables. Marker is
        // GoodsReceipt (pure lgx product, nothing fabricates it at runtime).
        ("_AddLogistics",               "GoodsReceipt"),
        // 2026-04 — AddMes: creates mes.ProductionRun/RunOperation/OperatorClockEvent/
        // DowntimeEvent/DowntimeReason/WorkInstruction/Revision/Step + six audit logs.
        // Marker is ProductionRun (pure mes product).
        ("_AddMesModule",               "ProductionRun"),
        // 2026-04 — AddQualityModule: creates qa.InspectionPlan/PlanCharacteristic/Inspection/
        // Result/NonConformance/Action/CapaCase/CapaCaseNonConformance + six audit logs.
        // Marker is InspectionPlan (pure qa product).
        ("_AddQualityModule",           "InspectionPlan"),
        // 2026-04 — AddWorkforceModule: creates wf.* schema (training, qualifications,
        // attendance, leave, handover, announcements, alerts) + seven audit logs.
        // Marker is TrainingCourse (pure wf product).
        ("_AddWorkforceModule",         "TrainingCourse"),
        // 2026-04 — AddEngineeringModule: creates eng.* schema (manufacturing routings + steps,
        // BOMs, ECOs + affected items + approvals, documents, deviations) + five audit logs.
        // Marker is EngineeringChangeOrder (pure eng product).
        ("_AddEngineeringModule",       "EngineeringChangeOrder"),
        // 2026-04 — AddMaintenanceModule: creates maint.* schema (asset profiles, PM schedules
        // + tasks, work orders + tasks, spare parts, part usage, meter readings, logs) + four
        // audit logs. Marker is MaintenanceWorkOrder (pure maint product).
        ("_AddMaintenanceModule",       "MaintenanceWorkOrder"),
        // 2026-04 — AddPerformanceModule: creates perf.* schema (OEE snapshots, production /
        // maintenance metric rollups, KPI definitions + values, scorecards, saved reports) +
        // three audit logs. Marker is KpiDefinition (pure perf product).
        ("_AddPerformanceModule",       "KpiDefinition"),
    ];

    /// <summary>
    /// Brings the <c>__EFMigrationsHistory</c> table into sync with the live database. If the
    /// app is pointed at a database whose schema was bootstrapped before EF Core knew about it
    /// (e.g. an existing AdventureWorks2022 instance), this stamps each matched migration as
    /// already-applied so the subsequent <c>MigrateAsync</c> call doesn't try to recreate the
    /// tables. Safe to call against a fresh empty database — it does nothing.
    /// </summary>
    private static async Task ReconcileMigrationHistoryAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var allMigrations = db.Database.GetMigrations().ToList();
        if (allMigrations.Count == 0) return;

        // Make sure __EFMigrationsHistory exists. EF would create it on first MigrateAsync,
        // but we need to insert into it before that — so create it manually if missing.
        await db.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[__EFMigrationsHistory]', 'U') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END
            """,
            cancellationToken);

        var applied = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToHashSet();

        var stamped = 0;
        foreach (var migrationId in allMigrations)
        {
            if (applied.Contains(migrationId)) continue;

            var marker = MigrationMarkers
                .FirstOrDefault(m => migrationId.EndsWith(m.MigrationSuffix, StringComparison.Ordinal));
            if (marker.MarkerTable is null) continue;

            if (!await TableExistsAsync(db, marker.MarkerTable, cancellationToken)) continue;

            logger.LogWarning(
                "Migration {Migration} is missing from __EFMigrationsHistory but its marker table " +
                "[{Table}] already exists. Stamping it as applied.",
                migrationId, marker.MarkerTable);

            // Use the EF Core assembly version as a placeholder ProductVersion.
            var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {migrationId})
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES ({migrationId}, {productVersion})
                """,
                cancellationToken);

            stamped++;
        }

        if (stamped > 0)
        {
            logger.LogWarning(
                "Stamped {Count} migration(s) as already applied based on existing schema. " +
                "MigrateAsync will now apply only genuinely-new migrations.",
                stamped);
        }
    }

    private static async Task<bool> TableExistsAsync(
        ApplicationDbContext db, string tableName, CancellationToken cancellationToken)
        => await TableExistsAsync(db, tableName, "dbo", cancellationToken);

    private static async Task<bool> TableExistsAsync(
        ApplicationDbContext db, string tableName, string schema, CancellationToken cancellationToken)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN OBJECT_ID(@name, N'U') IS NULL THEN 0 ELSE 1 END";
        var p = cmd.CreateParameter();
        p.ParameterName = "@name";
        p.Value = $"[{schema}].[{tableName}]";
        cmd.Parameters.Add(p);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    /// <summary>
    /// Creates any tables defined in the EF model that don't yet exist in the live database.
    /// Uses EF Core's <see cref="IMigrationsModelDiffer"/> to compute "create everything from
    /// scratch" operations against the design-time model, then filters them down to operations
    /// targeting tables that are actually missing. Tables marked
    /// <c>ExcludeFromMigrations</c> (e.g. <c>ToolSlotConfigurations</c>) are explicitly skipped.
    /// </summary>
    /// <remarks>
    /// This handles the case where the reconciler stamped a migration as applied based on a
    /// single marker table, but the migration created multiple tables and not all of them
    /// were present.
    /// </remarks>
    private static async Task EnsureMissingTablesAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var designModel = db.GetService<IDesignTimeModel>().Model;
        var differ = db.GetService<IMigrationsModelDiffer>();
        var generator = db.GetService<IMigrationsSqlGenerator>();

        // Build the set of tables that EF is supposed to manage (i.e. NOT excluded).
        var managedTables = new HashSet<(string Schema, string Table)>();
        foreach (var entityType in designModel.GetEntityTypes())
        {
            if (entityType.IsTableExcludedFromMigrations()) continue;
            var name = entityType.GetTableName();
            if (string.IsNullOrEmpty(name)) continue;
            managedTables.Add((entityType.GetSchema() ?? "dbo", name));
        }

        // Diff null vs current = the full list of operations needed to build the schema from
        // scratch. We then filter down to operations targeting missing managed tables.
        var allOps = differ.GetDifferences(source: null, target: designModel.GetRelationalModel());

        var missing = new HashSet<(string Schema, string Table)>();
        foreach (var (schema, table) in managedTables)
        {
            if (!await TableExistsAsync(db, table, schema, cancellationToken))
            {
                missing.Add((schema, table));
            }
        }

        if (missing.Count == 0) return;

        // Always include schemas used by any missing table — CREATE TABLE [org].[Foo] will fail
        // unless we first ensure the [org] schema exists. EnsureSchema is idempotent (EF emits
        // IF NOT EXISTS), so including it unconditionally is safe.
        var missingSchemas = missing.Select(m => m.Schema).Where(s => s != "dbo").Distinct().ToHashSet();

        var filtered = new List<MigrationOperation>();
        foreach (var op in allOps)
        {
            switch (op)
            {
                case EnsureSchemaOperation ensure:
                    if (missingSchemas.Contains(ensure.Name))
                    {
                        filtered.Add(ensure);
                    }
                    break;

                case CreateTableOperation create:
                    if (missing.Contains((create.Schema ?? "dbo", create.Name)))
                    {
                        filtered.Add(create);
                    }
                    break;

                case CreateIndexOperation idx:
                    if (missing.Contains((idx.Schema ?? "dbo", idx.Table)))
                    {
                        filtered.Add(idx);
                    }
                    break;

                case AddForeignKeyOperation fk:
                    if (missing.Contains((fk.Schema ?? "dbo", fk.Table)))
                    {
                        filtered.Add(fk);
                    }
                    break;

                // Other operation kinds (sequences, alter-database, etc.) are intentionally
                // dropped — we only want to fill in missing tables, not muck with anything
                // else at runtime.
            }
        }

        if (filtered.Count == 0) return;

        foreach (var (schema, table) in missing)
        {
            logger.LogWarning("Creating missing table [{Schema}].[{Table}]", schema, table);
        }

        var commands = generator.Generate(filtered, designModel);
        foreach (var cmd in commands)
        {
            await db.Database.ExecuteSqlRawAsync(cmd.CommandText, cancellationToken);
        }

        logger.LogWarning("Created {Count} missing table(s) from the EF model.", missing.Count);
    }

    /// <summary>
    /// Walks every entity in the EF model. For each table that exists in the live database,
    /// compares the model's column list against <c>INFORMATION_SCHEMA.COLUMNS</c> and
    /// <c>ALTER TABLE ADD</c>s any nullable columns that are missing. NOT NULL columns that
    /// are missing are logged as errors — adding them blindly would either fail (no default)
    /// or insert a default value the user may not want, so we leave that to manual SQL.
    /// </summary>
    /// <summary>
    /// Idempotently adds specific NOT NULL columns that were introduced after their table was
    /// first created. The generic patcher (<see cref="PatchMissingColumnsAsync"/>) only handles
    /// NULL columns; anything NOT NULL needs an explicit DEFAULT so backfill works.
    /// </summary>
    private static async Task EnsureRequiredColumnsAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        // (schema, table, column, typeWithDefault)  — order matters only if later patches depend on earlier ones.
        var patches = new[]
        {
            ("dbo", "SavedQueries", "IsKpi", "bit NOT NULL DEFAULT 0"),
        };

        foreach (var (schema, table, column, typeWithDefault) in patches)
        {
            try
            {
                var sql = $@"
                    IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
                               WHERE s.name = '{schema}' AND t.name = '{table}')
                    AND NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
                                    JOIN sys.schemas s ON s.schema_id = t.schema_id
                                    WHERE s.name = '{schema}' AND t.name = '{table}' AND c.name = '{column}')
                    BEGIN
                        ALTER TABLE [{schema}].[{table}] ADD [{column}] {typeWithDefault};
                    END";
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to ensure column [{Schema}].[{Table}].[{Column}]", schema, table, column);
            }
        }
    }

    private static async Task PatchMissingColumnsAsync(
        ApplicationDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        // Migration-only metadata (e.g. IsTableExcludedFromMigrations) lives on the design-time
        // model, not the read-optimized runtime model that db.Model exposes.
        var designModel = db.GetService<IDesignTimeModel>().Model;

        foreach (var entityType in designModel.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (string.IsNullOrEmpty(tableName)) continue;
            if (entityType.IsTableExcludedFromMigrations()) continue;

            var schema = entityType.GetSchema() ?? "dbo";

            if (!await TableExistsAsync(db, tableName, schema, cancellationToken)) continue;

            var existingColumns = await GetColumnsAsync(db, schema, tableName, cancellationToken);

            foreach (var prop in entityType.GetProperties())
            {
                var columnName = prop.GetColumnName();
                if (string.IsNullOrEmpty(columnName)) continue;
                if (existingColumns.Contains(columnName)) continue;

                var columnType = prop.GetColumnType();

                if (!prop.IsNullable)
                {
                    logger.LogError(
                        "Cannot auto-add NOT NULL column [{Schema}].[{Table}].[{Column}] ({Type}). " +
                        "Add it manually with: ALTER TABLE [<schema>].[<table>] ADD [<column>] <type> NOT NULL DEFAULT (...);",
                        schema, tableName, columnName, columnType);
                    continue;
                }

                logger.LogWarning(
                    "Adding missing nullable column [{Schema}].[{Table}].[{Column}] {Type}",
                    schema, tableName, columnName, columnType);

                var alterSql = $"ALTER TABLE [{schema}].[{tableName}] ADD [{columnName}] {columnType} NULL";
                await db.Database.ExecuteSqlRawAsync(alterSql, cancellationToken);

                // Refresh local cache so a second matching property in the same table doesn't
                // try to re-add the same column.
                existingColumns.Add(columnName);
            }
        }
    }

    private static async Task<HashSet<string>> GetColumnsAsync(
        ApplicationDbContext db, string schema, string tableName, CancellationToken cancellationToken)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS " +
            "WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table";
        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@schema";
        p1.Value = schema;
        cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@table";
        p2.Value = tableName;
        cmd.Parameters.Add(p2);

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.GetString(0));
        }
        return result;
    }

    /// <summary>
    /// Test entry point — assumes the schema is already in place (e.g. via EnsureCreated for
    /// SQLite-in-memory tests) and only seeds the reference data + identity users.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Seed callers may pass either an outer service provider (production) or an already-scoped
        // one (test). Detect that and reuse it instead of nesting another scope.
        var ownsScope = services.GetService<ApplicationDbContext>() is null;
        IServiceScope? localScope = null;
        IServiceProvider sp;
        if (ownsScope)
        {
            localScope = services.CreateScope();
            sp = localScope.ServiceProvider;
        }
        else
        {
            sp = services;
        }

        try
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            await SeedRolesAsync(sp);
            await SeedUsersAsync(sp);
            await SeedReferenceDataAsync(db, cancellationToken);
        }
        finally
        {
            localScope?.Dispose();
        }
    }

    private static async Task SeedRolesAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            DisplayName = "Test User",
            Email = "test@email.com",
            UserName = "test@email.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
        }, "p@55wOrd");

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            DisplayName = "Test Employee",
            Email = "employee@email.com",
            UserName = "employee@email.com",
            FirstName = "Test",
            LastName = "Employee",
            EmailConfirmed = true,
        }, "p@55wOrd", AppRoles.Employee);

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            DisplayName = "Test Manager",
            Email = "manager@email.com",
            UserName = "manager@email.com",
            FirstName = "Test",
            LastName = "Manager",
            EmailConfirmed = true,
        }, "p@55wOrd", AppRoles.Manager, AppRoles.Employee);

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            DisplayName = "Admin User",
            Email = "admin@email.com",
            UserName = "admin@email.com",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
        }, "p@55wOrd", AppRoles.All);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string password,
        params string[] roles)
    {
        var existing = await userManager.FindByEmailAsync(user.Email!);
        if (existing is not null) return;

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create seed user {user.Email}: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
        }

        if (roles.Length > 0)
        {
            var roleResult = await userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign roles to {user.Email}: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        await SeedPrimaryOrganizationAsync(db, cancellationToken);
        await SeedInventoryTransactionTypesAsync(db, cancellationToken);
        await SeedDowntimeReasonsAsync(db, cancellationToken);
    }

    /// <summary>
    /// Makes sure there's at least one row in <c>org.Organization</c> marked <c>IsPrimary=true</c>
    /// so downstream UI (OrgUnit create, Asset create, etc.) always has a root to hang things off.
    /// No-ops once a primary organization exists.
    /// </summary>
    private static async Task SeedPrimaryOrganizationAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var alreadySeeded = await db.Organizations.AsNoTracking().AnyAsync(o => o.IsPrimary, ct);
        if (alreadySeeded) return;

        var primary = new Features.Enterprise.Organizations.Domain.Organization
        {
            Code = "PRIMARY",
            Name = "Primary Organization",
            IsPrimary = true,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.Organizations.Add(primary);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Seeds the 20 canonical <c>inv.InventoryTransactionType</c> rows on first boot. Skips any
    /// code already present so manual edits to individual rows (e.g. flipping EmitsJson for a
    /// specific integration) survive the next startup.
    /// </summary>
    private static async Task SeedInventoryTransactionTypesAsync(ApplicationDbContext db, CancellationToken ct)
    {
        // (Id, Code, Name, Sign, RequiresApproval, EmitsJson)
        // Sign = balance arithmetic: +1 = inflow, -1 = outflow, 0 = paired (two legs) or absolute.
        // RequiresApproval: loss/gain events that need a signer.
        // EmitsJson: downstream consumers care — anything that crosses the four-walls boundary.
        var seeds = new (int Id, string Code, string Name, sbyte Sign, bool RequiresApproval, bool EmitsJson)[]
        {
            ( 1, "RECEIPT",      "Goods receipt",                +1, false, true  ),
            ( 2, "PUTAWAY",      "Putaway to bin",               +1, false, false ),
            ( 3, "PICK",         "Pick to staging",              -1, false, false ),
            ( 4, "PACK",         "Pack for shipment",            -1, false, false ),
            ( 5, "SHIP",         "Ship to customer",             -1, false, true  ),
            ( 6, "ADJUST_INC",   "Adjustment — increase",        +1, true,  false ),
            ( 7, "ADJUST_DEC",   "Adjustment — decrease",        -1, true,  false ),
            ( 8, "MOVE",         "Move between locations",        0, false, false ),
            ( 9, "SCRAP",        "Scrap / write-off",            -1, true,  false ),
            (10, "RETURN_CUST",  "Customer return",              +1, false, true  ),
            (11, "RETURN_VEND",  "Return to vendor",             -1, false, true  ),
            (12, "COUNT",        "Physical count (absolute)",     0, false, false ),
            (13, "CYCLE_COUNT",  "Cycle count (absolute)",        0, false, false ),
            (14, "WIP_ISSUE",    "Issue to work order",          -1, false, false ),
            (15, "WIP_RECEIPT",  "Receipt from work order",      +1, false, false ),
            (16, "ASSEMBLY",     "Assembly build",               +1, false, false ),
            (17, "DISASSEMBLY",  "Disassembly / teardown",       -1, false, false ),
            (18, "TRANSFER_OUT", "Transfer out (inter-org)",     -1, false, true  ),
            (19, "TRANSFER_IN",  "Transfer in (inter-org)",      +1, false, true  ),
            (20, "REWORK",       "Rework loop",                   0, false, false ),
        };

        var existingCodes = await db.InventoryTransactionTypes.AsNoTracking()
            .Select(t => t.Code).ToListAsync(ct);
        var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var toAdd = seeds
            .Where(s => !existing.Contains(s.Code))
            .Select(s => new Features.Inventory.Types.Domain.InventoryTransactionType
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Sign = s.Sign,
                RequiresApproval = s.RequiresApproval,
                EmitsJson = s.EmitsJson,
                IsActive = true,
            })
            .ToList();

        if (toAdd.Count == 0) return;
        db.InventoryTransactionTypes.AddRange(toAdd);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Seeds the 15 canonical <c>mes.DowntimeReason</c> codes on first boot. Skips any code
    /// already present so user edits to a specific row's name/description survive restarts.
    /// </summary>
    private static async Task SeedDowntimeReasonsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var seeds = new (string Code, string Name, string? Description)[]
        {
            ("SETUP",              "Setup / changeover",       "Job changeover, fixturing, machine warm-up."),
            ("MATERIAL",           "Material shortage",        "Out of stock at the line; awaiting kitting / replenishment."),
            ("MACHINE_FAULT",      "Machine fault",            "Equipment failure halting the run; see maintenance work order."),
            ("OPERATOR_BREAK",     "Operator break",           "Scheduled meal / rest break."),
            ("QUALITY_HOLD",       "Quality hold",             "Awaiting inspector sign-off after defect / NCR."),
            ("CHANGEOVER",         "Product changeover",       "Switching to a different SKU."),
            ("CLEANING",           "Cleaning / sanitation",    "Sanitation cycle between runs."),
            ("MAINT_SCHEDULED",    "Maintenance — scheduled",  "Planned PM service window."),
            ("MAINT_UNSCHEDULED",  "Maintenance — unscheduled","Unplanned repair, including post-failure recovery."),
            ("POWER",              "Power / utility outage",   "Loss of electrical, compressed air, water, etc."),
            ("TOOLING",            "Tooling change",           "Tool wear-out replacement, indexing."),
            ("WAIT_QC",            "Waiting for QC",           "First-piece or in-process inspection backlog."),
            ("WAIT_MATERIAL",      "Waiting for material",     "Upstream operation behind schedule."),
            ("MEETING",            "Meeting / training",       "Crew briefing, safety stand-down, training session."),
            ("OTHER",              "Other",                    "Catch-all when no other code fits."),
        };

        var existingCodes = await db.DowntimeReasons.AsNoTracking()
            .Select(r => r.Code).ToListAsync(ct);
        var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var toAdd = seeds
            .Where(s => !existing.Contains(s.Code))
            .Select(s => new Features.Mes.Downtime.Domain.DowntimeReason
            {
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            })
            .ToList();

        if (toAdd.Count == 0) return;
        db.DowntimeReasons.AddRange(toAdd);
        await db.SaveChangesAsync(ct);
    }
}
