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
├── Components/                # just App.razor + Routes.razor + _Imports.razor now
├── wwwroot/                   # static assets
├── Properties/                # launchSettings.json
├── App_Data/                  # runtime — not in git
├── Program.cs
└── AWBlazorApp.csproj
```

## Features

One folder per domain. Most features split further by entity. Every entity has the same internal shape:

```
Features/<Feature>/<Entity>/
├── Domain/                    # entities + audit log entities
├── Dtos/                      # request/response DTOs + Mappings
├── Api/                       # minimal-API endpoint group
├── Application/
│   ├── Services/              # application services / use cases
│   ├── Validators/            # FluentValidation
│   └── Hooks/                 # cross-feature triggers (optional)
├── Audit/                     # per-entity audit log builder (optional)
└── UI/Pages/                  # Blazor pages with @page routes
```

Smaller features keep `Domain/`, `Dtos/`, `Api/`, `Services/`, `UI/Pages/` directly under the feature root rather than per-entity.

Current feature inventory:

| Feature             | What it owns                                                                  |
|---------------------|-------------------------------------------------------------------------------|
| `Admin`             | Admin dashboard, request log, user management, permissions, demo seeder       |
| `ApiExplorer`       | `/api-explorer` page + catalog                                                |
| `Dashboard`         | Cross-module plant dashboard `/dashboard/plant`                               |
| `Engineering`       | Routings, BOMs, ECOs, deviations, documents                                   |
| `Enterprise`        | Org units, cost centers, stations, assets, product lines, organizations       |
| `Forecasting`       | Forecast definitions, data points, evaluation algorithms, analytics           |
| `Gallery`           | Product photo gallery + byte-streaming endpoint                               |
| `Home`              | `/` landing page                                                              |
| `HumanResources`    | AW HR schema (employees, departments, shifts, pay/dept histories, candidates) |
| `Identity`          | `ApiKey`, `SecurityAuditLog`, login + manage scaffold UI                      |
| `Insights`          | Saved queries, KPIs, dashboards, report schedules, notifications, CSV import  |
| `Inventory`         | Items, lots, serials, locations, transactions, reports                        |
| `Logistics`         | Receipts, shipments, transfers                                                |
| `Maintenance`       | Work orders, PM schedules, spare parts, meter readings, asset profiles, logs  |
| `Mes`               | Production runs, instructions, downtime                                       |
| `Performance`       | OEE, KPIs, scorecards, reports, MetricsRollupJob                              |
| `Person`            | AW Person schema (addresses, persons, business entities, …)                   |
| `ProcessManagement` | Process definitions, step executions, scheduler job, analytics                |
| `Production`        | AW Production schema (products, models, work orders, illustrations, …)        |
| `Purchasing`        | AW Purchasing schema (vendors, POs, ship methods, product vendors)            |
| `Quality`           | Inspection plans, inspections, NCRs, CAPA                                     |
| `Sales`             | AW Sales schema (orders, customers, territories, special offers, stores)      |
| `ToolSlots`         | Tool slot configurations + audit                                              |
| `UserGuide`         | Guide articles, document tree, content (`Content/_posts/_pages/_videos/_includes`) |
| `Workforce`         | Training, qualifications, attendance, leave, announcements, alerts            |

## Shared

Cross-feature code. Lives here when ≥2 features depend on it.

```
Shared/
├── Api/                       # Hello, Export, ChartExport, Preferences, ValidationExtensions
├── Audit/                     # diff helpers
├── Domain/                    # AuditableEntity base
├── Dtos/                      # PagedResult<T>, AdminDataDto, common shapes
├── Formatting/                # number/currency formatters
├── Services/                  # AnalyticsCacheService, CsvExportService, LookupService
├── Theming/                   # AppTheme, ChartPalettes
├── UI/
│   ├── Layout/                # MainLayout, NavMenu
│   └── Components/            # KpiCard, TimeSeriesChart, GlobalSearch, EmptyState, …
└── Validation/                # MudFormValidator
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

## Identity scaffold

The ASP.NET Core Identity UI lives under `Features/Identity/UI/Pages/Account/` (Login, Register, Manage/*) — there is no separate `Scaffold/` folder. Pages keep their public `/Account/Login`, `/Account/Manage/*` routes.

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
4. **Identity scaffold lives in `Features/Identity/UI/Pages/Account/`** — re-scaffold if you need to regenerate.
5. **Namespaces match folders.** `AWBlazorApp.Features.Sales.Customers.Domain`, not
   `AWBlazorApp.Entities.Customer`.
6. **Razor pages keep their `@page` routes** through moves so URLs don't
   break.
