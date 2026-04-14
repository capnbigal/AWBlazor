# EF Core 10 + SQL Server Reference

A pragmatic patterns cheat sheet for senior developers building on EF Core 10 (LTS, released Nov 2025, supported until Nov 2028), .NET 10, and SQL Server. Code-heavy; assumes you already know what an `IQueryable` is.

Sources: Microsoft Learn (EF Core 10 "What's new", Performance overview, Efficient querying, Efficient updating, DbContext configuration, Migrations overview, SQL Server indexes).

---

## 1. EF Core 10 features worth using

### 1.1 SQL Server 2025 / Azure SQL: native `vector` and `json` types

EF 10 adds first-class support for the SQL Server 2025 `vector` and `json` data types.

```csharp
public class Blog
{
    public int Id { get; set; }
    public string[] Tags { get; set; } = [];            // maps to json column
    public required BlogDetails Details { get; set; }   // complex-type -> json column

    [Column(TypeName = "vector(1536)")]
    public SqlVector<float> Embedding { get; set; }     // vector type
}

protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<Blog>().ComplexProperty(x => x.Details, d => d.ToJson());
}

// Vector similarity search
var top = await ctx.Blogs
    .OrderBy(b => EF.Functions.VectorDistance("cosine", b.Embedding, queryVector))
    .Take(3)
    .ToListAsync();

// LINQ through JSON complex type
var hot = await ctx.Blogs.Where(b => b.Details.Viewers > 3).ToListAsync();
// -> WHERE JSON_VALUE([b].[Details], '$.Viewers' RETURNING int) > 3
```

If compatibility level is 170+ (or `UseAzureSql`), EF auto-migrates existing `nvarchar(max)` JSON columns to the native `json` type. Opt out by explicitly mapping `nvarchar(max)`.

### 1.2 `ExecuteUpdateAsync` improvements

Now accepts a regular (non-expression) lambda and can target properties inside JSON complex types:

```csharp
await ctx.Blogs.ExecuteUpdateAsync(s =>
{
    s.SetProperty(b => b.Views, 8);
    if (nameChanged) s.SetProperty(b => b.Name, "foo");   // conditional setter
    s.SetProperty(b => b.Details.Views, b => b.Details.Views + 1); // into JSON
});
```

### 1.3 Named query filters (finally)

Lets you attach multiple filters and disable them selectively. Essential for combining soft-delete with multi-tenancy:

```csharp
modelBuilder.Entity<Blog>()
    .HasQueryFilter("SoftDelete", b => !b.IsDeleted)
    .HasQueryFilter("Tenant",     b => b.TenantId == _tenantId);

// disable only soft-delete, keep tenant filter
var all = await ctx.Blogs.IgnoreQueryFilters(["SoftDelete"]).ToListAsync();
```

### 1.4 `LeftJoin` / `RightJoin` LINQ operators

.NET 10 adds first-class `LeftJoin`; EF 10 translates it. Replaces the old `SelectMany` + `GroupJoin` + `DefaultIfEmpty` pattern.

```csharp
var q = ctx.Students.LeftJoin(
    ctx.Departments,
    s => s.DepartmentID,
    d => d.ID,
    (s, d) => new { s.FirstName, Department = d.Name ?? "[NONE]" });
```

### 1.5 Parameterized collections: new default translation

Pre-EF10, `ids.Contains(x)` was translated via `OPENJSON` over a single JSON parameter. EF 10 now emits one scalar parameter per element (with padding to reduce plan variants):

```sql
WHERE [b].[Id] IN (@ids1, @ids2, @ids3, @ids4 /* padded */)
```

Override per-query or globally when needed:

```csharp
// per-query: inline as constants (best for tiny, stable sets like enum-like role names)
ctx.Users.Where(u => EF.Constant(roles).Contains(u.Role));

// global: force OPENJSON mode (better for large/variable-length collections)
opt.UseSqlServer(cs, o => o.UseParameterizedCollectionMode(ParameterTranslationMode.MultipleParameters));
```

### 1.6 Complex types are now the right choice (not owned entities)

Complex types now support optional (nullable), struct, JSON mapping, `ExecuteUpdate`, and value-semantics assignment. Migrate owned entity types to complex types unless you need identity.

```csharp
b.ComplexProperty(c => c.ShippingAddress);                 // table splitting
b.ComplexProperty(c => c.BillingAddress, c => c.ToJson()); // JSON column
```

### 1.7 Other EF 10 wins

- **Split queries** now preserve ordering across subqueries (fixes a subtle data-corruption class).
- **Redacted logs**: inlined constants logged as `?` unless `EnableSensitiveDataLogging()`.
- **Analyzer warning** on string concatenation into `FromSqlRaw` / `ExecuteSqlRaw`.
- **Default-constraint naming**: `HasDefaultValueSql("GETDATE()", "DF_Post_CreatedDate")` or `modelBuilder.UseNamedDefaultConstraints()`.
- Migrations no longer wrap in a single transaction (reverting an EF 9 change that broke several scenarios).
- Simpler parameter names (`@city` instead of `@__city_0`).
- New translations: `DateOnly.ToDateTime`, `DateOnly.DayNumber`, `DatePart.Microsecond/Nanosecond`, `COALESCE` -> `ISNULL`.

---

## 2. Query patterns

### 2.1 `AsNoTracking` for read-only paths

Tracking allocates a snapshot per entity. For read-only queries skip it:

```csharp
var posts = await ctx.Posts.AsNoTracking().Where(p => p.BlogId == id).ToListAsync();
```

Benchmark (MS docs): ~30% faster, ~40% less allocation for 10 blogs x 20 posts. **Caveat**: no identity resolution - the same row referenced by multiple parents materializes as distinct instances. Use `AsNoTrackingWithIdentityResolution()` if you need dedup without change tracking.

Set default globally when the majority of the app is read-heavy:

```csharp
opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
```

### 2.2 Project only what you need

```csharp
// BAD: pulls every column
foreach (var b in ctx.Blogs) Console.WriteLine(b.Url);

// GOOD: SELECT Url only
await foreach (var url in ctx.Blogs.Select(b => b.Url).AsAsyncEnumerable())
    Console.WriteLine(url);
```

### 2.3 `Include` vs projection vs `AsSplitQuery`

Eager-loading a one-to-many with `Include` produces a JOIN that duplicates the parent row N times ("cartesian explosion"):

```csharp
// Single query - cartesian explosion risk
var blogs = await ctx.Blogs.Include(b => b.Posts).ToListAsync();

// Split query - one roundtrip per collection, no explosion
var blogs = await ctx.Blogs.AsSplitQuery().Include(b => b.Posts).ToListAsync();
```

Rule of thumb: single query for 1:1 / many:1 and small 1:N collections; `AsSplitQuery()` when the child collection is wide or deeply nested. Internal buffering happens when MARS is off (default on SQL Server) - all split-query results except the last are buffered.

Filtered `Include` (EF 5+):

```csharp
ctx.Blogs.Include(b => b.Posts
    .Where(p => p.BlogId == 1).OrderByDescending(p => p.Title).Take(5));
```

**Avoid lazy loading** in hot paths - it silently creates N+1. If you can't predict what's needed, use `context.Entry(blog).Collection(b => b.Posts).LoadAsync()` explicitly.

### 2.4 Buffering vs streaming

```csharp
var list = await q.ToListAsync();                 // buffered
var arr  = await q.ToArrayAsync();                // buffered
await foreach (var x in q.AsAsyncEnumerable()) { } // streamed
```

Never call `.ToList().Where(...)` - it buffers then filters client-side. Use `AsEnumerable()` or `AsAsyncEnumerable()` when you genuinely need client-side ops.

### 2.5 Keyset pagination

Avoid `Skip(n).Take(m)` for deep pages - SQL Server still scans `n` rows. Use keyset pagination:

```csharp
var page = await ctx.Posts
    .Where(p => p.CreatedAt < lastSeenDate
             || (p.CreatedAt == lastSeenDate && p.Id < lastSeenId))
    .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
    .Take(50)
    .ToListAsync();
```

### 2.6 Always prefer `Async` variants

Mixing sync + async on a single `DbContext` is a ticket to thread-pool starvation. Known caveat: `Microsoft.Data.SqlClient` has rough edges on async for large text/binary values - profile if you're doing blob-heavy work.

---

## 3. Bulk operations: `ExecuteUpdateAsync` / `ExecuteDeleteAsync`

Non-tracking single-roundtrip UPDATE/DELETE. No `SaveChanges`. No snapshot. No entity materialization.

```csharp
// Give everyone a raise - one UPDATE, zero allocations
await ctx.Employees.ExecuteUpdateAsync(s =>
    s.SetProperty(e => e.Salary, e => e.Salary + 1000));

// Soft-delete pattern
await ctx.Posts.Where(p => p.CreatedAt < cutoff)
    .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true));

// Hard delete
await ctx.ApiKeys.Where(k => k.RevokedAt < DateTime.UtcNow.AddDays(-90))
    .ExecuteDeleteAsync();
```

SQL emitted:

```sql
UPDATE [Employees] SET [Salary] = [Salary] + 1000;
```

**Gotchas**:
- Does NOT fire `SaveChanges` interceptors or change-tracker events. Audit hooks won't run.
- Does NOT execute in the same transaction as `SaveChangesAsync` unless you wrap both with `ctx.Database.BeginTransactionAsync()`.
- `ExecuteUpdate` + tracked entities: the in-memory state diverges from the DB. Call `ctx.ChangeTracker.Clear()` or reload.
- Can't be combined with `Include` in the same call.

### SaveChanges batching

EF batches automatic `SaveChanges` updates. Default SQL Server range is 4-42 statements per roundtrip. Tune only with benchmarking:

```csharp
opt.UseSqlServer(cs, o => o.MinBatchSize(1).MaxBatchSize(100));
```

---

## 4. DbContext lifetime

### 4.1 The rules

- `DbContext` = one unit of work. Short-lived. **Not thread-safe.** Always `await` before the next operation.
- In ASP.NET Core `AddDbContext` registers scoped; each HTTP request gets its own context, so this is safe.
- **Blazor Server is different**: one DI scope per user *circuit*, which lives as long as the SignalR connection. A scoped `DbContext` can live for hours and accumulate tracked entities. **Use `IDbContextFactory` instead.**

### 4.2 Registration patterns

```csharp
// Standard ASP.NET Core controllers / minimal APIs
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(cs, sql => sql.EnableRetryOnFailure()));

// Blazor Server - factory
builder.Services.AddDbContextFactory<ApplicationDbContext>(o =>
    o.UseSqlServer(cs));

// Both (common real-world case: Identity needs scoped, Blazor needs factory)
builder.Services.AddDbContextFactory<ApplicationDbContext>(o => o.UseSqlServer(cs));
builder.Services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
```

### 4.3 Blazor Server usage

```razor
@inject IDbContextFactory<ApplicationDbContext> DbFactory

@code {
    private async Task LoadAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        _blogs = await db.Blogs.AsNoTracking().ToListAsync();
    }
}
```

Factory-produced contexts are **not** owned by the DI scope - you must dispose them (`await using`).

### 4.4 Connection pooling: `AddDbContextPool`

Pools and reuses `DbContext` instances (via `Reset()`), saving per-request allocation cost. Use only when your `DbContext` has no per-request state beyond what EF resets:

```csharp
builder.Services.AddDbContextPool<ApplicationDbContext>(o => o.UseSqlServer(cs),
    poolSize: 1024);
```

**Don't pool** if: you stash `IHttpContextAccessor`/tenant info in constructor fields, override `OnConfiguring` based on request data, or subscribe to events in the constructor.

SQL Server connection pool is separate (ADO.NET-level, on by default, tuned via connection string `Max Pool Size=100`).

### 4.5 Retry / resiliency

```csharp
o.UseSqlServer(cs, sql => sql.EnableRetryOnFailure(
    maxRetryCount: 5,
    maxRetryDelay: TimeSpan.FromSeconds(30),
    errorNumbersToAdd: null));
```

With retry enabled, EF internally buffers resultsets (to replay consistently) - this defeats streaming. Wrap manual transactions with `ctx.Database.CreateExecutionStrategy().ExecuteAsync(...)`.

---

## 5. Migrations

### 5.1 Standard workflow

```pwsh
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations add AddBlogCreatedTimestamp
dotnet ef migrations script --idempotent --output schema.sql
dotnet ef migrations remove    # undo the last (unmigrated) add
```

Migration files + `ModelSnapshot.cs` go in source control. EF records applied migrations in `__EFMigrationsHistory`.

### 5.2 Production deployment: idempotent SQL scripts

Don't use `database update` in production. Generate an idempotent script and run it via your deploy pipeline:

```pwsh
dotnet ef migrations script --idempotent --output migrate.sql
```

The `--idempotent` flag wraps each migration in `IF NOT EXISTS (SELECT ... FROM __EFMigrationsHistory WHERE MigrationId = ...)` so re-running is safe.

### 5.3 Exclude externally-owned tables

```csharp
modelBuilder.Entity<ToolSlotConfiguration>()
    .ToTable("ToolSlotConfigurations", t => t.ExcludeFromMigrations());
```

EF reads/writes the table but never generates `CREATE`/`ALTER`/`DROP` for it. Coordinate schema changes with whoever owns the table.

### 5.4 Common pitfalls

- **Multiple `DbContext`s that share Identity tables**: exclude the shared tables from one of them (see above).
- **Renaming a property in C# != rename column**: EF sees Drop + Add. Hand-edit the migration to emit `RENAME COLUMN`.
- **Data-preserving transformations**: EF migrations only diff *schema*. Put data transforms in `migrationBuilder.Sql("UPDATE ...")` inside a custom `Up()`.
- **Design-time factory**: if your `DbContext` can't be constructed with a parameterless-ish pattern at design time (e.g. it needs config from DI), implement `IDesignTimeDbContextFactory<ApplicationDbContext>`.
- **EF 10 change**: migrations no longer run inside a single wrapping transaction (reverted from EF 9). Each migration is still atomic on SQL Server due to DDL auto-commit behavior.
- **Production partial state**: if you have a database that was partially built by hand, use `__EFMigrationsHistory` INSERTs to "stamp" already-present migrations as applied before running `update`.

---

## 6. SQL Server index design

### 6.1 Clustered vs nonclustered

- **Clustered**: defines the physical row order (the table IS the index's leaf level). **One per table.** SQL Server creates one automatically for the `PRIMARY KEY` unless one already exists.
- **Nonclustered**: separate B+ tree, leaf pages contain the key + a row locator (the clustered key, or a RID for heaps). Up to 999 per table.

Rule: clustered key should be narrow, ever-increasing (minimizes page splits), unique, and frequently used in range queries. `IDENTITY INT` or `bigint` PK is the usual default. Avoid wide composite clustered keys - every nonclustered index carries that key as its row locator.

### 6.2 Composite indexes and column order

```sql
CREATE NONCLUSTERED INDEX IX_Orders_CustomerId_OrderDate
    ON dbo.Orders (CustomerId, OrderDate DESC);
```

Column order matters: this index covers `WHERE CustomerId = @c`, `WHERE CustomerId = @c AND OrderDate > @d`, and `WHERE CustomerId = @c ORDER BY OrderDate DESC`. It does **not** help `WHERE OrderDate > @d` alone - the leading column must appear in the predicate for a seek.

EF Core:

```csharp
modelBuilder.Entity<Order>()
    .HasIndex(o => new { o.CustomerId, o.OrderDate })
    .IsDescending(false, true);
```

### 6.3 Included (covering) columns

Included columns are stored at the leaf level but not in the key. Use them to make an index "covering" so the query plan skips the key lookup back to the table:

```sql
CREATE NONCLUSTERED INDEX IX_Orders_CustomerId_Covering
    ON dbo.Orders (CustomerId)
    INCLUDE (OrderDate, TotalAmount, Status);
```

EF Core:

```csharp
modelBuilder.Entity<Order>()
    .HasIndex(o => o.CustomerId)
    .IncludeProperties(o => new { o.OrderDate, o.TotalAmount, o.Status });
```

Trade-off: larger index pages, slower writes. Only cover hot `SELECT` patterns.

### 6.4 Filtered indexes

Index only a subset of rows. Great for soft-delete and sparse flags:

```sql
CREATE NONCLUSTERED INDEX IX_Users_Active_Email
    ON dbo.Users (Email)
    WHERE IsDeleted = 0;
```

EF Core:

```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .HasFilter("[IsDeleted] = 0");
```

Much smaller, faster, and reduces write amplification since deleted rows don't update the index.

### 6.5 Index design rules

- Indexes speed reads, slow writes. Every INSERT/UPDATE/DELETE pays per affected index.
- Prefer narrow keys. The clustered key appears inside every nonclustered index.
- SARGability: `WHERE Name LIKE 'A%'` uses an index; `WHERE Name LIKE '%A'` does not; `WHERE YEAR(CreatedAt) = 2025` does not. Use half-open ranges: `WHERE CreatedAt >= '2025-01-01' AND CreatedAt < '2026-01-01'`.
- Functions over columns: create a **persisted computed column** and index it, or use SQL Server's expression indexes (limited support).
- Check `sys.dm_db_index_usage_stats` for unused indexes; drop them. Check `sys.dm_db_missing_index_details` for query-planner suggestions.
- `PRIMARY KEY` -> clustered (default) + unique; `UNIQUE` -> nonclustered unique.

---

## 7. Auditing patterns: EF Core interceptors

Interceptors are EF 10's clean hook for cross-cutting concerns like audit logging. Register via options:

```csharp
opt.UseSqlServer(cs).AddInterceptors(new AuditingSaveChangesInterceptor());
```

### 7.1 `SaveChanges` interceptor for stamping created/modified

```csharp
public sealed class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> r, CancellationToken ct = default)
    {
        var ctx = e.Context;
        if (ctx is null) return new(r);

        var now = DateTime.UtcNow;
        foreach (var entry in ctx.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = _currentUser.Id;
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = _currentUser.Id;
            }
        }
        return new(r);
    }
}
```

### 7.2 Writing audit-trail rows from the interceptor

For full before/after diffs, snapshot `entry.OriginalValues` vs `entry.CurrentValues` and emit a row into an `AuditLog` table inside the same `SaveChanges` batch. Swap `SavedChangesAsync` if you need the generated PK of a newly-inserted row.

**Gotcha**: `ExecuteUpdateAsync` / `ExecuteDeleteAsync` bypass the change tracker and therefore bypass this interceptor. If your audit policy must cover bulk ops, either wrap them with explicit audit inserts or fall back to SQL Server triggers / Change Data Capture / temporal tables.

### 7.3 Temporal tables (SQL Server native)

For "free" auditing, use SQL Server system-versioned temporal tables. EF Core supports them:

```csharp
modelBuilder.Entity<Order>().ToTable("Orders", b => b.IsTemporal());

// Query history
var history = await ctx.Orders
    .TemporalAll()
    .Where(o => o.Id == id)
    .OrderBy(o => EF.Property<DateTime>(o, "PeriodStart"))
    .ToListAsync();
```

Zero app-code overhead, captured at the DB level regardless of how the row changed (`SaveChanges`, `ExecuteUpdate`, raw SQL, DBA fiddling).

---

## 8. Soft-delete patterns

### 8.1 Global query filter + boolean flag

```csharp
public interface ISoftDelete { bool IsDeleted { get; set; } }

modelBuilder.Entity<Blog>().HasQueryFilter("SoftDelete", b => !b.IsDeleted);

// Bypass for admin/export pages
var all = await ctx.Blogs.IgnoreQueryFilters(["SoftDelete"]).ToListAsync();
```

With EF 10's named filters you can combine soft-delete with tenant/culture filters independently.

### 8.2 Override `SaveChanges` to convert deletes into updates

```csharp
public override int SaveChanges()
{
    foreach (var e in ChangeTracker.Entries<ISoftDelete>())
    {
        if (e.State == EntityState.Deleted)
        {
            e.State = EntityState.Modified;
            e.Entity.IsDeleted = true;
        }
    }
    return base.SaveChanges();
}
```

### 8.3 Filtered unique indexes

Soft-deleted rows usually shouldn't block reusing e.g. a unique email. Use a filtered unique index:

```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email).IsUnique()
    .HasFilter("[IsDeleted] = 0");
```

### 8.4 Index every FK + the soft-delete column

Nonclustered indexes should include the `IsDeleted` column (or be filtered) so the planner can skip deleted rows efficiently.

### 8.5 Purge cadence

Soft-deleted rows still cost storage + index maintenance. Schedule a purge job:

```csharp
await ctx.Blogs.IgnoreQueryFilters()
    .Where(b => b.IsDeleted && b.ModifiedAt < DateTime.UtcNow.AddYears(-1))
    .ExecuteDeleteAsync();
```

---

## 9. Quick-reference checklist

- [ ] Reads use `AsNoTracking` or a `Select` projection.
- [ ] Writes use `ExecuteUpdateAsync` / `ExecuteDeleteAsync` where change tracking isn't needed.
- [ ] Blazor Server components inject `IDbContextFactory<T>`, not `T` directly.
- [ ] Large `Include` chains use `AsSplitQuery()`.
- [ ] Pagination uses keyset, not `Skip/Take`, for deep pages.
- [ ] Every FK has a nonclustered index; hot queries have covering (`.IncludeProperties`) indexes.
- [ ] Soft-delete tables have filtered indexes excluding deleted rows.
- [ ] Production deploys apply `dotnet ef migrations script --idempotent`, never `database update`.
- [ ] `EnableRetryOnFailure` is on for Azure SQL / flaky networks.
- [ ] Audit concerns use `SaveChangesInterceptor` OR SQL Server temporal tables - not both.
- [ ] Nothing uses lazy loading in a request path.
