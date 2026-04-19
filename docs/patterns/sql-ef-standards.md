# SQL Server + EF Core Standards

The agreed conventions for this codebase. Where this document and the official EF Core docs disagree, this document wins.

For the underlying capabilities, see [`/docs/research/efcore-sqlserver-reference.md`](../research/efcore-sqlserver-reference.md).

## DbContext usage

### In Blazor components → `IDbContextFactory`

```csharp
@inject IDbContextFactory<ApplicationDbContext> DbFactory

@code {
    private async Task LoadDataAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var rows = await db.Addresses.AsNoTracking().ToListAsync();
        // ...
    }
}
```

### In endpoints → scoped `ApplicationDbContext`

```csharp
private static async Task<Ok<List<AddressDto>>> ListAsync(
    ApplicationDbContext db, CancellationToken ct)
{
    var rows = await db.Addresses.AsNoTracking().Select(a => a.ToDto()).ToListAsync(ct);
    return TypedResults.Ok(rows);
}
```

### In services → use `IDbContextFactory` if singleton, scoped DbContext if scoped

```csharp
public sealed class LookupService(IDbContextFactory<ApplicationDbContext> dbFactory)  // singleton
{
    public async Task<List<AddressTypeDto>> GetAddressTypesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.AddressTypes.AsNoTracking().Select(a => a.ToDto()).ToListAsync();
    }
}
```

**Why the split?** Blazor components live for the duration of a SignalR circuit (could be hours), but `DbContext` should live for one logical operation. `IDbContextFactory` gives you a fresh `DbContext` per call. Endpoints have a request-scoped lifetime that already matches `DbContext`, so scoped injection works.

## Querying

### Always use `AsNoTracking()` on read paths

```csharp
var rows = await db.Addresses.AsNoTracking().ToListAsync();    // ✅
var rows = await db.Addresses.ToListAsync();                    // ❌ wasteful
```

Skip if the next thing you do is modify and save. But for "load → display → discard" flows, AsNoTracking is non-negotiable.

### Project to DTOs in the query

```csharp
var rows = await db.Addresses.AsNoTracking()
    .Select(a => new AddressDto(a.Id, a.Line1, a.City))
    .ToListAsync();
```

vs.

```csharp
var entities = await db.Addresses.AsNoTracking().ToListAsync();
var rows = entities.Select(a => a.ToDto()).ToList();   // wasteful — loaded all columns
```

The first version generates a tighter SELECT.

### Pagination

```csharp
var query = db.Addresses.AsNoTracking().OrderBy(x => x.Id);
var total = await query.CountAsync();
var page = await query.Skip(skip).Take(take).Select(x => x.ToDto()).ToListAsync();
return new PagedResult<AddressDto>(page, total, skip, take);
```

For very large tables (>1M rows) where deep pages are common, switch to keyset pagination:

```csharp
var page = await db.Addresses
    .AsNoTracking()
    .Where(x => x.Id > lastSeenId)
    .OrderBy(x => x.Id)
    .Take(pageSize)
    .ToListAsync();
```

### Includes vs split queries

```csharp
// Single query with JOIN — fine for 1-2 includes on small datasets
await db.Orders.Include(o => o.LineItems).ToListAsync();

// Split into multiple queries — better for big include trees
await db.Orders.Include(o => o.LineItems).Include(o => o.Customer)
    .AsSplitQuery()
    .ToListAsync();
```

Cartesian explosion: `.Include(a).Include(b)` produces `count(a) * count(b)` rows. Use AsSplitQuery to avoid it.

### Avoid N+1

```csharp
// ❌ N+1
foreach (var u in users)
{
    var roles = await UserManager.GetRolesAsync(u);  // 1 query per user
    Render(u, roles);
}

// ✅ single batched query
var userIds = users.Select(u => u.Id).ToList();
var rolesByUser = await db.UserRoles
    .Where(ur => userIds.Contains(ur.UserId))
    .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name })
    .GroupBy(x => x.UserId)
    .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleName).ToList());

foreach (var u in users)
{
    var roles = rolesByUser.GetValueOrDefault(u.Id) ?? new();
    Render(u, roles);
}
```

## Writing

### Audit + entity write atomicity

Use `AuditedSaveExtensions` for new code:

```csharp
await db.AddWithAuditAsync(
    entity,
    e => AddressAuditService.RecordCreate(e, user.Identity?.Name),
    ct);
```

For existing code that already uses `BeginTransactionAsync`, leave it (the result is the same; migration is mechanical follow-up work).

### Bulk operations

For bulk update/delete without loading entities:

```csharp
// Delete old logs
await db.RequestLogs
    .Where(r => r.Timestamp < DateTime.UtcNow.AddDays(-30))
    .ExecuteDeleteAsync(ct);

// Bulk update
await db.Forecasts
    .Where(f => f.Status == "Pending" && f.CreatedDate < cutoff)
    .ExecuteUpdateAsync(s => s.SetProperty(f => f.Status, "Expired"), ct);
```

**Caveat:** `ExecuteDeleteAsync` / `ExecuteUpdateAsync` bypass the change tracker — `AuditingInterceptor` does NOT fire. Don't use them for entities that need audit logs unless you write the audit row manually first.

## Migrations

### Most schema changes go through DatabaseInitializer, not migrations

Because the codebase uses runtime model diffing for most tables (only 3 actual EF migrations), generating a new migration via `dotnet ef migrations add` will re-include every model-only table. Don't do this unless you're adding a brand-new entity that's not yet in the model snapshot.

### Adding a new EF-managed table

If you really need a migration:
1. `cd src/AWBlazorApp && dotnet ef migrations add YourMigrationName`
2. Inspect the generated migration file — if it includes more than your changes, regenerate from a fresh snapshot
3. Add an entry to `DatabaseInitializer.MigrationMarkers` array with the marker table name
4. Update `ApplicationDbContextModelSnapshot.cs` (auto-generated)

### Adding indexes only

Use `DatabaseInitializer.EnsureCompositeIndexesAsync` — append a new entry with idempotent CREATE INDEX SQL:

```csharp
var indexes = new[]
{
    ("IX_YourTable_YourColumns",
     "CREATE NONCLUSTERED INDEX [IX_YourTable_YourColumns] " +
     "ON [dbo].[YourTable]([Col1], [Col2])"),
    // ...
};
```

The `IF NOT EXISTS` wrapper is added automatically.

### Adding nullable columns

If you add a nullable column to an existing entity, `PatchMissingColumnsAsync` will ALTER TABLE ADD it on next startup. No migration required.

### Adding NOT NULL columns

These need explicit handling. Either:
1. Add as nullable, backfill via SQL, then make NOT NULL via migration
2. Coordinate a downtime to add via migration with a default value

## Index design

### Composite columns: most-selective first

```sql
-- Good if WHERE TenantId = X AND CreatedDate > Y is the common query
CREATE INDEX IX_X_TenantId_CreatedDate ON dbo.X(TenantId, CreatedDate);

-- Bad if TenantId is highly selective
CREATE INDEX IX_X_CreatedDate_TenantId ON dbo.X(CreatedDate, TenantId);
```

### Included columns for covering queries

```sql
CREATE INDEX IX_Orders_Status ON dbo.Orders(Status) INCLUDE (CustomerName, Total);
```

### Filtered indexes for sparse data

```sql
CREATE INDEX IX_Orders_PendingOnly ON dbo.Orders(CreatedDate)
    WHERE Status = 'Pending';
```

Useful when most rows have one value but you query the rare ones.

## Connection strings

Live in `appsettings.json` (dev override in `appsettings.Development.json`). Production uses Trusted_Connection or User Secrets/env vars.

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=ELITE;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=True"
}
```

For high-concurrency apps, append:
```
;Min Pool Size=20;Max Pool Size=200;
```

## Anti-patterns

- ❌ Loading entities just to project to DTO afterward (project in the query)
- ❌ Forgetting `AsNoTracking()` on read paths
- ❌ N+1 queries via foreach over navigation properties
- ❌ `ExecuteUpdate`/`ExecuteDelete` on audited entities (skips interceptor)
- ❌ `Include` chains 3+ deep without `AsSplitQuery`
- ❌ Generating migrations for index-only changes (re-includes everything)
- ❌ Hardcoded `Dispose()` calls on `DbContext` (use `await using`)
- ❌ Sharing a single `DbContext` across multiple async operations (not thread-safe)
