# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build, test, run

```pwsh
dotnet restore AWBlazorApp.slnx
dotnet build   AWBlazorApp.slnx
dotnet test    AWBlazorApp.slnx                        # all 213 tests
dotnet test    AWBlazorApp.slnx --filter "FullyQualifiedName~Login_Form_Post"   # single test by name pattern
dotnet run     --project AWBlazorApp                   # https://localhost:5001
```

EF Core migrations:

```pwsh
cd AWBlazorApp
dotnet ef migrations add <Name>
dotnet ef migrations script --idempotent --output ../schema.sql   # generate idempotent SQL
```

The solution is `AWBlazorApp.slnx` (XML solution file). Two projects: `AWBlazorApp` (web) and `AWBlazorApp.Tests` (NUnit).

## Stack at a glance

.NET 10 + Blazor Web App (mixed Interactive Server + static SSR) + MudBlazor 9 + EF Core 10 (SQL Server) + ASP.NET Core Identity + Hangfire 1.8 + Serilog 10 + FluentValidation 12 + Swashbuckle + Markdig + YamlDotNet + NUnit.

**Databases (all SQL Server on `ELITE`):**
- **Production:** `AdventureWorks2022` — used by the Production environment (`appsettings.json` `DefaultConnection`).
- **Dev / test:** `AdventureWorks2022_dev` — used by the Development environment (`appsettings.Development.json` `DefaultConnection`). Integration tests run against this database via `WebApplicationFactory<Program>` in the Development environment.

**There is no SQLite in this project, in any project, for any purpose.** The `Microsoft.Data.Sqlite` and `Microsoft.EntityFrameworkCore.Sqlite` packages are not referenced anywhere; the non-SQL-Server branch of `DatabaseInitializer.InitializeAsync` has been removed and now throws if the provider isn't SQL Server. **Don't reintroduce SQLite.**

**Don't reintroduce ServiceStack, Vue, Tailwind, or any NPM toolchain.** This project came from a ServiceStack + Vue template and the entire migration was about removing those. See `memory/project_migration_status.md` and `docs/phase-plan.md` for the locked-in technology decisions.

## Critical architectural patterns

These are not obvious from reading individual files — they were learned the hard way during the migration and are documented in the README "Architecture notes" section, but they're important enough to repeat here.

### 1. Conditional render mode (interactive vs. static SSR)

`Components/App.razor` chooses the render mode per request:

```razor
<HeadOutlet @rendermode="@PageRenderMode" />
<Routes @rendermode="@PageRenderMode" />

@code {
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    private IComponentRenderMode? PageRenderMode
        => HttpContext.AcceptsInteractiveRouting() ? InteractiveServer : null;
}
```

- Most pages render with **InteractiveServer** so MudBlazor's interactive features (dialogs, snackbars, MudDataGrid sorting/pagination) work.
- Identity scaffold pages each declare `@attribute [ExcludeFromInteractiveRouting]`, which makes `AcceptsInteractiveRouting()` return false → those pages render as **static SSR**. This is required for `SignInManager` cookie writes to work.

**When adding a new Identity / Manage page**, always include `@attribute [ExcludeFromInteractiveRouting]` directly on the page (don't rely on `_Imports.razor` inheritance — it's flaky and the .NET 9 standard template doesn't use it either).

### 2. MudBlazor inputs DO NOT WORK in static SSR forms

This is the biggest single gotcha. **Never** use `MudTextField`, `MudCheckBox`, `MudSelect`, etc. inside an `<EditForm method="post">` on a page marked `[ExcludeFromInteractiveRouting]`. They don't emit the HTML `name` attribute the SSR form binder needs, so every form POST will fail validation with "field required" — the model values never make it to the server.

**Use Blazor's built-in `<InputText>`, `<InputCheckbox>`, `<InputSelect>` inside SSR forms.** They auto-derive `name="Input.Email"` from the `@bind-Value` expression. Style them with the shared classes in `wwwroot/css/account-forms.css` (`form-field`, `form-control`, `form-label`, `form-error`, `form-validation-summary`).

`MudPaper`, `MudContainer`, `MudText`, `MudButton`, `MudDivider`, `MudStack`, `MudLink`, `MudAlert`, `MudChip` are fine inside SSR forms — only the actual `<input>`-emitting components are the problem.

Per-row dynamic forms (e.g. revoke buttons in a list, see `ApiKeys.razor`) work via separate `EditForm`s with distinct `FormName`s + a hidden `<input type="hidden" name="RevokeInput.Id" value="@key.Id">`.

### 3. `[SupplyParameterFromForm]` properties get nulled by the binder

In static SSR, the framework's form binder calls the property setter on every render — including on GETs — and sets the property to `null` when there's no matching form data. The field initializer (`= new()`) survives the constructor but gets clobbered before `EditForm.OnParametersSet` runs, which throws "Model parameter required."

**Always re-initialize form-bound properties in `OnInitialized`:**

```csharp
[SupplyParameterFromForm]
private InputModel Input { get; set; } = new();

protected override void OnInitialized() => Input ??= new();
```

This is the BL0008 analyzer warning's actual fix. Apply to every `[SupplyParameterFromForm]` property on every static-SSR page.

### 4. The `DatabaseInitializer` startup pipeline

`Data/DatabaseInitializer.cs` runs five steps in order on every startup against SQL Server:

1. **`ReconcileMigrationHistoryAsync`** — if the database already contains tables that one of our EF migrations would create (because someone ran an earlier prerelease), stamp the relevant migration as already-applied in `__EFMigrationsHistory` so `MigrateAsync` doesn't crash with "object already exists." Markers are in the `MigrationMarkers` array — **add a new entry whenever you generate a new EF migration**.
2. **`MigrateAsync`** — apply genuinely-pending migrations.
3. **`EnsureMissingTablesAsync`** — uses `IMigrationsModelDiffer` + `IMigrationsSqlGenerator` to create any tables defined in the model that don't yet exist. Handles the partial-state case where step 1 stamped a migration but only some of its tables actually existed.
4. **`PatchMissingColumnsAsync`** — walks every entity in the **design-time** model (via `db.GetService<IDesignTimeModel>().Model`, **not** `db.Model` — the runtime model strips migration metadata) and `ALTER TABLE ADD`s any **nullable** columns the model expects but the live table is missing. NOT NULL columns are logged as errors and require manual SQL.
5. **`SeedAsync`** — creates Identity roles + 4 seed users. Forecast definitions are created by users through the UI.

All five steps run in both production (`AdventureWorks2022`) and dev/test (`AdventureWorks2022_dev`) — they only differ in which database on `ELITE` the connection string points at. Non-SQL-Server providers are rejected up front with `InvalidOperationException`.

### 5. `dbo.ToolSlotConfigurations` is externally managed

The `ToolSlotConfiguration` entity in `Data/Entities/ToolSlotConfiguration.cs` is configured with `.ToTable("ToolSlotConfigurations", t => t.ExcludeFromMigrations())` in `ApplicationDbContext.OnModelCreating`. EF reads/writes the table but **never** creates, alters, or drops it.

The C# property names are PascalCase but the real database columns are uppercase / snake_case (`Id` ↔ `CID`, `MtCode` ↔ `MT_CODE`, `Family` ↔ `FAMILY`, etc.). All columns have explicit `[Column("...")]` attributes mapping each property to its real column name. **If you change the entity, you must coordinate with the DBA who owns the table** — the EF migration will not propagate changes there.

### 6. `IDbContextFactory<ApplicationDbContext>` for Blazor components

Blazor interactive components must NOT inject the scoped `ApplicationDbContext` because the component lifetime can span the entire SignalR circuit, which is longer than a single logical operation. Instead, **inject `IDbContextFactory<ApplicationDbContext>`** and create a fresh context per operation:

```csharp
@inject IDbContextFactory<ApplicationDbContext> DbFactory

@code {
    private async Task LoadDataAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        // ... query
    }
}
```

`Program.cs` registers both: `AddDbContextFactory<>` for the factory + an explicit `AddScoped<ApplicationDbContext>` resolver from the factory so Identity's `UserStore` and the Minimal API endpoint handlers (which want a scoped DbContext) keep working.

### 7. API key auth scheme

External REST clients can authenticate via `X-Api-Key: ek_...` header. The handler is `Authentication/ApiKeyAuthenticationHandler.cs`. The `"ApiOrCookie"` authorization policy in `Program.cs` accepts either Identity cookies or the API key scheme — all `/api/*` minimal-API endpoints use this policy. API keys inherit the owning user's roles (so `[Authorize(Roles = "Admin")]` works via API key auth too).

Users manage their own keys at `/Account/Manage/ApiKeys`. Keys are stored plain-text in the `ApiKeys` table — there's no hash. (If that's not acceptable for production, see Phase 10 in `docs/phase-plan.md`.)

## Test infrastructure

`AWBlazorApp.Tests/IntegrationTest.cs` uses `WebApplicationFactory<Program>` with `builder.UseEnvironment("Development")`, which means the test host reads `appsettings.Development.json` and points the EF `DbContextFactory` at **`ELITE / AdventureWorks2022_dev`**. Tests do not substitute a fake database — they run against the real SQL Server instance, so `ELITE` must be reachable and the current Windows user must have access to `AdventureWorks2022_dev`.

The test fixture only overrides two feature flags via in-memory configuration:

```csharp
builder.ConfigureAppConfiguration((_, config) =>
{
    config.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Features:Hangfire"]   = "false",   // don't spin up the Hangfire server in tests
        ["RequestLogs:Enabled"] = "false",   // don't let Serilog's MSSqlServer sink write to dev RequestLogs
    });
});
```

Nothing else about the host is rewritten — EF, Identity, the minimal-API endpoints, and `DatabaseInitializer` all run in their real configurations against `AdventureWorks2022_dev`.

`dbo.ToolSlotConfigurations` is marked `.ExcludeFromMigrations()` because the DBA owns it in production. The test fixture compensates by running a `CREATE TABLE IF NOT EXISTS` idempotent SQL block in `OneTimeSetUp` so the dev database has a ToolSlotConfigurations shape that tests can exercise. Any test that writes rows to `AdventureWorks2022_dev` is responsible for cleaning up its own data (see `ToolSlot_Create_Update_Delete_Writes_Audit_Rows`).

`FormPostHelper.PostFormAsync` in the test project does GET → parse antiforgery token from HTML → POST. **The MudBlazor SSR form binding bug we fought through during the migration is only catchable by tests that actually POST forms** — page-render tests (`GetAsync` + assert 200) are not enough. When you add a new Identity form page, also add a `*_Form_Post_*` test that exercises it via this helper.

## Project memory

There are several memory notes in `~/.claude/projects/C--Users-capnb-source-repos-AWBlazorApp/memory/`:

- `project_migration_status.md` — locked-in technology decisions, the 5-phase migration history, what NOT to reintroduce
- `project_database_target.md` — SQL Server ELITE / AdventureWorks2022 specifics, ToolSlotConfigurations external management
- `feedback_mudblazor_ssr_forms.md` — the MudBlazor + static SSR forms gotcha (most important practical rule)
- `feedback_migration_style.md` — user prefers aggressive single-shot phases over micro-commits

Before starting non-trivial work, read these memory notes — they capture decisions that aren't obvious from the code.

## Analytics & data exploration (Phase 6-7)

Four analytics dashboards at `/analytics/*` (Sales, Production, HR, Purchasing) use the shared `TimeSeriesChart` and `KpiCard` components in `Components/Shared/`. The Sales dashboard uses SQL-side GroupBy queries and `AnalyticsCacheService` (IMemoryCache, 5-minute TTL). The other dashboards still load data in-memory — migrate them to the cached/SQL pattern as they grow.

Ten entity pages have `<HierarchyColumn>` + `<ChildRowContent>` with self-contained `*ExpandedRow.razor` components that load related data on expand. Cross-entity links (MudLink) in expanded rows navigate between related pages.

`GlobalSearch.razor` provides a search autocomplete in the app bar. `MainLayout.razor` has a dark mode toggle via `MudThemeProvider @bind-IsDarkMode`.

`TimeSeriesChart` has a CSV download button that exports the currently-filtered chart data.

## Security (Phase 7)

- **Rate limiting**: fixed-window 100 req/min via `AddRateLimiter` in Program.cs
- **Security headers**: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy — inline middleware in Program.cs
- **API key hashing**: `Authentication/ApiKeyHasher.cs` uses SHA-256. The auth handler checks both plain-text and hashed keys for backwards compatibility. New keys should be stored hashed.
- **Logout antiforgery**: The `/Account/Logout` endpoint has `.DisableAntiforgery()` because the Blazor Server circuit's antiforgery token can desync with the HTTP pipeline.

## Forward-looking work

`docs/phase-plan.md` documents Phases 8-14 (deployment/CI, identity completeness, observability, AI chat, further hardening, performance, documentation). Phases 1-7 are complete; phases 8+ are forward-looking and independent — pick whichever applies to the user's current need.

## Things explicitly NOT in this project

- **No Tailwind, no Vue, no NPM toolchain.** Removed in Phase 1. Never reintroduce.
- **No ServiceStack.** Removed in Phase 1. Never reintroduce.
- **No SQLite — at all.** Neither the app nor the test project references `Microsoft.EntityFrameworkCore.Sqlite` / `Microsoft.Data.Sqlite`. Tests run against real SQL Server (`ELITE / AdventureWorks2022_dev`). Never reintroduce SQLite.
- **No external auth providers** (Google/Microsoft/GitHub) wired up — see Phase 9 in `docs/phase-plan.md` to add.
- **No 2FA QR code rendering** — only the manual setup key + `otpauth://` URI. See Phase 9b.

