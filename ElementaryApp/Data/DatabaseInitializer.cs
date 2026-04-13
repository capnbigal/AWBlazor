using System.Data;
using ElementaryApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace ElementaryApp.Data;

/// <summary>
/// Applies pending EF Core migrations on startup and seeds initial roles and users.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Entry point — applies pending migrations and seeds. Used by Program.cs.
    /// ElementaryApp only supports SQL Server (production = <c>AdventureWorks2022</c>, dev/test =
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
                $"ElementaryApp only supports the SQL Server EF provider; current provider is '{provider}'. " +
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

        await SeedAsync(sp, cancellationToken);
    }

    /// <summary>
    /// Maps each migration to a "marker" table created by that migration. If the marker table
    /// is already present in the database AND the migration is missing from
    /// <c>__EFMigrationsHistory</c>, the migration is stamped as applied so EF won't try to
    /// recreate the schema. New migrations should append to this map.
    /// </summary>
    private static readonly (string MigrationSuffix, string MarkerTable)[] MigrationMarkers =
    [
        ("_InitialSchema",         "AspNetRoles"),
        ("_AddApiKeys",            "ApiKeys"),
        ("_AddToolSlotAuditLogs",  "ToolSlotAuditLogs"),
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

        var filtered = new List<MigrationOperation>();
        foreach (var op in allOps)
        {
            switch (op)
            {
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
            ProfileUrl = "/img/profiles/user1.svg",
        }, "p@55wOrd");

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            DisplayName = "Test Employee",
            Email = "employee@email.com",
            UserName = "employee@email.com",
            FirstName = "Test",
            LastName = "Employee",
            EmailConfirmed = true,
            ProfileUrl = "/img/profiles/user2.svg",
        }, "p@55wOrd", AppRoles.Employee);

        await EnsureUserAsync(userManager, new ApplicationUser
        {
            DisplayName = "Test Manager",
            Email = "manager@email.com",
            UserName = "manager@email.com",
            FirstName = "Test",
            LastName = "Manager",
            EmailConfirmed = true,
            ProfileUrl = "/img/profiles/user3.svg",
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

    private static Task SeedReferenceDataAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        // Booking/Coupon seed data removed — replaced by Forecasting domain.
        // Forecast definitions are created by users through the UI.
        return Task.CompletedTask;
    }
}
