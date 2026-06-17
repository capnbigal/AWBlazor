# Implementation Plan — Enterprise Data & Process Platform

**Companion to:** [`GROUND_UP_APP_REVIEW.md`](./GROUND_UP_APP_REVIEW.md). That document is the findings; this is the *how*. It is grounded in the current code (~350 routes, 118 endpoint files, ~25 domains), not the older `phase-plan.md`/`refactor-plan.md` (now historical).

**Guiding principles:**
- **Evolve, don't rewrite.** The vertical-slice architecture, API layer, validation, and theme are good. The work is hardening, adoption, and discovery — not re-architecture.
- **Build the platform out of itself.** Most "missing" capabilities already have a component (`CrudPage`, `PageHeader`, `EmptyState`, `LookupService`, `MudFormValidator`) that is simply under-adopted. Adopt before you add.
- **Fix correctness/security first, then consistency, then capability.**
- **Every change ships behind tests once CI runs them.** No risky big-bang refactors; everything is incremental and revertible.

---

## 1. Recommended Target Architecture

Keep the single project and the vertical slices. The target adds a thin validated-write layer, per-slice EF config, and a small set of cross-cutting platform services. Nothing else changes.

```
src/AWBlazorApp/
├── App/                         # composition root (Program.cs + Extensions/Middleware/Routing)
├── Features/<Domain>/<Entity>/
│   ├── Api/                     # minimal-API endpoint group (typed Results<>, validators, role gating)
│   ├── Application/
│   │   ├── Services/            # NEW where needed: CreateXAsync/UpdateXAsync — ONE validated write path
│   │   └── Validators/          # FluentValidation (already here)
│   ├── Domain/
│   │   ├── <Entity>.cs
│   │   └── <Entity>Configuration.cs   # NEW: IEntityTypeConfiguration<T> (moved out of the god-context)
│   ├── Dtos/                    # records + ToDto/ToEntity/ApplyTo
│   └── UI/Pages/                # Blazor pages (migrate onto CrudPage + PageHeader)
├── Shared/
│   ├── Api/                     # CRUD builders, EntityCatalog (NEW), search endpoint (NEW)
│   ├── Services/                # LookupService, AnalyticsCacheService, ISearchService (NEW),
│   │                            #   ICurrentUserAccessor (NEW), ISecurityAuditService (NEW)
│   ├── UI/
│   │   ├── Components/          # PageHeader (revived), CrudPage, EmptyState, Breadcrumbs (NEW),
│   │   │                        #   RelatedRecordsRail (NEW), RecordDetailShell (NEW)
│   │   └── Layout/              # NavMenu (from PageRegistry), GlobalSearch (federated)
│   └── Navigation/              # NEW: PageRegistry (route+title+synonyms+category+icon+roles)
└── Infrastructure/
    ├── Persistence/             # DbContext (thin OnModelCreating → ApplyConfigurationsFromAssembly)
    ├── Authentication/          # ApiKey (drop plaintext fallback; FixedTimeEquals)
    ├── Observability/           # NEW: OpenTelemetry wiring, correlation middleware, job-health filter
    └── ...
```

**Cross-cutting platform services to introduce (all in `Shared`/`Infrastructure`):**

| Service | Purpose | Replaces / fixes |
|---|---|---|
| `ICurrentUserAccessor` | User identity that works on the SignalR circuit *and* HTTP | Audit attribution gap on UI writes |
| `ISecurityAuditService` | Write login/lockout/password/role/API-key events | Dead `SecurityAuditLog` + zero failed-login alert |
| `ISearchService` | Federated record + page search | `GlobalSearch` page-name-only |
| `EntityCatalog` | One canonical table/entity registry | Triplicated table lists; seeds the data catalog |
| `PageRegistry` | One source of truth for routes/labels/roles | 3-way nav/search/reference drift |
| `IEndpointModule` (marker) | Convention-based endpoint discovery | 183-line hand-maintained list (optional) |

---

## 2. Suggested Folder / Project Organization

- **Stay single-project.** The triggers for splitting (non-web DTO client, pure-unit-test project, provider swap, separately-deployed workers) do not apply yet. Revisit only when one does.
- **Per-slice EF configuration.** Move each entity's fluent config from `ApplicationDbContext.OnModelCreating` into `Features/<Domain>/<Entity>/Domain/<Entity>Configuration.cs`. The `DbSet` properties can stay on the context (or move to partial files). Final `OnModelCreating` body: `modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);` plus the few genuinely cross-entity bits.
- **Resolve `Processes` vs `ProcessManagement`.** Merge under one `Processes` domain (`Processes/Definitions`, `Processes/Timelines`) **or** give timelines a distinct `/discovery/timelines` + `/api/process-timelines` prefix. Document the chosen boundary in an ADR.
- **Relocate orphan routes.** `/tool-slots` → `/maintenance/tool-slots`; align Sales/Purchasing landing pages with their `/aw` entity pages. Add redirects.
- **Complete or annotate half-built slices** (`Engineering/Documents`, `Engineering/Deviations`) — add the missing `Dtos`/`Api`, or a one-line README noting they're deliberately UI-only.
- **Archive stale docs** into `docs/history/`: `phase-plan.md`, `architecture/refactor-plan.md`, `features/backlog.md`, `reviews/*`, `patterns/servicestack-style-patterns.md`.

---

## 3. Shared Component Opportunities

The biggest single quality lever. Build/adopt these shared shells; let the ~350 pages inherit improvements from one place.

| Component | Action | Impact |
|---|---|---|
| `PageHeader` | **Revive** (currently 0 usages). Title as `<h1>` styled `h4`; optional `Subtitle`, `Breadcrumbs`, `Actions` slots. | Fixes `FocusOnNavigate`/heading hierarchy for all pages; one place for chrome |
| `CrudPage` | Render `PageHeader` internally; default `NoRecordsContent = EmptyState`; default loading skeleton. Migrate the 119 hand-rolled `Index` pages onto it in batches. | Removes ~100 duplicated grids; consistent empty/loading/a11y |
| `Breadcrumbs` | **New.** Route → `Module › Reference data › Entity`, driven by `PageRegistry`. Surface via `PageHeader`. | Orientation across the whole app |
| `RelatedRecordsRail` | **New.** Given (entity, id): related records by FK, related KPIs, process chains, next actions. | The "lead the user" discovery payoff |
| `RecordDetailShell` | **New.** Generic per-record detail page; or have list pages honor `?id=`/`?focus=`. | Enables cross-entity navigation to a *specific* record |
| `EmptyState`/`KpiCardSkeleton` | Apply by default through `CrudPage` and module landing pages. | No more "No matching records" / `0`-flash |
| `GlobalSearch` | Federate `LookupService` record search + a `PageRegistry`-generated page tier; wire Ctrl+K. | Data discovery, not nav-only |
| In-app design gallery | **New** route rendering live theme + components (mirror `design_handoff/preview`). | Keeps the design system in sync |

---

## 4. Service / API Patterns

**Validated write path (the core pattern change).**
For each write-capable slice, introduce a small application service the *endpoint and the UI both call*:

```csharp
public sealed class ShipmentService(IDbContextFactory<ApplicationDbContext> dbFactory, IValidator<CreateShipmentRequest> validator)
{
    public async Task<Result<int>> CreateAsync(CreateShipmentRequest req, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(req, ct);
        if (!v.IsValid) return Result.Invalid(v);                 // → ValidationProblem (API) or snackbar (UI)
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var e = req.ToEntity(); db.Shipments.Add(e);
        await db.SaveChangesAsync(ct);
        return Result.Ok(e.Id);
    }
}
```
- The endpoint returns `ValidationProblem`/`Created` from the `Result`; the dialog shows a snackbar/closes. **One validated, audited write path; no duplication.**
- For pure CRUD where a full service is overkill, the minimum bar is: dialogs resolve `IValidator<T>` and call `ValidateAllAsync()` before `SaveChanges` (the `MudFormValidator` bridge already exists).
- Convert highest-risk slices first (inventory/logistics/MES postings, status transitions, ProcessManagement dialogs).

**API hardening patterns:**
- `AddProblemDetails()` + `IExceptionHandler` → RFC-7807 for `/api/*`; map `DbUpdateException` (unique/FK) → 409 with a friendly message; standardize state-conflicts on 409.
- A `.ProducesStandardErrors()` extension chained on the CRUD builders → 400/401/403/404 documented for all ~90 generated endpoints.
- `AddSecurityDefinition("ApiKey", header X-Api-Key)` + requirement so Swagger documents + "Authorize"s API-key auth; enable `GenerateDocumentationFile` + `IncludeXmlComments`.
- `/api/v1` prefix now (one parent group) so a future `/api/v2` can coexist.
- `.RequireRateLimiting("api")` on the parent `/api` group; convert the ~10 untyped-`Results` endpoints to `TypedResults`.
- A shared `?sort=`/`?q=` convention baked into the CRUD builders so every list endpoint gains uniform search/sort.

**Data-access patterns:**
- `EnableRetryOnFailure` (verify inline-transaction endpoints wrap in the execution strategy).
- Global soft-delete query filters (`HasQueryFilter`) + `IgnoreQueryFilters()` admin toggle.
- Atomic `UPDATE … SET Quantity = Quantity + {delta}` (or rowversion + retry) for `InventoryBalance`.
- An `EntitySummaryService.CountSumMinMaxTopN(entity, fkColumn)` so every `*ExpandedRow` is 2 queries, never a full-history fetch.

---

## 5. Data Explorer Improvement Plan

Phased, each phase independently shippable:

1. **Search foundation (P1, ~1–2d).** `ISearchService` federating `LookupService` record searches; `/api/search`; wire `GlobalSearch` + a `/search` page. *This is the single biggest discovery win.*
2. **Per-record detail (P1, ~1w).** Generic `RecordDetailShell` or `?id=` filter for `/aw/*` + Enterprise-core (template already exists in MES/Logistics/etc.); point cross-entity links + timeline chips at specific records.
3. **`EntityCatalog` + data catalog (P1–P2, ~1–2w).** One canonical registry (de-dupe Database Explorer / export allowlist / GlobalSearch); `/catalog` glossary (friendly name, description, owner, sample values, "used by"); cross-linked to explorer/record/timeline.
4. **Schema relationship map (P2, ~1w).** `INFORMATION_SCHEMA.COLUMNS` + `sys.foreign_keys` (read-only) → columns/types/keys + navigable FK chips in the Database Explorer.
5. **Native AW timelines (P1, ~3–5d).** Synthesize timeline events from temporal columns (`OrderDate`/`ShipDate`/`DueDate`/`ModifiedDate`/status) with `AuditLog` fallback, so historical records have meaningful timelines.
6. **Ad-hoc query builder (P2, ~1w).** Guided entity+dimension+measure+filter → parameterized query; broaden beyond Admin-only; replaces the 10-metric enum.
7. **OpenAPI-driven API Explorer (P2, ~3d).** Fetch `/swagger/v1/swagger.json`; group by tag; always mirrors all endpoints.
8. **"Explore/Discover" hub (P2, ~2d).** A landing page tying explorer ↔ catalog ↔ timeline ↔ record with "use this when…" guidance.

---

## 6. Navigation Redesign Plan

1. **Central `PageRegistry`** (route, title, plain-language synonyms, category, icon, required roles) — consumed by `NavMenu`, `GlobalSearch`, `Breadcrumbs`, and a new role-filtered `/all-pages` directory. Removes the 3-way metadata drift.
2. **Breadcrumbs everywhere** via `PageHeader` (route-derived).
3. **Role-based landing.** Replace the "Welcome to AWBlazor / built on .NET 10" Home blurb with role-aware "Start here" tiles (Employee → my work; Manager → ops KPIs; Admin → admin dashboard).
4. **Label hygiene.** One concept = one label: `/reports` → "Data explorer", `/dashboard` → "Insights overview", keep "Plant dashboard"/"Admin dashboard"; disambiguate "reports".
5. **Surface the four missing analytics links** (`analytics/sales|production|hr|purchasing`) under an Insights "Analytics" group.
6. **Role-gate reference hubs + module cards** so users don't hit dead-end pages they can't use.
7. **Recently-viewed + favorites/pins** rail (per-user) on Home.

---

## 7. Dashboard / Analytics Improvement Plan

- Analytics are already SQL-side + cached — **protect that**; just fix `ForecastsAnalytics`'s in-memory average and add a short-TTL cache to `GeoAnalytics`.
- Parallelize the 20–30 sequential count round-trips in `PlantDashboardService`/Admin dashboard on cache-miss (separate short-lived contexts + `Task.WhenAll`), or pre-compute in the existing KPI-snapshot Hangfire job and read a snapshot row.
- When horizontally scaled, move analytics + KPI-snapshot caching to a distributed cache (Redis) to avoid per-replica dashboard skew; document the 5-min staleness as intentional until then.
- Add an Admin "Performance" page fed by a slow-query `DbCommandInterceptor` so the team sees the slowest dashboard/expanded-row queries against real data sizes.

---

## 8. Security / Auth Readiness Plan

Ordered by severity:

1. **[P0] Seed accounts.** Guard `SeedUsersAsync` behind `IsDevelopment()`/flag; rotate the live admin password; remove the hardcoded `Demo:AutofillLogin` password; force change-on-first-login for any prod bootstrap admin.
2. **[P0] Lockout.** `lockoutOnFailure: true`; confirm `LockoutEnabled` on users.
3. **[P1] Rate limiting.** `PartitionedRateLimiter` keyed on IP for `"auth"`; attach `"api"` to the parent `/api` group.
4. **[P1] Authorization model.** Add an auth `FallbackPolicy` requiring authentication; default the Employee permission floor to None with explicit per-area Read grants; reconcile endpoint `RequireRole` with `AreaPermissionMiddleware` so they can't drift; add an "explain access" admin tool.
5. **[P1] Security audit.** `ISecurityAuditService` writing login/lockout/password/role/API-key events → lights up the failed-login alert; add a Security activity admin viewer + a "security posture" page.
6. **[P2] API keys.** Finish the hash migration, then drop the plaintext-fallback branch; `CryptographicOperations.FixedTimeEquals`; batch `LastUsedDate` writes; required expiry + rotation + per-area scope.
7. **[P2] Transport/data.** Prod `Encrypt=True` with a trusted cert (drop `TrustServerCertificate`); tighten CSP `connect-src`; honor `[Sensitive]`/`[NotLogged]` in audit payloads (Sales has credit-card entities); re-enable logout antiforgery if the circuit-token issue is fixable.

---

## 9. Testing Plan

1. **[P0] Run tests in CI.** `mssql` service container (health-gated) + a `ConnectionStrings__DefaultConnection` env override in the fixtures + `dotnet test --logger trx --collect "XPlat Code Coverage"` with artifact upload. The good suite that exists starts gating merges.
2. **[P1] Establish the fast unit tier.** `TestValidate` tests for the most-used validators (no SQL Server); split `[Category("Unit")]` vs `[Category("Integration")]` so the unit tier runs on every push in seconds.
3. **[P1] HTTP-level CRUD round-trip tests** per domain (POST→201→GET→PATCH→DELETE→404, plus invalid payload→400) through the existing api-key client — exercises model binding + validation + role auth + the `PagedResult` envelope that workflow tests currently bypass.
4. **[P1] Remove flakiness.** Mark DB-mutating fixtures `[NonParallelizable]`; pick distinct `ProductId`s per test instead of toggling a shared `TracksLot` row; consolidate the two WAF fixtures onto one base.
5. **[P2] bUnit component tests** for the shared primitives (`CrudPage` paging/sort, `StatusHelper`, `FkAutocomplete`, `TimeSeriesChart`) — the interactive surface that defines the product, testable without SQL.
6. **[P2] Guard the fragile infra.** A `DatabaseInitializer` healing test (drop a nullable column / un-stamp a migration → assert the pipeline restores it); response-header assertions for `SecurityHeadersMiddleware`; an OpenAPI contract/snapshot test.
7. **[P2] Architecture-fitness tests** (no scoped `DbContext` in components; write pages go through a validator/service; every slice has its expected shape + a registered endpoint).

---

## 10. Documentation Cleanup Plan

| Action | Files | Note |
|---|---|---|
| **Fix the lead descriptor** | `README.md:3`, `CLAUDE.md`, ADRs, design handoff | "Blazor Server" → "Blazor Web App (mixed Interactive Server + static SSR)" |
| **Truth-up counts** | `CLAUDE.md`, `README.md`, reviews | Replace `213`/`350` with "~340 NUnit cases"; correct initializer to **8 stages**; replace 6-route API table with a pointer to `/swagger` |
| **Commit the design system** | `design_handoff_awblazor/` | Currently untracked — commit (or `.gitignore` with a note) so it survives a clean checkout |
| **Archive history** | `phase-plan.md`, `refactor-plan.md`, `backlog.md`, `reviews/*` | Move to `docs/history/`; banner each as historical |
| **Delete cruft** | `patterns/servicestack-style-patterns.md` | Self-described non-proposal for removed tech; fold any real idea into `conventions.md` |
| **Refresh the index** | `docs/README.md` | List all 6 ADRs; de-emphasize the April review; link the two new review docs |
| **New durable docs** | `docs/` | `feature-map.md` (generated from `@page` + NavMenu); `api-overview.md` (from OpenAPI); `data-explorer-design.md` + `process-discovery-design.md` (promote the timeline spec); `architecture/database-initializer.md` (the 8-stage pipeline + MigrationMarkers checklist); a single living `roadmap.md` |
| **Consolidate the "add a feature" recipe** | `CONTRIBUTING.md` + `architecture/adding-a-feature.md` | One authoritative checklist incl. audit/permission/timeline wiring |
| **Config reference** | `README.md` | Document `Demo:ShiftDates` + `Demo:AutofillLogin` with "never enable in prod" |
| **CI freshness check** | `.github/workflows` | Assert prose counts (tests/endpoints/pages) against code; fail on drift |

---

## 11. Ordered Task List

Dependencies in parentheses. P0 = production-blocking.

**Sprint 1 — Stabilize & harden (≈2 weeks)**
1. [P0] Guard seed users behind `IsDevelopment()`; rotate live admin password; remove hardcoded autofill password; `lockoutOnFailure: true`.
2. [P0] CI runs tests (mssql service + conn-string override + `dotnet test` + coverage/trx).
3. [P1] `AddProblemDetails()` + `IExceptionHandler` for `/api/*`; attach `"api"` rate limiter; partition `"auth"` by IP.
4. [P1] `ISecurityAuditService` + writes; lights up failed-login alert. (3)
5. [P1] Docker `HEALTHCHECK`; Serilog file fallback; per-request `TraceId` in LogContext.
6. [Docs] Truth-up docs, commit design system, banner stale plans, add index for new review docs *(done this session)*.

**Sprint 2 — Consistency & first discovery win (≈2–3 weeks)**
7. [P1] Revive `PageHeader` (title as `<h1>`); `CrudPage` renders it + default `EmptyState` + loading skeleton.
8. [P1] Federated record search: `ISearchService` + `/api/search` + `GlobalSearch` federation + Ctrl+K. (LookupService)
9. [P1] Fix the ~5 over-fetching expanded rows → aggregate projections.
10. [P1] `InventoryBalance` concurrency fix; `EnableRetryOnFailure`; global soft-delete query filters.
11. [P1] First validator unit tests + `[NonParallelizable]` on DB-mutating fixtures. (2)
12. [P1] Wire ProcessManagement dialogs to their existing validators + try/catch.

**Sprint 3 — Authz model & per-record discovery (≈3 weeks)**
13. [P1] Auth `FallbackPolicy`; narrow Employee floor; reconcile the two authz models.
14. [P1] Per-record detail / `?id=` filter for `/aw` + Enterprise-core; deep-link cross-entity links + timeline chips. (7)
15. [P1] `RelatedRecordsRail` on detail pages. (14)
16. [P1] Native AW activity timelines (temporal-column synthesis + AuditLog fallback).
17. [P2] Central `PageRegistry`; breadcrumbs; role-based landing; analytics nav links.

**Sprint 4 — Catalog, observability, API contract (≈3 weeks)**
18. [P1] `EntityCatalog` (de-dupe table lists); `/catalog` data glossary. (17)
19. [P2] Schema relationship map in the Database Explorer.
20. [P1] OpenTelemetry auto-instrumentation + OTLP/Prometheus exporter; correlation IDs in RequestLogs/AuditLog; job-health watchdog.
21. [P2] API: `/api/v1` prefix, ProblemDetails error docs, OpenAPI security scheme + XML comments, `?sort=`/`?q=` convention.
22. [P2] Per-entity `IEntityTypeConfiguration` extraction (one domain at a time, snapshot-verified).

**Ongoing / opportunistic**
- Migrate hand-rolled `Index` pages onto `CrudPage` in batches (7).
- Convert untyped-`Results` endpoints to `TypedResults`; standardize 409 conflicts.
- bUnit + architecture-fitness tests. (2)
- Archive stale docs; generated `feature-map.md`/`api-overview.md`; docs-freshness CI check.

---

### How to use this plan

Treat Sprint 1 as non-negotiable and immediate (it contains both P0s). Sprints 2–4 are the path from "hardened internal app" to "guided data + process discovery platform." Each task is independently revertible and leaves the app running. Re-baseline after Sprint 1 once CI is gating — the test suite will catch regressions the rest of the plan might otherwise introduce.
