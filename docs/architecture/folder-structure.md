# Folder structure — current state

This is the authoritative reference for where code lives in
`src/AWBlazorApp/`. If you're adding a new feature, see
[`adding-a-feature.md`](./adding-a-feature.md).

## Top level

```
src/AWBlazorApp/
├── App/                       # composition root (thin Program.cs + extensions)
├── Features/                  # vertical slices — one folder per business domain
├── Shared/                    # cross-feature code (widgets, base domain, helpers)
├── Infrastructure/            # persistence, auth, email, hubs, cron jobs, migrations
├── Scaffold/                  # generated code (Identity UI) — don't hand-edit
├── Components/                # just App.razor + Routes.razor + _Imports.razor now
├── Data/                      # ApplicationUser (Identity type) + AppRoles constants
├── wwwroot/                   # static assets
├── Properties/                # launchSettings.json
├── App_Data/                  # runtime — not in git
├── Program.cs
└── AWBlazorApp.csproj
```

## Features

One folder per domain. Every feature has the same internal shape:

```
Features/<Name>/
├── Components/
│   ├── Pages/                 # Blazor pages with @page routes
│   └── Shared/                # feature-only widgets
├── Domain/                    # entities
├── Endpoints/                 # minimal API (group per feature)
├── Services/                  # application services / use cases
├── Audit/                     # per-entity audit logic (optional)
├── Models/                    # DTOs
└── Validators/                # FluentValidation
```

Current feature inventory:

| Feature                       | What it owns                                                             |
|-------------------------------|--------------------------------------------------------------------------|
| `AdventureWorks`              | All 73 AW tables (Person, Sales, Production, HR, Purchasing schemas), endpoints, audit, pages, analytics dashboards, geographic map |
| `Admin`                       | Admin dashboard, request log, user management, permissions, database explorer |
| `ApiExplorer`                 | `/api-explorer` page + catalog                                           |
| `Forecasting`                 | Forecast definitions, data points, evaluation algorithms, analytics      |
| `Gallery`                     | Product photo gallery + byte-streaming endpoint                          |
| `Home`                        | `/` landing page                                                         |
| `Identity`                    | `ApiKey`, `SecurityAuditLog`, `UserAreaPermission` entities              |
| `Insights`                    | Saved queries, KPIs, dashboard, report schedules, notifications, my-activity, CSV import |
| `ProcessManagement`           | Process definitions, step executions, scheduler job, analytics           |
| `ToolSlots`                   | Tool slot configurations + audit                                         |
| `UserGuide`                   | Guide articles, document tree, content (`_posts/_pages/_videos/_includes`) |

## Shared

Cross-feature code. Lives here when ≥2 features depend on it.

```
Shared/
├── Components/
│   ├── Layout/                # MainLayout, NavMenu
│   └── Widgets/               # KpiCard, TimeSeriesChart, GlobalSearch, etc.
├── Domain/                    # AuditableEntity base class
├── Endpoints/                 # Hello, Export, ChartExport, Preferences, ValidationExtensions
├── Services/                  # AnalyticsCacheService, CsvExportService, LookupService
├── Models/                    # Common DTO shapes, PagedResult<T>, AdminDataDto, etc.
├── Validators/                # MudFormValidator
└── NumberFormat.cs            # compact currency/count formatting helper
```

## Infrastructure

Things that talk to the outside world or persist state.

```
Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── DatabaseInitializer.cs
│   ├── AuditingInterceptor.cs
│   ├── AuditedSaveExtensions.cs
│   └── Migrations/
├── Authentication/            # ApiKeyAuthenticationHandler, ApiKeyHasher
├── Email/                     # SmtpConfig, SmtpEmailJob, HangfireSmtpEmailSender
├── Hangfire/                  # HangfireDashboardAuthFilter
├── Jobs/                      # cross-cutting Hangfire jobs (cleanup, migration)
└── SignalR/                   # NotificationHub
```

When the day comes to extract a separate `AWBlazorApp.Infrastructure` project,
this folder lifts out as-is.

## Scaffold

Generated code — don't hand-edit; re-run the scaffold if you need changes.

```
Scaffold/
└── Identity/
    └── Account/               # ASP.NET Core Identity UI (Login, Register, Manage/*)
```

Pages keep their public `/Account/Login`, `/Account/Manage/*` routes.

## App

The composition root.

```
App/
├── Extensions/                # ServiceRegistration, MiddlewarePipeline
├── Middleware/                # SecurityHeadersMiddleware, AreaPermissionMiddleware
└── Routing/                   # EndpointMappingExtensions
```

`Program.cs` at the root is thin — a few extension method calls.

## Rules to keep this layout alive

1. **One feature, one folder.** Don't scatter.
2. **≥2 features means `Shared/`.** One feature = stays in that feature.
3. **Talks to SQL/SMTP/Hangfire/external = `Infrastructure/`.**
4. **Generated code lives in `Scaffold/`** and isn't edited by hand.
5. **Namespaces match folders.** `AWBlazorApp.Features.Sales.Domain`, not
   `AWBlazorApp.Entities.Sales`.
6. **Razor pages keep their `@page` routes** through moves so URLs don't
   break.
