# AWBlazorApp Architecture

A guide to the application's high-level shape, layer responsibilities, and the conventions that hold it together.

## Stack

| Layer | Tech |
|---|---|
| UI | Blazor Web App (.NET 10) — mixed Interactive Server + Static SSR |
| Component library | MudBlazor 9 |
| API | ASP.NET Core minimal APIs |
| Auth | ASP.NET Core Identity + custom API key scheme |
| ORM | EF Core 10 (SQL Server provider) |
| DB | SQL Server (`AdventureWorks2022` prod, `AdventureWorks2022_dev` test) |
| Background jobs | Hangfire 1.8 (SQL Server-backed) |
| Logging | Serilog 10 with MSSqlServer sink |
| Validation | FluentValidation 12 |
| API docs | Swashbuckle |
| Tests | NUnit + WebApplicationFactory<Program> |

No NPM toolchain. No Tailwind. No Vue. No SQLite. Never reintroduce.

## High-level shape

```
+----------------------------------+   +---------------------------------+
|       Browser                    |   |       External clients          |
|  - Blazor Server circuits        |   |  - HTTP API consumers           |
|  - Static SSR Identity pages     |   |  - X-Api-Key header             |
+---------------+------------------+   +---------------+-----------------+
                |                                      |
                | SignalR + HTTP                       | HTTPS
                v                                      v
+--------------------------------------------------------------------+
|                    ASP.NET Core (.NET 10)                          |
|  +------------------------------+  +---------------------------+   |
|  |   Blazor Components          |  |   Minimal API endpoints   |   |
|  |   - Pages/AdventureWorks/*   |  |   - Endpoints/AW/*        |   |
|  |   - Pages/Analytics/*        |  |   - /api/aw/*             |   |
|  |   - Pages/Forecasts/*        |  |   - /api/forecasts        |   |
|  |   - Pages/Admin/*            |  |   - /api/tool-slots       |   |
|  |   - Pages/Account/*          |  |                           |   |
|  +--------------+---------------+  +-------------+-------------+   |
|                 |                                |                 |
|                 +----------------+---------------+                 |
|                                  |                                 |
|                                  v                                 |
|              +------------------------------------------+          |
|              |  Application services (Services/*)       |          |
|              |  - LookupService (cached reference data) |          |
|              |  - PermissionService                     |          |
|              |  - NotificationService                   |          |
|              |  - {Entity}AuditService                  |          |
|              |  - Forecasting/* algorithms              |          |
|              +------------------+-----------------------+          |
|                                 |                                  |
|                                 v                                  |
|              +------------------------------------------+          |
|              |  EF Core ApplicationDbContext            |          |
|              |  - 70+ DbSets (AdventureWorks + ours)    |          |
|              |  - AuditingInterceptor                   |          |
|              |  - DatabaseInitializer (5-stage startup) |          |
|              +------------------+-----------------------+          |
|                                 |                                  |
|                                 v                                  |
+---------------------------------|----------------------------------+
                                  |
                                  v
                          SQL Server ELITE
                  AdventureWorks2022 / AdventureWorks2022_dev
```

## Project layout

```
src/AWBlazorApp/
├── App/                            # composition root
│   ├── Extensions/                 # ServiceRegistration, MiddlewarePipeline
│   ├── Middleware/                 # SecurityHeadersMiddleware, AreaPermissionMiddleware
│   └── Routing/                    # EndpointMappingExtensions
├── Components/                     # App.razor + Routes.razor + _Imports.razor (only)
├── Features/                       # vertical slices (one folder per business domain)
│   └── <Feature>/<Entity>/         # Domain/, Dtos/, Api/, Application/{Services,Validators,Hooks}/, Audit/, UI/Pages/
│   Features include: Admin, Dashboard, Engineering, Enterprise, Forecasting, Gallery,
│   Home, HumanResources, Identity, Insights, Inventory, Logistics, Maintenance, Mes,
│   Performance, Person, ProcessManagement, Production, Purchasing, Quality, Sales,
│   ToolSlots, UserGuide, Workforce.
├── Infrastructure/                 # cross-cutting plumbing
│   ├── Persistence/                # ApplicationDbContext, DatabaseInitializer,
│   │                               # AuditingInterceptor, AuditedSaveExtensions, Migrations/
│   ├── Authentication/             # ApiKeyAuthenticationHandler, ApiKeyHasher
│   ├── Email/                      # SmtpConfig, SmtpEmailJob, HangfireSmtpEmailSender
│   ├── Hangfire/                   # HangfireDashboardAuthFilter
│   ├── Jobs/                       # Cleanup + hash-migration Hangfire jobs
│   └── SignalR/                    # NotificationHub
├── Shared/                         # cross-feature code
│   ├── Api/                        # Hello, Export, ChartExport, Preferences, ValidationExtensions
│   ├── Audit/                      # diff helpers
│   ├── Domain/                     # AuditableEntity base
│   ├── Dtos/                       # PagedResult<T>, AdminDataDto, common DTOs
│   ├── Formatting/                 # number/currency formatters
│   ├── Services/                   # LookupService, AnalyticsCacheService, CsvExportService
│   ├── Theming/                    # AppTheme, ChartPalettes
│   ├── UI/                         # Layout (MainLayout, NavMenu) + Components (KpiCard, TimeSeriesChart, GlobalSearch, …)
│   └── Validation/                 # MudFormValidator
├── wwwroot/                        # static files (css, js, images)
├── Properties/                     # launchSettings.json
├── App_Data/                       # runtime — not in git
└── Program.cs                      # composition root, calls App/Extensions/*
```

## Layer responsibilities

### Features (UI + Api + Application + Domain)
- Each feature owns its own pages, endpoints, services, validators, DTOs, and entities
- Pages live in `<Feature>/<Entity>/UI/Pages/`; should NOT contain business logic
- Inject `IDbContextFactory<ApplicationDbContext>`, NOT a scoped `ApplicationDbContext`

### Endpoints (REST API)
- Map HTTP routes to handler functions
- Call validators, then call services or write to DbContext directly
- Map DTOs ⇄ entities via the `Mappings` static helpers
- Wrap multi-step writes via `AuditedSaveExtensions.AddWithAuditAsync` (preferred) or transactions

### Services
- Pure business logic — no HTTP knowledge
- Can be called from both UI and Api layers
- Stateless (singletons) where possible; scoped only when they hold per-request state

### Infrastructure
- DbContext, entities, migrations, interceptors, auth handlers, email, Hangfire, SignalR
- The boundary to external systems

### DTOs
- The contract between layers — never expose entities directly to UI or HTTP clients
- Sealed records preferred
- Mapping helpers in `static class {Entity}Mappings`

## Cross-cutting concerns

| Concern | Implementation |
|---|---|
| Authentication | ASP.NET Core Identity (cookie) + custom `ApiKey` scheme |
| Authorization | `[Authorize(Roles=...)]`, `"ApiOrCookie"` policy, custom permission middleware |
| Logging | Serilog with structured logs to `RequestLogs` table |
| Auditing | `AuditingInterceptor` on Created/Modified fields + `{Entity}AuditService` for change history |
| Validation | FluentValidation + `MudFormValidator<T>` adapter for MudForms |
| Error handling | `app.UseExceptionHandler("/Error")` + endpoint-level try/catch around DbUpdateException |
| Rate limiting | `[EnableRateLimiting("auth")]` on auth pages, app-wide `"api"` limiter |
| Caching | `IMemoryCache` in `LookupService` and `AnalyticsCacheService` (5-min TTL) |
| Background jobs | Hangfire recurring jobs for audit/log cleanup, forecast evaluation, process scheduling |

## Render mode strategy

- **App.razor** chooses per-request based on `[ExcludeFromInteractiveRouting]`
- Interactive Server is the default for everything that isn't an Identity page
- All Identity pages use Static SSR for cookie/redirect support
- See `docs/research/blazor-net10-reference.md` for the full decision tree

## Database initialization

`DatabaseInitializer.InitializeAsync` runs 5 stages on every startup:

1. **`ReconcileMigrationHistoryAsync`** — stamp pre-existing migrations as applied
2. **`MigrateAsync`** — apply genuinely-pending migrations
3. **`EnsureMissingTablesAsync`** — create model-only tables via `IMigrationsModelDiffer`
4. **`PatchMissingColumnsAsync`** — add missing nullable columns via ALTER TABLE
5. **`EnsureCompositeIndexesAsync`** — create perf indexes (idempotent IF NOT EXISTS)
6. **`SeedAsync`** — create roles + 4 seed users

This unusual pipeline exists because the codebase uses **runtime model diffing** for most tables (only 3 actual migrations) — generating a real migration would re-include every model-only table. Document any new model addition with a note about whether it goes through migrations or runtime diffing.

## What's external (don't touch lightly)

- `dbo.ToolSlotConfigurations` — DBA-owned, `ExcludeFromMigrations()` in code, manual `[Column]` mapping for legacy column names. Coordinate with DBA before changing the entity.
- AdventureWorks2022 schema — read-only reference data; we manage CRUD audit logs but not the source tables themselves.
