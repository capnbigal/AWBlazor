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
2. Pick the owning feature folder under `Features/<Feature>/<Entity>/`
3. Create entity in `Features/<Feature>/<Entity>/Domain/` with `[Table("...")]`, properties, `[MaxLength]`
4. Add `DbSet<>` in `ApplicationDbContext`
5. Configure in `OnModelCreating` (indexes, relationships)
6. If new table, decide: migration vs runtime diffing (most are runtime — see `EnsureMissingTablesAsync`)
7. Create DTOs in `Features/<Feature>/<Entity>/Dtos/` (`{Entity}Dto`, `Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Mappings`)
8. Create validators in `Features/<Feature>/<Entity>/Application/Validators/` (`Create{Entity}Validator`, `Update{Entity}Validator`)
9. Create audit log entity (`{Entity}AuditLog` in Domain/) and audit service (`{Entity}AuditService` in Application/Services/)
10. Create endpoints in `Features/<Feature>/<Entity>/Api/` following the 6-handler pattern
11. Register endpoints in `App/Routing/EndpointMappingExtensions.cs`
12. Create Razor pages in `Features/<Feature>/<Entity>/UI/Pages/`: `Index.razor`, `History.razor`, `{Entity}Dialog.razor`
13. Add nav link to `Shared/UI/Layout/NavMenu.razor`
14. Add tests: GET smoke test in `AWBlazorApp.Tests/Infrastructure/Api/ApiSmokeTests.cs` + form POST test under `AWBlazorApp.Tests/Features/<Feature>/Api/` if needed

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
5. Check the relevant CSP entry in `App/Middleware/SecurityHeadersMiddleware.cs`

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
| All endpoints | `src/AWBlazorApp/Features/<Feature>/<Entity>/Api/` (one group per entity) |
| All DTOs | `src/AWBlazorApp/Features/<Feature>/<Entity>/Dtos/` + `src/AWBlazorApp/Shared/Dtos/` |
| All entities | `src/AWBlazorApp/Features/<Feature>/<Entity>/Domain/` |
| All validators | `src/AWBlazorApp/Features/<Feature>/<Entity>/Application/Validators/` + `src/AWBlazorApp/Shared/Validation/` |
| All Razor pages | `src/AWBlazorApp/Features/<Feature>/<Entity>/UI/Pages/` |
| All shared widgets | `src/AWBlazorApp/Shared/UI/Components/` |
| All services | `src/AWBlazorApp/Features/<Feature>/<Entity>/Application/Services/` + `src/AWBlazorApp/Shared/Services/` |
| Service registration | `src/AWBlazorApp/App/Extensions/ServiceRegistration.cs` |
| Middleware pipeline | `src/AWBlazorApp/App/Extensions/MiddlewarePipeline.cs` |
| Endpoint routing | `src/AWBlazorApp/App/Routing/EndpointMappingExtensions.cs` |
| Database initialization | `src/AWBlazorApp/Infrastructure/Persistence/DatabaseInitializer.cs` |
| Authentication handlers | `src/AWBlazorApp/Infrastructure/Authentication/` |
| Tests | `AWBlazorApp.Tests/` (mirrors `Features/` layout — see `Infrastructure/Testing/IntegrationTestFixtureBase.cs` for the base) |
| Documentation | `docs/` (this folder) |
