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
AWBlazorApp/
├── Authentication/         # API key auth handler, Hangfire dashboard auth filter
├── Components/
│   ├── Account/            # ASP.NET Identity scaffolded pages (static SSR)
│   ├── Layout/             # MainLayout, NavMenu
│   ├── Pages/              # All routable pages
│   │   ├── AdventureWorks/ # 67 reference-data CRUD page sets
│   │   ├── Analytics/      # Sales, Production, HR, Purchasing dashboards
│   │   ├── Forecasts/      # Forecast definition + execution UI
│   │   ├── ToolSlots/      # External-table CRUD
│   │   ├── Processes/      # Process definition + execution
│   │   ├── Admin/          # Users, Permissions, Audit, Activity
│   │   ├── Reports/        # Database explorer
│   │   └── Guide/          # Markdown-driven user guides
│   ├── Shared/             # KpiCard, TimeSeriesChart, GlobalSearch, EmptyState, ...
│   └── App.razor           # Root component
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DatabaseInitializer.cs
│   ├── AuditingInterceptor.cs
│   ├── AuditedSaveExtensions.cs   # transactional audit helper (preferred for new code)
│   ├── AppRoles.cs
│   ├── PermissionArea*.cs
│   └── Entities/
│       ├── AdventureWorks/
│       ├── Forecasting/
│       ├── ProcessManagement/
│       └── *.cs                   # ApiKey, SecurityAuditLog, etc.
├── Endpoints/
│   ├── AdventureWorks/             # 67 CRUD endpoint files (one per entity)
│   ├── ToolSlotConfigurationEndpoints.cs
│   ├── ForecastEndpoints.cs
│   ├── PreferencesEndpoints.cs     # dark mode toggle
│   ├── PermissionEndpoints.cs
│   └── EndpointMappingExtensions.cs
├── Hubs/
│   └── NotificationHub.cs           # SignalR
├── Migrations/                       # 3 migrations (most schema is runtime-diffed)
├── Models/
│   ├── AdventureWorks/              # 67 DTO files (Dto + CreateRequest + UpdateRequest + Mappings)
│   ├── Common.cs                    # PagedResult<T>, IdResponse
│   └── *.cs
├── Services/
│   ├── AdventureWorksAudit/         # 67 audit-log builder services
│   ├── Forecasting/                 # Forecast algorithms (Linear, Exp Smoothing, Moving Avg)
│   ├── LookupService.cs             # cached reference-data lookups
│   ├── PermissionService.cs
│   ├── NotificationService.cs
│   ├── AuditLogCleanupJob.cs        # Hangfire daily prune
│   ├── RequestLogCleanupJob.cs
│   ├── ApiKeyHashMigrationJob.cs
│   └── ProcessSchedulerJob.cs
├── Startup/
│   ├── ServiceRegistration.cs       # all AddX() helpers
│   ├── MiddlewarePipeline.cs        # all UseX() + endpoint mappings
│   └── SecurityHeadersMiddleware.cs
├── Validators/
│   ├── AdventureWorks/              # FluentValidation per entity
│   ├── MudFormValidator.cs          # adapter MudForm ⇄ FluentValidation
│   └── *Validators.cs
├── _posts/                           # Markdown blog content (compiled into output)
├── _pages/                           # Markdown user guides
├── wwwroot/                          # static files (css, js, images)
└── Program.cs                        # 80 lines, all setup is in Startup/*
```

## Layer responsibilities

### Components (UI)
- Render UI, handle user input, dispatch to services or directly to `IDbContextFactory<T>`
- Should NOT contain business logic — extract to services
- Should NOT contain data validation rules — those live in FluentValidation validators
- Inject `IDbContextFactory<ApplicationDbContext>`, NOT a scoped `ApplicationDbContext`

### Endpoints (REST API)
- Map HTTP routes to handler functions
- Call validators, then call services or write to DbContext directly
- Map DTOs ⇄ entities via the `Mappings` static helpers
- Wrap multi-step writes in transactions (use `AuditedSaveExtensions` for the entity+audit pattern)

### Services
- Pure business logic — no HTTP knowledge
- Can be called from both Components and Endpoints
- Stateless (singletons) where possible; scoped only when they hold per-request state

### Data
- DbContext, entities, migrations, interceptors
- Entities contain the schema only — no business methods (anemic by design; logic lives in services)

### Models (DTOs)
- The contract between layers — never expose entities directly to Components or HTTP clients
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
