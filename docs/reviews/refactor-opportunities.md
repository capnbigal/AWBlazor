# Refactor Opportunities

Concrete, actionable refactors identified during the comprehensive review. Each entry has: location, problem, recommendation, effort estimate.

## H1. Generic CRUD endpoint helper

**Location:** `AWBlazorApp/Endpoints/AdventureWorks/*.cs` (67 files, ~7200 LOC total)

**Problem:** All 67 endpoint files are nearly identical. They map 6 handlers (List/Get/Create/Patch/Delete/History), use the same authorization, the same transaction pattern, the same DTO mapping shape. The only variations are entity type, DTO names, and audit service.

**Recommendation:** Extract:

```csharp
// In Endpoints/CrudEndpointBuilder.cs (new file)
public static class CrudEndpointBuilder
{
    public static IEndpointRouteBuilder MapCrudEndpoints<TEntity, TDto, TCreate, TUpdate, TAuditLog>(
        this IEndpointRouteBuilder app,
        string routePrefix,
        string tag,
        Func<TEntity, TDto> toDto,
        Func<TCreate, TEntity> toEntity,
        Action<TUpdate, TEntity> applyUpdate,
        Func<TEntity, string?, TAuditLog> recordCreate,
        Func<object, TEntity, string?, TAuditLog> recordUpdate,
        Func<TEntity, string?, TAuditLog> recordDelete,
        Func<TEntity, object> snapshot)
        where TEntity : class
        where TAuditLog : class
    { /* ... */ }
}

// AddressEndpoints.cs collapses to:
public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
{
    return app.MapCrudEndpoints<Address, AddressDto, CreateAddressRequest, UpdateAddressRequest, AddressAuditLog>(
        routePrefix: "/api/aw/addresses",
        tag: "Addresses",
        toDto: a => a.ToDto(),
        toEntity: r => r.ToEntity(),
        applyUpdate: (r, e) => r.ApplyTo(e),
        recordCreate: (e, by) => AddressAuditService.RecordCreate(e, by),
        recordUpdate: (s, e, by) => AddressAuditService.RecordUpdate((AddressSnapshot)s, e, by),
        recordDelete: (e, by) => AddressAuditService.RecordDelete(e, by),
        snapshot: e => AddressAuditService.CaptureSnapshot(e));
}
```

**Estimate:** Medium (2-3 days). Build helper + migrate one entity for verification + bulk-migrate the rest with a script.

**Risk:** High value, low risk if migrated incrementally and tested.

## H2. Migrate to `AuditedSaveExtensions` everywhere

**Location:** All 67 endpoints + 65 dialogs that currently use inline `BeginTransactionAsync`/`CommitAsync`

**Problem:** Phase 2 wrapped audit writes in transactions via inline code. The cleaner `AuditedSaveExtensions` helper was added in Phase 4 but existing code wasn't migrated. Result: the codebase has two patterns for the same thing.

**Recommendation:** Mechanical find-and-replace:
- `await using var tx = await db.Database.BeginTransactionAsync(ct); db.X.Add(entity); await db.SaveChangesAsync(ct); db.YAuditLogs.Add(...); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);`
- → `await db.AddWithAuditAsync(entity, e => YAuditService.RecordCreate(e, user.Identity?.Name), ct);`

**Estimate:** Small (4-6 hours with sed/perl + manual verification).

## H3. Extract `DialogSaveHelper`

**Location:** `Components/Pages/AdventureWorks/*/[Entity]Dialog.razor` (~80 files)

**Problem:** Every dialog has a 60-line `SaveAsync` method with the same shape: validate → optional FK check → BeginTransaction → save entity → save audit → commit → catch DbUpdateException → close.

**Recommendation:**
```csharp
// Components/Shared/DialogSaveHelper.cs
public static class DialogSaveHelper
{
    public static async Task<bool> SaveAndAuditAsync<TEntity, TAuditLog>(
        ApplicationDbContext db,
        TEntity entity,
        Func<TEntity, TAuditLog> auditFunc,
        ISnackbar snackbar)
        where TEntity : class where TAuditLog : class
    { /* ... */ }
}
```

Each dialog's SaveAsync becomes 10 lines.

**Estimate:** Medium (1-2 days). Extract helper + migrate one dialog as POC + roll out.

## M1. Generic grid sort

**Location:** `Components/Pages/AdventureWorks/*/Index.razor` (~40 files)

**Problem:** Every list page has a switch statement mapping 5-8 sort field names to `OrderBy`/`OrderByDescending` calls. ~12 lines per page × 40 pages.

**Recommendation:**
```csharp
public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, SortDefinition<T>? sort, Expression<Func<T, object>> defaultSort)
{
    if (sort is null) return query.OrderBy(defaultSort);
    var param = Expression.Parameter(typeof(T), "x");
    var prop = Expression.Property(param, sort.SortBy);
    var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(prop, typeof(object)), param);
    return sort.Descending ? query.OrderByDescending(lambda) : query.OrderBy(lambda);
}
```

**Estimate:** Small (4 hours).

## M2. Validation pipeline filter

**Location:** Every endpoint's `CreateAsync`/`UpdateAsync` handler

**Problem:** Repeated:
```csharp
var v = await validator.ValidateAsync(request, ct);
if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
```

**Recommendation:** `AddEndpointFilter` per group:
```csharp
group.AddEndpointFilter<FluentValidationFilter>();
```

The filter resolves the validator from DI based on the request's parameter type.

**Estimate:** Small (3 hours, including refactoring).

## M3. ARIA labels audit

**Location:** All Razor pages with `MudIconButton`

**Problem:** Icon-only buttons lack `aria-label`. Screen readers announce "button" with no context.

**Recommendation:** Find-and-replace pass:
- Wrap in `<MudTooltip Text="...">` if hover hint also useful
- Or add `aria-label="..."` directly

**Estimate:** Small (4 hours for all pages).

## M4. Loading state on list pages

**Location:** All `Index.razor` pages

**Problem:** No loading indicator during `LoadServerDataAsync`. Just a brief blank.

**Recommendation:** Wrap grid in:
```razor
@if (loading) { <MudProgressLinear Indeterminate="true" Class="mb-2" /> }
<MudDataGrid ... />
```

Or use the grid's built-in `Loading` state if available in the version.

**Estimate:** Small (2 hours).

## M5. Activity dashboard split

**Location:** `Components/Pages/Admin/Activity.razor` (454 lines)

**Problem:** Single component with charts + table + filters. Hard to navigate, hard to test, hard to reuse.

**Recommendation:** Split into:
- `<ActivityCharts ChartData="@..." />` (~150 lines)
- `<ActivityTable Items="@..." />` (~150 lines)
- `<ActivityFilters @bind-Filter="@..." />` (~80 lines)
- Activity.razor becomes a composition (~80 lines)

**Estimate:** Medium (1 day).

## L1. Magic strings → constants

**Location:** `ProductDialog.razor:62-82` and similar

**Problem:** Hardcoded product line/class/style codes scattered across dialog markup.

**Recommendation:** `Data/ProductConstants.cs`:
```csharp
public static class ProductConstants
{
    public static readonly (string Code, string Label)[] ProductLines =
        [("R", "R — Road"), ("M", "M — Mountain"), /* ... */];
}
```

**Estimate:** Small (2 hours).

## L2. Lookup cache TTL upgrade

**Location:** `Services/LookupService.cs`

**Problem:** 5-minute TTL is too aggressive for true reference data (`AddressTypes`, `Cultures`, `Currencies`).

**Recommendation:** Bump to 1 hour. Add `/api/admin/clear-lookup-cache` endpoint for admins to bust manually.

**Estimate:** Small (2 hours).

## L3. Doc CLAUDE.md gaps

**Location:** `CLAUDE.md`

**Problem:** Doesn't document the standard endpoint shape or dialog SaveAsync shape, even though every new entity should follow them.

**Recommendation:** Add sections after §8 (Audited writes):
- §9. Standard endpoint pattern (link to `docs/architecture/conventions.md`)
- §10. Standard dialog pattern (link to same)

**Estimate:** Small (1 hour).

## Future ideas (don't act on yet)

### F1. Source-generated audit services
Roslyn source generator that produces `{Entity}AuditService.RecordCreate/Update/Delete` from a `[Auditable]` attribute on the entity. Cuts ~70 boilerplate files but requires a generator project.

### F2. Custom `[RequireAppRole]` attribute
`[RequireAppRole(AppRoles.Employee, AppRoles.Manager, AppRoles.Admin)]` wrapping the policy chain. Reduces per-endpoint authorization boilerplate but adds a layer of indirection.

### F3. `Result<T>` envelope
Standard response shape for all endpoints. Defer until external API consumers exist who care about contract consistency.

### F4. bUnit component tests
Currently zero unit tests for components. Start with shared components (`KpiCard`, `EmptyState`, `GlobalSearch`).

### F5. Mapperly source-generated mappers
Replace hand-written `ToDto`/`ToEntity` with Mapperly attributes. Faster + less boilerplate but adds a code generation dependency.

---

## Order of operations

If we tackle all of this:

1. **Week 1:** H2 (mechanical migration to `AuditedSaveExtensions`), L3 (CLAUDE.md updates), L1 (constants)
2. **Week 2:** H1 (generic CRUD helper) + verify with one entity
3. **Week 3:** Roll out H1 to remaining entities (parallelize)
4. **Week 4:** H3 (DialogSaveHelper) + M1 (generic grid sort)
5. **Week 5:** M2 (validation filter) + M4 (loading states)
6. **Week 6:** M3 (ARIA audit) + M5 (Activity split) + L2 (cache TTL)
7. **Future:** F1-F5 as priorities allow
