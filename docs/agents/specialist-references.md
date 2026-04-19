# Specialist Reference Index

Quick links for future Claude sessions to find authoritative guidance fast.

## When you need...

| If you're doing… | Read first |
|---|---|
| Anything involving render modes, prerendering, persistent state | [Blazor .NET 10 reference](../research/blazor-net10-reference.md) |
| Adding a `MudDataGrid`, `MudDialog`, `MudForm` | [MudBlazor 9 reference](../research/mudblazor-9-reference.md) |
| Writing EF Core queries, migrations, indexes | [EF Core + SQL Server reference](../research/efcore-sqlserver-reference.md) |
| Authentication, authorization, antiforgery | [Identity + auth reference](../research/identity-auth-reference.md) |
| Designing a new endpoint, service, DTO | [ServiceStack-style patterns](../patterns/servicestack-style-patterns.md) |
| Understanding the project shape | [Application architecture](../architecture/application-architecture.md) |
| Following file/class/naming conventions | [Conventions](../architecture/conventions.md) |
| Picking the next thing to build | [Feature backlog](../features/backlog.md) |
| Reviewing recent code | [Code review report](../reviews/code-review-2026-04-13.md) |

## Common tasks → checklists

### Adding a new entity (CRUD page)

1. Read `docs/architecture/conventions.md` for file naming
2. Create entity in `Data/Entities/` with `[Table("...")]`, properties, `[MaxLength]`
3. Add `DbSet<>` in `ApplicationDbContext`
4. Configure in `OnModelCreating` (indexes, relationships)
5. If new table, decide: migration vs runtime diffing (most are runtime — see `EnsureMissingTablesAsync`)
6. Create DTOs in `Models/` (`{Entity}Dto`, `Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Mappings`)
7. Create validators in `Validators/` (`Create{Entity}Validator`, `Update{Entity}Validator`)
8. Create audit log entity (`{Entity}AuditLog`) and audit service (`{Entity}AuditService`)
9. Create endpoints in `Endpoints/` following the 6-handler pattern
10. Register endpoints in `EndpointMappingExtensions.cs`
11. Create Razor pages: `Index.razor`, `History.razor`, `{Entity}Dialog.razor`
12. Add nav link to `NavMenu.razor`
13. Add tests: GET smoke test in `ApiSmokeTests.cs` + form POST test if needed

### Adding a new minimal API endpoint

1. Read [ServiceStack-style patterns](../patterns/servicestack-style-patterns.md)
2. Use `MapGroup("/api/...").RequireAuthorization("ApiOrCookie")`
3. Inject `ApplicationDbContext` (scoped) for endpoint handlers — NOT `IDbContextFactory`
4. Validate with FluentValidation: `IValidator<TRequest>` parameter
5. Use `AsNoTracking()` on read paths
6. Wrap entity+audit writes in `AuditedSaveExtensions.AddWithAuditAsync` (preferred) or transaction
7. Return `TypedResults.Ok(...)`, `TypedResults.NotFound()`, `TypedResults.ValidationProblem(...)`
8. Add a smoke test to `ApiSmokeTests.cs` if it's an `/api/aw/*` endpoint

### Adding a new Blazor page

1. Decide render mode: read [Blazor .NET 10 reference §1](../research/blazor-net10-reference.md#1-render-modes)
2. If it writes cookies/redirects → static SSR + `[ExcludeFromInteractiveRouting]`
3. Otherwise → inherits Interactive Server from `App.razor`
4. Inject `IDbContextFactory<ApplicationDbContext>`, NOT a scoped `ApplicationDbContext`
5. Forms: in static SSR, use `<InputText>` etc. — NEVER MudBlazor inputs
6. Re-init `[SupplyParameterFromForm]` properties in `OnInitialized`
7. Add `aria-label` to every icon-only button
8. Add page to `NavMenu.razor` if user-discoverable

### Investigating a security concern

1. Check [Identity + auth reference](../research/identity-auth-reference.md) for the right pattern
2. Verify the endpoint has `RequireAuthorization` (not just `[Authorize]` on the page)
3. Check if rate limiting applies (auth pages → `[EnableRateLimiting("auth")]`)
4. Check if antiforgery is bypassed (`.DisableAntiforgery()`) and if so why
5. Check the relevant CSP entry in `SecurityHeadersMiddleware.cs`

### Diagnosing a Blazor render issue

1. Read [Blazor .NET 10 reference §8 (cheat sheet)](../research/blazor-net10-reference.md#8-cheat-sheet--common-pitfalls)
2. Common: form fields not POSTing → MudBlazor inputs in SSR form
3. Common: state flickering → use `[PersistentState]`
4. Common: HttpContext null in event handler → page should be `[ExcludeFromInteractiveRouting]`

### Debugging slow EF Core queries

1. Read [EF Core reference §2 (Query patterns)](../research/efcore-sqlserver-reference.md)
2. Add `AsNoTracking()` if read-only
3. Check for N+1 (loops calling navigation properties)
4. Check the index strategy in `ApplicationDbContext.OnModelCreating`
5. Use `.AsSplitQuery()` for big Include trees

## Important codebase facts (avoid relearning)

- **Database**: SQL Server `ELITE / AdventureWorks2022(_dev)`, no SQLite anywhere
- **Most tables are NOT in EF migrations** — they're created via runtime model diffing in `DatabaseInitializer.EnsureMissingTablesAsync`
- **3 actual migrations exist**: InitialSchema, AddApiKeys, AddToolSlotAuditLogs (see `MigrationMarkers` array)
- **`dbo.ToolSlotConfigurations` is DBA-owned** — `ExcludeFromMigrations()`, manual `[Column]` mapping
- **All Identity pages** must declare `@attribute [ExcludeFromInteractiveRouting]` (not inherited)
- **Tests run against real SQL Server `AdventureWorks2022_dev` on ELITE** — no in-memory DB
- **350 tests pass** as of `28edde4`. Don't merge if tests fail.
- **Commit policy**: end every commit body with `Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>`
- **Branch policy**: never commit directly to main/master; always work on a branch and merge back

## Tooling notes

- Build: `dotnet build AWBlazorApp.slnx`
- Test: `dotnet test AWBlazorApp.slnx`
- Test single: `dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~Login_Form_Post"`
- Run: `dotnet run --project src/AWBlazorApp` (https://localhost:5001)
- EF migrations: `cd src/AWBlazorApp && dotnet ef migrations add <Name>` — but read [EF Core reference §5](../research/efcore-sqlserver-reference.md) first; most schema changes go through DatabaseInitializer instead

## Where things live

| Need to find… | Look here |
|---|---|
| All endpoints | `src/AWBlazorApp/Endpoints/` |
| All DTOs | `src/AWBlazorApp/Models/` |
| All entities | `src/AWBlazorApp/Data/Entities/` |
| All validators | `src/AWBlazorApp/Validators/` |
| All Razor pages | `src/AWBlazorApp/Components/Pages/` |
| All shared components | `src/AWBlazorApp/Components/Shared/` |
| All services | `src/AWBlazorApp/Services/` |
| Service registration | `src/AWBlazorApp/Startup/ServiceRegistration.cs` |
| Middleware pipeline | `src/AWBlazorApp/Startup/MiddlewarePipeline.cs` |
| Database initialization | `src/AWBlazorApp/Data/DatabaseInitializer.cs` |
| Authentication handlers | `src/AWBlazorApp/Authentication/` |
| Tests | `AWBlazorApp.Tests/` |
| Documentation | `docs/` (this folder) |
