# Vertical-slice refactor — plan of record

Status: in progress (as of 2026-04-15).

The solution is being refactored from layered-first organization (one feature
scattered across `Data/`, `Models/`, `Services/`, `Validators/`, `Endpoints/`,
`Components/Pages/`) to a domain-first vertical-slice layout where each feature
owns one folder under `Features/<Name>/`.

## Target tree (single project)

```
AWBlazorApp/
├── App/                    # composition root (thin Program.cs + extensions)
│   ├── Extensions/
│   ├── Middleware/
│   └── Routing/
├── Features/               # vertical slices
│   ├── Identity/
│   ├── Admin/
│   ├── Sales/              # AW Sales schema + sales analytics + geo
│   ├── Production/         # AW Production schema + production analytics + gallery
│   ├── Purchasing/
│   ├── HumanResources/
│   ├── Person/             # AW Person schema (shared addresses/persons)
│   ├── Forecasting/
│   ├── ProcessManagement/
│   ├── ToolSlots/
│   ├── UserGuide/          # includes _posts/_pages/_videos/_includes content
│   ├── Gallery/
│   ├── Insights/           # saved queries, KPIs, dashboards, reports, notifications
│   └── ApiExplorer/
├── Shared/                 # cross-feature code
│   ├── Components/         # KpiCard, TimeSeriesChart, GlobalSearch, layout
│   ├── Domain/             # AuditableEntity, base records
│   ├── Services/           # AnalyticsCacheService, CsvExportService, etc.
│   └── Endpoints/          # helper extensions
├── Infrastructure/         # persistence + cross-cutting plumbing
│   ├── Persistence/        # DbContext, DatabaseInitializer, Migrations,
│   │                       # EntityTypeConfigurations
│   ├── Authentication/
│   ├── Email/
│   ├── Hangfire/
│   └── SignalR/
├── Scaffold/               # generated — don't hand-edit
│   └── Identity/           # Identity UI scaffold lives here
├── wwwroot/
├── Properties/
├── App_Data/               # runtime only — ignored by git
├── AWBlazorApp.csproj
└── Program.cs
```

Each feature folder has the same internal shape:

```
Features/<Name>/
├── Components/
│   ├── Pages/              # Blazor pages
│   └── Shared/             # feature-only widgets
├── Domain/                 # entities
├── Endpoints/              # minimal API
├── Services/               # application services / use cases
├── Audit/                  # per-entity audit logic (optional)
├── Models/                 # DTOs
└── Validators/             # FluentValidation
```

## Single-project decision

Staying single-project for now. Blazor Server is tightly coupled to its host;
splitting into `Domain`/`Infrastructure`/`Web` projects adds friction with
limited near-term payoff. Trigger list for a future split:

| Trigger                              | Add project                   |
|--------------------------------------|-------------------------------|
| Non-web client needs DTOs            | `AWBlazorApp.Contracts`       |
| Pure unit tests without WebApp host  | `AWBlazorApp.Domain`          |
| Swap EF/SQL for another provider     | `AWBlazorApp.Infrastructure`  |
| Workers deploy separately from web   | `AWBlazorApp.Worker`          |

None apply today. Defer.

## Phasing

The migration ships in ~13 PRs; each is independently revertible and leaves
the app running.

| Phase | Scope                                                       |
|-------|-------------------------------------------------------------|
| 0     | Hygiene + architecture doc (this file)                      |
| 1     | Scaffold empty `App/ Features/ Shared/ Infrastructure/` folders |
| 2     | Infrastructure extraction (DbContext, migrations, auth, email, hubs) |
| 3     | Composition root: `Startup/` → `App/`                       |
| 4     | Small features → `Features/` (Insights, Forecasting, etc.)  |
| 5     | Identity UI scaffold → `Scaffold/Identity/`                 |
| 6     | AdventureWorks split by SQL schema (Sales/Production/HR/Purchasing/Person) |
| 7     | `Analytics/*` pages distributed into their owning features  |
| 8     | `Shared/` cleanup + delete empty layer folders              |
| 9     | `AWBlazorApp.Tests/` mirrors `Features/`                    |
| 10    | Docs + `adding-a-feature.md` contributor guide              |

## Rules, once this lands

1. **One feature, one folder.** Pages, entities, endpoints, services, DTOs,
   validators for feature *X* live under `Features/X/`. Nothing else.
2. **`Shared/` is for ≥2-feature code.** If only one feature uses it,
   it stays in that feature.
3. **`Infrastructure/` is the boundary.** Any code that talks to SQL, SMTP,
   Hangfire, or external auth lives here.
4. **`Scaffold/` is off-limits for hand edits.** Re-run the scaffold.
5. **Tests mirror `Features/` exactly.** New feature → new test folder of
   the same name.
