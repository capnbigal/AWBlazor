# Ground-Up Application Review

**Date:** 2026-06-17
**Reviewer:** Claude Opus 4.8 (1M) — 13-dimension multi-agent review, 44 agents, with adversarial verification of every falsifiable high/critical claim.
**Method:** First-principles review of the *actual* code (not prior plans). 13 specialist reviewers covered structure, architecture, API/OpenAPI, EF/data, UI/UX, navigation/IA, data-explorer & process-discovery, security, observability, validation, testing, performance, and docs. The 31 highest-impact falsifiable findings were then re-checked against source by independent verifier agents: **all 31 confirmed or partially-confirmed, none refuted.** Where a reviewer overstated scope, this report uses the corrected scope.

> **Scope note for readers:** This review *supersedes* `docs/reviews/*` (April 2026) and `docs/architecture/refactor-plan.md`. Those describe a 67-endpoint, pre-vertical-slice app of ~350 tests. The codebase has since roughly doubled: **~350 routable page routes, 118 API endpoint files across ~25 domains, ~693 `.cs` + ~431 `.razor` files, 35 EF migrations, an 8-stage startup initializer.** Treat the older docs as history.

---

## Executive Summary

**What the app is today.** AWBlazor is a genuinely substantial, well-engineered enterprise Blazor application built on the AdventureWorks2022 schema. It is *not* a thin demo. It has a clean vertical-slice architecture applied consistently across ~115 entity slices, a mature minimal-API layer (typed `Results<>`, FluentValidation, `AsNoTracking` + clamped paging, a uniform `PagedResult<T>` envelope, per-verb role authorization), a transparent audit-logging interceptor, SHA-256-hashed API keys, security headers, health checks, Hangfire background jobs, six SQL-side cached analytics dashboards, a product-grade live Plant dashboard, and a thoughtfully-tuned MudBlazor theme with full dark mode. Around it sits real operational maturity: a Dockerized DigitalOcean deployment, deployment runbooks, dependabot, a vulnerability-scan CI job, ADRs, and a high-fidelity design-system handoff lifted from the real theme code.

**What it could become.** The stated goal — evolve from "technical demo around AdventureWorks" into a polished, enterprise-ready **internal data + process-discovery platform** that lets non-technical users discover data, understand processes, follow relationships, and find next actions *without knowing the schema* — is the right north star, and the bones are already here. The gap is not the foundation; it is **(a) a handful of real enterprise-readiness defects** (one critical), **(b) under-adoption of the platform's own good abstractions**, and **(c) the missing "connective tissue"** that turns ~350 list-and-grid pages into a guided discovery experience.

**The highest-value direction, in one line:** *Fix the small set of production-blocking defects now, harvest the dozens of cheap consistency wins the codebase has already built the components for, then invest the next quarter in the three discovery capabilities that change the product — federated record search, per-record detail + relationship navigation, and a data catalog/native-history layer over the AdventureWorks tables.*

**The one thing that must change before anything else:** Default admin credentials (`admin@email.com` / `p@55wOrd`, granted all roles) are seeded **unconditionally in every environment, including the live production database**, with no environment guard and no force-rotate. This is a known-credentials full-admin takeover on the deployed app. It is a one-line fix plus a password rotation. **Do it today.**

---

## Current State Inventory

### Solution & code

| Aspect | Reality |
|---|---|
| Solution | `AWBlazorApp.slnx` — 2 projects: `AWBlazorApp` (web) + `AWBlazorApp.Tests` (NUnit) |
| Stack | .NET 10 · Blazor Web App (mixed Interactive Server + static SSR) · MudBlazor 9.3 · EF Core 10 (SQL Server, HierarchyId) · ASP.NET Core Identity · Hangfire 1.8 · Serilog 10 · FluentValidation 12 · Swashbuckle |
| Size | ~431 `.razor`, ~693 `.cs`, **~350 routable `@page` routes**, ~25 feature domains, 35 EF migrations |
| Layout | Vertical slices: `Features/<Domain>/<Entity>/{Api, Domain, Dtos, Application(Services/Validators), UI}` + `Shared/`, `Infrastructure/`, `App/` |
| Composition root | `Program.cs` (96 LOC) → named extension methods; `AddFeatureServices()` composes 16 per-domain registrations |

### Feature domains

- **AdventureWorks-derived CRUD** (routes under `/aw/*`, ~177 pages): Person, Production, Sales, Purchasing, HumanResources.
- **Custom business domains:** Enterprise, Engineering, MES (shop-floor), Quality, Maintenance, Workforce, Inventory, Logistics, Performance, Forecasting, ProcessManagement, Processes (Timelines), Insights, Dashboard, Admin, ApiExplorer, Gallery, UserGuide, Identity, Home.

### API & services

- **118 endpoint files** calling `MapGroup("/api...")`, of which 46 use the shared generic `MapCrudWithInterceptor` builder and ~69 hand-roll for custom logic. Uniform typed-results + validation + paging pattern.
- Auth: Identity cookies **+** `X-Api-Key` scheme; `"ApiOrCookie"` policy accepts either. Roles: Admin/Manager/Employee. Area-level gating via `AreaPermissionMiddleware` + a permissions admin UI.
- 8 recurring Hangfire jobs + 1 one-shot (cleanups, forecast eval, process scheduler, notification eval, KPI snapshot, inventory outbox emitter, metrics rollup, API-key hash migration).

### Data & persistence

- `ApplicationDbContext` (1377 LOC): **exactly 161 `DbSet`s, 156 inline `modelBuilder.Entity<>` blocks in one `OnModelCreating`, zero `IEntityTypeConfiguration` classes.** Thorough indexing (~150 `HasIndex`), correct `.ExcludeFromMigrations()` + `.HasTrigger()` handling of ~70 DBA-owned AW tables.
- `DatabaseInitializer` (898 LOC): **8-stage** startup pipeline (reconcile history → migrate → ensure-missing-tables → patch-missing-columns → ensure-required-columns → ensure-composite-indexes → ensure-spatial-seed → seed) + optional `Demo:ShiftDates`.
- Transactional inventory **outbox** pattern; consolidated audit-log interceptor.

### UI & discovery surfaces

- Shared components: `CrudPage`, `PageHeader`, `EmptyState`/`EmptyChartState`, `KpiCard`(+Skeleton), `TimeSeriesChart` (CSV export), `GlobalSearch`, `ModuleCard`, `ReferenceDataHub`, `StatusChip`, `FkAutocomplete`/`FkSelect`, `AuditHistoryGrid`, `PermissionGuard`, `NotificationListener`.
- Discovery: `/reports` (Database Explorer — row counts), `/queries` (10 canned metrics, Admin-only), `/import` (CSV), `/documents/tree` (HierarchyId), `/api-explorer`, `/enterprise/tree`, `/processes` + `/processes/timeline` (MudTimeline), six `/analytics/*` dashboards, live Plant dashboard.

### Docs & ops

- `CLAUDE.md`, `README.md`, `CONTRIBUTING.md`; `docs/` with `adr/` (6), `architecture/` (5), `patterns/` (4), `research/` (4), `reviews/` (3), `features/` (2), `agents/`, `superpowers/`; deployment runbooks + scripts; `.github/workflows/` (build + image push), dependabot, templates; **untracked** `design_handoff_awblazor/` design system.

---

## Strengths

These are real and should be protected, not "refactored away":

1. **Vertical-slice architecture, applied at scale.** ~115 entity slices share the same shape across AW and custom domains. New domains add without touching existing ones. Rare at this size.
2. **The minimal-API layer is the most mature part of the codebase.** 111 of ~115 endpoint files use typed `Results<>` unions; uniform `PagedResult<T>`; `AsNoTracking` + `Math.Clamp(take, 1, 500)`; FluentValidation → `ValidationProblem`; correct REST verbs (201+Location, 204, 404); per-verb role gating tightening to Manager/Admin on destructive ops.
3. **Validation is a single source of truth.** 250 `AbstractValidator` classes; auto-registered; reused identically by the API (`ValidationProblem`) and the UI via a clean `MudFormValidator<T>` bridge (73 of 79 dialogs). Rules show real domain thought (cross-field, regex, ranges, conditional PATCH).
4. **`IDbContextFactory` discipline is followed without exception** — 0 of ~431 `.razor` components inject the scoped context; all use per-operation `await using`.
5. **Data-access hygiene + indexing.** Endpoint queries project to DTOs, clamp paging, order by indexed columns; ~150 explicit indexes including filtered-unique and composite. The ~70 DBA-owned AW tables are handled correctly (`ExcludeFromMigrations` + `HasTrigger` to dodge the OUTPUT-clause problem) — a subtle, hard-won detail done right.
6. **Analytics are SQL-side and cached.** All six `/analytics/*` dashboards aggregate via `GroupBy` behind `AnalyticsCacheService` (5-min TTL) — the CLAUDE.md note that "others still load in-memory" is now stale.
7. **Transparent audit logging.** `AuditLogInterceptor` emits before/after JSON diffs on every `SaveChanges`, with a sensible exclusion list — so even the raw UI write path is audited.
8. **A genuinely good security baseline** beneath the defects: SHA-256 API keys with a migration job, parameterized + allowlisted raw SQL everywhere, Admin-gated admin/permissions/Swagger/Hangfire surfaces, DTOs that never leak password hashes, HSTS + security headers + cookie hardening, antiforgery + `LocalRedirect` on Identity.
9. **Product-grade surfaces exist.** The Plant dashboard (live SignalR, clickable deep-linked KPIs, inline SVG sparklines, critical-alerts banner) and the Process Timeline UI (breadcrumbs, status chips, inline actions) show the team can build polished experiences.
10. **A real design system.** `design_handoff_awblazor/DESIGN_SYSTEM.md` + `colors_and_type.css` are lifted from the actual `AppTheme.cs`/`ChartPalettes.cs` — accurate, not aspirational. Zero hardcoded hex colors in feature pages; dark mode holds together.
11. **Test quality where it exists is high.** The `FormPostHelper` GET→token→POST flow catches the exact MudBlazor SSR-form regression class CLAUDE.md warns about; auth tests are adversarial (valid/invalid/revoked keys); workflow tests assert real downstream balance state; combinatorial guard tests catch EF "could not be translated" across the whole registry.
12. **Operational maturity:** Dockerized DO deployment, runbooks, dependabot, a `dotnet list package --vulnerable` CI job, 6 well-formed ADRs.

---

## Major Gaps

Ordered by how much they block "enterprise-ready." Each is verified against source.

1. **[CRITICAL] Default admin credentials seeded in production.** `DatabaseInitializer.SeedUsersAsync` runs unconditionally in every environment and creates `admin@email.com` / `p@55wOrd` with all roles. No environment guard, no rotation. *Full-admin takeover on the live app.*
2. **[HIGH] Brute-force protection is effectively off.** `Login.razor` calls `PasswordSignInAsync(..., lockoutOnFailure: false)`, so the configured 5-attempt lockout never fires; the `"auth"` rate limiter is a single global (non-partitioned) bucket — coarse enough to DoS all users yet not per-attacker.
3. **[HIGH] The `"api"` rate limiter is dead config.** Registered (100/min) but attached to **zero** endpoints; no global limiter. The entire `/api` surface (118 files) is unthrottled despite docs claiming otherwise.
4. **[HIGH] No global error contract.** `AddProblemDetails()` is never called; unhandled API exceptions (SqlException, FK/unique violations, null in `.ToDto`) fall through to the **HTML** `/Error` page — useless for the `X-Api-Key` clients the app explicitly supports.
5. **[HIGH] CI never runs the tests.** `build.yml` only restores + builds. ~344 NUnit cases — including the SSR-form and auth regression guards — gate nothing on PR or merge. A green check is misleading.
6. **[HIGH] Security events are never recorded.** `SecurityAuditLog` is read by the failed-login alert but **never written** anywhere — logins, lockouts, password changes, role grants, API-key events are invisible, and the "Failed logins (24h)" alert is permanently zero (false sense of safety).
7. **[HIGH] The discovery story is scaffolding without substance.** `GlobalSearch` matches ~55 static page names and **no records**; there are **no per-record detail pages** for `/aw` entities (cross-entity links land on unfiltered lists); the Process Timeline reads only the in-app `AuditLog`, so it is **empty for all historical AdventureWorks data**; the Database Explorer shows row counts but no columns/keys/relationships; there is **no data catalog/glossary**.
8. **[HIGH] Dual write-path with validation bypass.** 206 `.razor` files call `SaveChangesAsync` directly and **0** invoke a validator — every UI create/edit re-derives the query and skips the FluentValidation rules its matching API endpoint enforces.
9. **[HIGH] No production telemetry.** Zero OpenTelemetry/metrics/tracing; no correlation/trace ID linking `RequestLogs` ↔ `AuditLog` ↔ exceptions; no alerting on Hangfire job failure or health degradation; no Docker `HEALTHCHECK`.
10. **[MEDIUM] The platform's own good abstractions are under-adopted.** `PageHeader` is referenced **0 times** (dead); `CrudPage` reaches ~10 of ~138 list pages while 119 hand-roll the grid; `EmptyState` reaches ~10 of ~118 grids; **no page emits an `<h1>`** so `FocusOnNavigate Selector="h1"` silently never works.
11. **[MEDIUM] The `ApplicationDbContext` god-file** (161 DbSets, 156 inline configs, 0 `IEntityTypeConfiguration`) is the one place the slice architecture breaks — a permanent merge-conflict magnet.
12. **[MEDIUM] Docs have drifted hard from the code** (test count says 213 *and* 350; initializer "5 steps" is really 8; README documents 6 of 118 API routes; `phase-plan.md` still lists "git init" as pending). The high-value untracked design system can be wiped by a clean checkout.

---

## Architecture Recommendations

The architecture is sound. These are *consistency and boundary* fixes, not a rewrite. **Do not** introduce MediatR/CQRS for 150 CRUD entities, split into multiple projects, or re-slice the working layout.

1. **Close the dual write-path by construction, not discipline.** The cheapest durable fix is a thin **per-entity application service** (`CreateShipmentAsync`/`UpdateShipmentAsync`) that runs the validator + maps + saves, called by *both* the endpoint and the Blazor dialog. Where that is too much for pure CRUD, at minimum have edit dialogs resolve and run `IValidator<T>.ValidateAllAsync()` before `SaveChanges` (the `MudFormValidator` bridge already exists — ~30 AW dialogs do this; the strategic ProcessManagement dialogs do not). Convert the highest-risk write slices first (inventory/logistics/MES postings, status transitions).
2. **Extract `IEntityTypeConfiguration<T>` per entity, incrementally.** Co-locate each `<Entity>Configuration.cs` in its slice's `Domain/`, replace `OnModelCreating`'s body with `ApplyConfigurationsFromAssembly(typeof(Program).Assembly)`. Pilot on one domain, confirm the model snapshot is byte-identical, then proceed domain-by-domain. Pure relocation, no schema change.
3. **Adopt the shared shells you already built.** Make `CrudPage` render `PageHeader` internally; migrate hand-rolled `Index` pages onto `CrudPage` in batches (most are a re-wrap, not a rewrite). This is the lever that fixes empty/loading-state inconsistency, the missing `<h1>`, and future breadcrumbs *in one place*.
4. **Make cross-cutting invariants explicit.** Give `IPostingTriggerHook` an `Order` property and sort at consumption, removing the brittle "register Quality before Workforce" DI-order dependency. Write a short ADR mapping the intentional soft-FK cross-slice references (e.g. `InventoryItem.ProductId`) so aggregate boundaries are documented, not implied by scattered comments.
5. **Reduce the two remaining central coupling points** (`ApplicationDbContext.OnModelCreating` and the 183-line hand-maintained `EndpointMappingExtensions`) with convention-based discovery — a marker interface `IEndpointModule`/reflection registrar for endpoints, and `ApplyConfigurationsFromAssembly` for EF. Optional; lower priority than (1)–(3).
6. **Resolve the `AuditedSaveExtensions` ambiguity.** The `AuditLogInterceptor` already audits every `SaveChanges`, and the helper has **zero** feature usages. Either delete it and correct CLAUDE.md §8, or re-document it strictly as a "two-writes-in-one-transaction" convenience unrelated to audit completeness.
7. **Disambiguate `Processes` vs `ProcessManagement`.** Two domains share `/processes` and `/api/processes` with no documented boundary. Either merge under one `Processes` domain (`Processes/Definitions`, `Processes/Timelines`) or give the timeline feature a distinct prefix (`/discovery/timelines`) — and document the split.

**Target architecture (practical):** keep single-project vertical slices; add a thin per-slice application-service layer for validated writes; per-slice EF configurations; convention-based endpoint/config discovery; a small set of cross-cutting platform services (`ICurrentUserAccessor`, `ISecurityAuditService`, an `EntityCatalog`, a federated `ISearchService`) in `Shared/`. That is the whole delta.

---

## UI/UX Recommendations

The design foundation is strong; the components are unevenly wired up, so the experience swings between "polished product" (dashboard, timelines, analytics) and "raw table" (most CRUD pages). Close that gap by **adoption**, not redesign.

1. **Revive `PageHeader` as the one page-chrome component** — title rendered as `<h1>` (styled like `h4`), optional subtitle, optional breadcrumbs slot, consistent actions slot — and route `CrudPage` through it. This single change fixes the `FocusOnNavigate`/heading-hierarchy a11y defect across all pages and gives one place to evolve chrome.
2. **Make friendly empty + loading states the default.** Add `NoRecordsContent = EmptyState` inside `CrudPage` so every shell page gets it; add a `loaded`/`KpiCardSkeleton` guard to the ~10 module landing pages so KPIs don't flash `0` while counts load. (Honor the design system's terse empty-state copy.)
3. **Add breadcrumbs everywhere via the shared header.** Only 2 of ~350 pages have them today. A route-driven `Module › Reference data › Entity` trail through `PageHeader`/`CrudPage` propagates to all pages at once — core orientation for non-technical users in a 350-page app.
4. **Fix the error/empty edges.** Replace the dev-oriented copy in `Error.razor` ("Switching to the Development environment…") with a calm message + "Return home" button; add a friendly `<NotFound>` to the Router; wrap interactive content in a top-level `<ErrorBoundary>` with a MudBlazor fallback showing the trace id + retry.
5. **Hide schema jargon from non-technical users.** Leaf CRUD pages surface raw table names (`Sales.Customer`) as captions. Gate that behind an Admin/"developer detail" toggle; show plain-language titles to everyone else.
6. **Pick 5–10 high-traffic grids and give them a narrow-viewport strategy** (reduced essential-column set or stacked card layout under `sm`) so the product doesn't read as a horizontal-scroll database slab on a tablet.
7. **Per-user grid preferences** (density, chosen columns, saved filters) and a small **in-app design-system gallery** route that renders the live theme + components, so the documented system stays in sync.

---

## Data Explorer and Process Discovery Recommendations

This is the product thesis and the area with the **highest leverage**. The building blocks exist (Process Timeline engine, API Explorer, Database Explorer, Document/Org trees, expanded-row cross-links) but each is shallow or schema-blind, so a user still has to know the schema and hunt. The job is to add the connective tissue that makes the app *lead* users.

**The three changes that change the product:**

1. **Federated record search (the single highest-value gap).** `GlobalSearch` searches ~55 page names and zero records — yet `LookupService` *already* exposes deep-linkable DB search for products (name/number), customers (account), vendors, employees, sales/purchase orders. Wire those into `GlobalSearch` as a "Records" tier alongside pages, and add a `/search` page + `/api/search` endpoint. This is the change most likely to make the app feel like it leads users, and it largely reuses existing code.

2. **Per-record detail + relationship navigation.** There are no `/aw/<entity>/{id}` detail pages, so every cross-entity "related" link lands on an unfiltered list. Newer modules (MES, Logistics, Engineering, Quality, Maintenance, Performance) **already** have `{Id}` detail pages and query-param-filtered lists — a working template in-repo. Extend it to `/aw/*` and Enterprise-core: a generic record-detail page (or have list pages honor `?id=`/`?focus=`), then point cross-entity links and the Process Timeline's `EntityTypeChip` at the specific record. Add a "Related & next actions" rail (related records by FK, related KPIs, the chains this record participates in, suggested actions).

3. **A data catalog + native-history layer.** Add a `/catalog` (business glossary): friendly name, plain-English description, owner, sample values, and "used by these pages/APIs" for every entity — backed by a single canonical `EntityCatalog` that also dedupes the table list currently copy-pasted across Database Explorer, the export allowlist, and `GlobalSearch`. Upgrade the Database Explorer from a row-count scoreboard into a *map* by reading `INFORMATION_SCHEMA.COLUMNS` + `sys.foreign_keys` (read-only) to show columns/types/keys and navigable FK relationships. And make the **Process Timeline honest and useful for real data** by synthesizing events from native AW temporal columns (`OrderDate`/`ShipDate`/`DueDate`/`ModifiedDate`/status) with `AuditLog` fallback — today it shows "No activity" for essentially every historical record.

**Supporting moves:** make "Saved Queries" actually ad-hoc (a guided entity+dimension+measure builder, broadened beyond Admin-only) instead of 10 canned metrics; drive the API Explorer from the live OpenAPI doc instead of an 11-endpoint hand list; add an "Explore/Discover" hub that ties explorer ↔ catalog ↔ timeline ↔ record together with "use this when…" guidance.

---

## Enterprise Readiness Checklist

| Area | Current State | Recommendation | Priority | Effort | Risk |
|---|---|---|---|---|---|
| **Secrets / seed accounts** | Default admin `p@55wOrd` (all roles) seeded in **prod**; no env guard | Guard seed behind `IsDevelopment()`/flag; rotate live password; force change-on-first-login for any prod bootstrap | **P0** | S | low |
| **Account lockout** | `lockoutOnFailure: false` — 5-attempt policy is dead config | Pass `lockoutOnFailure: true`; confirm `LockoutEnabled` on users | **P0** | XS | low |
| **API rate limiting** | `"api"` policy registered, attached to nothing | Wrap `/api` in a parent group with `.RequireRateLimiting("api")` | **P1** | S | low |
| **Auth rate limiting** | Single global non-partitioned bucket (DoS-able) | `PartitionedRateLimiter` keyed on IP (and/or username) | **P1** | S | low |
| **API error contract** | No `AddProblemDetails`; 500s render HTML | `AddProblemDetails()` + `IExceptionHandler` → RFC-7807 for `/api/*`; map `DbUpdateException` → 409 | **P1** | S | low |
| **Authorization model** | Two parallel models; no fallback policy; Employee gets Read on **all** areas (Sales/HR/Person) | Add auth `FallbackPolicy`; default Employee floor to None + explicit per-area grants; reconcile endpoint roles ↔ middleware | **P1** | L | med |
| **Security audit trail** | `SecurityAuditLog` never written; failed-login alert always 0 | `ISecurityAuditService`; write login/lockout/password/role/API-key events | **P1** | M | low |
| **CI gating** | Build only; tests never run | Add `mssql` service + conn-string env override + `dotnet test --logger trx --collect coverage` | **P0** | M | med |
| **Observability (metrics/tracing)** | None (no OTel) | OpenTelemetry (ASP.NET/EF/SqlClient auto-instrumentation) + OTLP/Prometheus exporter | **P1** | L | low |
| **Correlation IDs** | RequestLogs ↔ AuditLog ↔ errors not joinable | Push per-request `TraceId` into LogContext; add `TraceId` column to both; stamp in interceptor | **P1** | M | low |
| **Job/health alerting** | Hangfire failures + health degradation silent | `IElectStateFilter` watchdog → `NotificationService`; alert on `/healthz/ready` degradation | **P1** | M | low |
| **Container health** | No Docker `HEALTHCHECK` | Add `HEALTHCHECK` → `/healthz` | **P2** | XS | low |
| **Log durability** | RequestLogs sink → same SQL DB; no fallback | Add `Serilog.Sinks.File` rolling fallback; ship console off-box | **P2** | XS | low |
| **DB concurrency** | `InventoryBalance` upsert has lost-update race (no rowversion) | Atomic `UPDATE … SET Quantity = Quantity + {delta}` (or rowversion + retry) | **P1** | M | med |
| **Connection resilience** | No `EnableRetryOnFailure` | Add it (verify inline-transaction endpoints tolerate the execution strategy) | **P2** | S | med |
| **SQL TLS** | `TrustServerCertificate=True` everywhere | Prod: `Encrypt=True` with a trusted cert, or document residual risk | **P2** | S | med |
| **API-key hygiene** | Indefinite plaintext fallback; non-constant-time compare; write-on-every-call | Finish hash migration → drop plaintext branch; `FixedTimeEquals`; batch `LastUsedDate` | **P2** | S | med |
| **Soft-delete safety** | No `HasQueryFilter`; per-query `DeletedDate` (often forgotten) | Global query filters + `IgnoreQueryFilters()` admin toggle | **P1** | M | low |
| **Sensitive-field audit** | `ChangesJson` serializes all values (Sales has credit-card entities) | Honor `[Sensitive]`/`[NotLogged]` → mask in payload builders | **P2** | S | low |
| **Validation coverage** | 250 validators, **0** direct tests; UI bypasses them | Validator unit tests (`TestValidate`); run validators on UI write path | **P1** | M | low |
| **Caching @ scale** | `IMemoryCache` per-replica (dashboard skew if scaled out) | Distributed cache (Redis) when horizontally scaled; document 5-min staleness now | **P3** | M | low |
| **Backup/restore** | Nightly SQL backup cron + runbooks present | Verify restore drill; document RPO/RTO | **P2** | S | low |
| **Docs accuracy** | Test/route/step counts stale and self-contradictory | Single source of truth + a CI "docs freshness" count check | **P2** | M | low |

---

## Refactor Roadmap

| Refactor | Why | Files / Areas | Priority | Effort | Risk |
|---|---|---|---|---|---|
| Adopt `CrudPage` + `PageHeader` across `Index` pages | Kills ~100 hand-rolled grids; fixes empty/loading/`<h1>`/breadcrumbs in one place | `Shared/UI/Components/CrudPage.razor`, `PageHeader.razor`, 119 `Features/**/UI/Pages/Index.razor` | P1 | XL (batched) | low |
| Per-entity `IEntityTypeConfiguration` | Removes the 1377-LOC god-file merge bottleneck | `Infrastructure/Persistence/ApplicationDbContext.cs` → per-slice `Domain/*Configuration.cs` | P1 | L | med |
| Validated write path (app service or UI-validator) | Closes the dual-write validation bypass | `Features/**/UI/Pages/*Index.razor` + `*Dialog.razor`; pilot Logistics/MES | P1 | L | med |
| Fix the over-fetching expanded rows | Loads full transaction history (121K-row tables) per expand | `Customer/Product/Vendor/SalesTerritory/SalesPersonExpandedRow.razor` (~5 files) | P1 | M | low |
| Wire `GlobalSearch` to `LookupService` records | Turns nav palette into data discovery; reuses existing search methods | `Shared/UI/Components/GlobalSearch.razor`, `Shared/Services/LookupService.cs` | P1 | M | low |
| Single canonical `EntityCatalog` | De-dupes the table list across 3 files; seeds the data catalog | `DatabaseExplorer.razor`, `ExportEndpoints.cs`, `GlobalSearch.razor` | P2 | M | low |
| Delete dead `CrudEndpointBuilder` (`MapIntIdCrud`, 140 LOC, 0 callers) | Misleads readers about the canonical CRUD path | `Shared/Api/CrudEndpointBuilder.cs` (+ dangling `<see cref>`) | P2 | XS | low |
| Resolve/remove `AuditedSaveExtensions` (0 usages) | Interceptor already audits everything; doc says "use this" | `Infrastructure/Persistence/AuditedSaveExtensions.cs`, CLAUDE.md §8 | P2 | S | low |
| `IPostingTriggerHook.Order` instead of DI registration order | Makes a correctness invariant explicit + testable | `App/Extensions/ServiceRegistration.cs`, `Logistics/Services/LogisticsPostingService.cs` | P2 | S | low |
| Convert ~10 untyped-`Results` endpoints to `TypedResults` | Restores OpenAPI response schemas | `Features/Mes/Api/ShopFloorEndpoints.cs` + ~9 | P2 | S | low |
| Standardize state-conflict errors (409) | Inconsistent: ValidationProblem vs BadRequest<string> | posting/workflow endpoints across domains | P2 | M | low |
| Relocate `/tool-slots` under `/maintenance/tool-slots` | Route matches its home domain | `Features/Maintenance/ToolSlots/UI/Pages` | P3 | S | low |
| Convention-based endpoint/config discovery | Removes the two remaining central coupling points | `EndpointMappingExtensions.cs`, `ApplicationDbContext` | P3 | M | low |
| `Processes` vs `ProcessManagement` boundary | Two domains share `/processes`/`/api/processes` | both domains + routing | P2 | M | med |
| Consolidate test fixtures onto one base | Two WAF setups with different security relaxations | `IntegrationTest.cs` vs `IntegrationTestFixtureBase.cs` | P2 | M | low |
| Archive stale docs | `refactor-plan.md`, `backlog.md`, `reviews/*`, `servicestack-style-patterns.md` are point-in-time/cruft | `docs/` → `docs/history/` | P2 | S | low |

---

## Feature Roadmap

| Feature | User Value | Technical Notes | Priority | Effort | Dependencies |
|---|---|---|---|---|---|
| **Federated record search** | Find a customer/product/order without knowing the schema | Reuse `LookupService` search methods; federate into `GlobalSearch` + `/api/search`; typed deep-linked results | P1 | M | — |
| **Per-record detail + Related/Next-actions rail** | Pivot Customer→Orders→Shipments→Timeline by following links | Generic detail page or `?id=` filter; template already exists in MES/Logistics/etc.; deep-link timeline chips | P1 | L | record search |
| **Data catalog / business glossary** (`/catalog`) | "What is this data, who owns it, what uses it" | Small metadata table seeded from `EntityCatalog` + endpoint catalog; cross-link explorer/record/timeline | P1 | XL | EntityCatalog |
| **Schema relationship map** | See how tables relate without the ERD | Read `INFORMATION_SCHEMA.COLUMNS` + `sys.foreign_keys` (read-only); navigable FK chips in Database Explorer | P2 | L | — |
| **Native AW activity timelines** | Every historical record gets a meaningful "what happened" | Synthesize events from temporal columns + status; `AuditLog` fallback | P1 | M | — |
| **Ctrl+K command/data palette** | One keystroke to anything (pages, records, actions) | Wire the advertised-but-dead Ctrl+K; tiers: pages (from a `PageRegistry`) + records + actions | P2 | M | record search, PageRegistry |
| **Central `PageRegistry`** | One source of truth for nav, search, breadcrumbs, sitemap | route+title+synonyms+category+icon+roles; consumed by NavMenu/GlobalSearch/breadcrumbs/`/all-pages` | P2 | M | — |
| **Role-based landing pages** | Each role lands on relevant work, not a tech blurb | Existing `AuthorizeView Roles` pattern; "Start here" tiles | P2 | M | — |
| **Recently-viewed + favorites/pins** | Return to frequent records/pages fast | Per-user persistence (existing DataProtection/Identity); rail on Home | P2 | M | — |
| **Guided ad-hoc query builder** | Non-technical users ask new questions safely (no SQL) | entity+dimension+measure+filter → parameterized query; replaces the 10-metric enum | P2 | L | — |
| **Admin "Security posture" page** | Surface seed accounts, keys without expiry, stale keys, admins | Reads Identity/ApiKey/SecurityAuditLog; pairs with SecurityAuditLog writes | P2 | M | SecurityAuditLog writes |
| **Security activity viewer** | Filterable trail of auth/security events | Mirror the existing `RequestLog.razor` viewer | P2 | S | SecurityAuditLog writes |
| **Trace explorer** | Given a trace id, show request line + audit rows + error | One-click incident triage | P2 | M | correlation IDs |
| **Job-health watchdog tile** | See when a recurring job silently dies | Hangfire state filter → NotificationService | P1 | M | — |
| **OpenAPI-driven API Explorer + generated client** | Always mirrors all ~118 endpoints; external teams can build | Fetch `/swagger/v1/swagger.json`; group by tag | P2 | M | OpenAPI polish |
| **Self-service API-key lifecycle** | Required expiry, rotation, per-area scope | Extend existing ApiKey management | P3 | M | — |
| **Architecture-fitness tests** | Prevent drift (slice shape, no scoped-DbContext in components, write-path validation) | NUnit assertions over the assembly/routes | P2 | M | CI runs tests |

---

## Quick Wins

Safe, fast, high-signal. Most are XS/S and low-risk:

- **Set `lockoutOnFailure: true`** in `Login.razor` (XS) — activates the already-configured lockout.
- **Guard `SeedUsersAsync` behind `IsDevelopment()`** and rotate the live admin password (S). *(P0 — do with the lockout fix.)*
- **Remove the hardcoded admin password** from the `Demo:AutofillLogin` path; source from config if needed (XS).
- **`AddProblemDetails()`** + branch the exception handler so `/api/*` returns `application/problem+json` (S).
- **Attach `.RequireRateLimiting("api")`** to a parent `/api` group (S).
- **Add a Docker `HEALTHCHECK`** pointing at `/healthz` (XS).
- **Add a `Serilog.Sinks.File` rolling fallback** so logs survive SQL outages (XS).
- **Tighten CSP `connect-src`** from `'self' ws: wss:` to the specific origin(s) (XS).
- **Default `NoRecordsContent = EmptyState` inside `CrudPage`** (XS) — all shell pages gain friendly empty states.
- **Add a `loaded`/`KpiCardSkeleton` guard** to the ~10 module landing pages so KPIs don't flash `0` (S).
- **Render page titles as `<h1>`** (styled like `h4`) in `PageHeader`/`CrudPage` to fix `FocusOnNavigate` (S).
- **Make the Process Timeline `EntityTypeChip` a deep link** to the record (S).
- **Fix `GlobalSearch` legacy route entries** (`aw/products`, `tool-slots`) to canonical routes; wire the advertised Ctrl+K (S).
- **Convert `ForecastsAnalytics` in-memory average to SQL-side**; drop `Virtualize` on server-paged grids (S).
- **Fix `CustomerExpandedRow`/`EmployeeExpandedRow` over-fetch** to aggregate projections (S).
- **Delete dead `CrudEndpointBuilder.cs`** (140 LOC, 0 callers) (XS).
- **Doc quick wins:** "Blazor Server" → "Blazor Web App (mixed)"; replace `213`/`350` with "~340 NUnit cases"; correct the initializer to **8 stages**; list all 6 ADRs; document `Demo:*` flags with the "never in prod" warning; **commit (or deliberately `.gitignore`) `design_handoff_awblazor/`**; add "historical" banners to `phase-plan.md`/`refactor-plan.md`/reviews.

---

## Medium-Term Improvements

Need planning but worth doing this quarter:

- **Federated record search + per-record detail + Related rail** — the three discovery changes that move the product (see that section).
- **`CrudPage`/`PageHeader` adoption + breadcrumbs** across `Index`/detail/history pages, in batches.
- **Validated write path** (per-entity app service or UI-validator) for the highest-risk write slices; wire ProcessManagement dialogs to their existing validators.
- **Per-entity `IEntityTypeConfiguration` extraction**, one domain at a time, snapshot-verified.
- **Security hardening pass:** authz fallback policy + narrowed Employee floor + reconciled models; `SecurityAuditLog` writes + admin viewer; finish API-key hash migration and drop plaintext.
- **Observability foundation:** OpenTelemetry auto-instrumentation, per-request correlation IDs threaded through RequestLogs/AuditLog/errors, job-health watchdog, container `HEALTHCHECK`.
- **CI that runs tests** (mssql service container) + coverage/trx artifacts; first batch of validator unit tests; mark DB-mutating fixtures `[NonParallelizable]`.
- **Data correctness:** `InventoryBalance` concurrency fix; global soft-delete query filters; `EnableRetryOnFailure`.
- **Data catalog + schema relationship map + native AW timelines.**
- **Docs reset:** archive stale plans/reviews; write a generated `feature-map.md` + `api-overview.md`; promote the timeline spec into a durable design doc; add a "docs freshness" CI check.

---

## Long-Term Vision

A polished, enterprise-ready **internal data + process platform** where:

- **Discovery is the default.** A user types a customer, product, or order number and is taken straight to it; from any record they see related records, the KPIs it rolls into, the process chains it belongs to, and the next likely actions — never needing to know a table name. A data catalog answers "what is this and what uses it" in business language.
- **Processes are legible.** Every record — historical or in-app — has a meaningful timeline; chains are authorable; the app explains workflows, not just rows.
- **The platform is trustworthy.** Auth is hardened with a single coherent authorization model and a real security trail; every write is validated once and audited with full request context; OpenTelemetry + correlation IDs make incidents one-click to triage; CI gates every change with a real test suite; the API is a versioned, documented, rate-limited contract external teams can build against.
- **It scales past AdventureWorks.** Because the slice architecture, shared shells, `EntityCatalog`, and discovery layer are schema-agnostic, swapping AdventureWorks for a real business domain is a configuration + catalog exercise, not a rewrite. The same federated search, relationship map, catalog, and timeline light up automatically.
- **It stays consistent as it grows.** Architecture-fitness tests, a central `PageRegistry`, generated docs, and convention-based registration keep drift from re-accumulating.

---

## Recommended First Implementation Sprint

A focused sprint that creates visible improvement without destabilizing the app. Items 1–4 are production-critical; 5–10 are high-value, low-risk wins the codebase already has the parts for. (Items implemented in this session are noted.)

1. **[P0] Lock down auth.** Guard `SeedUsersAsync` behind `IsDevelopment()`, rotate the live admin password, remove the hardcoded `Demo:AutofillLogin` password, and set `lockoutOnFailure: true`. *(½ day, mostly verification + a prod password rotation.)*
2. **[P0] Make CI run the tests.** Add an `mssql` service container + a `ConnectionStrings__DefaultConnection` env override + a `dotnet test --logger trx --collect "XPlat Code Coverage"` step. The good suite that exists starts gating merges. *(1 day.)*
3. **[P1] Global API error contract + rate limiting.** `AddProblemDetails()` + an `IExceptionHandler` for `/api/*`; attach `.RequireRateLimiting("api")` to a parent `/api` group; partition the `"auth"` limiter by IP. *(1 day.)*
4. **[P1] Security audit trail.** `ISecurityAuditService` writing login/lockout/password/role/API-key events; lights up the failed-login alert. *(1–2 days.)*
5. **[P1] Federated record search.** Wire `GlobalSearch` to the existing `LookupService` record searches as a "Records" tier; wire Ctrl+K. The single biggest discovery win. *(1–2 days.)*
6. **[P1] Shared-shell consistency pass.** `CrudPage` renders `PageHeader`; default `EmptyState`; titles as `<h1>`; loading guard on module landing pages. Fixes empty/loading/a11y across the app from one place. *(1–2 days.)*
7. **[P1] Kill the worst over-fetch.** Convert the ~5 full-history expanded-row loaders to aggregate projections + `Take(10)`. *(½ day.)*
8. **[P1] Operational visibility.** Docker `HEALTHCHECK`; Serilog file fallback; per-request `TraceId` in LogContext. *(½ day.)*
9. **[P2] First unit-test layer.** `TestValidate` tests for the 5–10 most-used validators (no SQL Server needed) — establishes the missing fast tier. *(½ day.)*
10. **[Docs] Truth-up the docs** *(done in this session)*: fix the "Blazor Server"/test-count/initializer-step-count/API-route drift, commit the design system, banner the stale plans, and add a docs index for the two new review documents.

> **Net:** roughly a two-week sprint removes every production-blocking defect, harvests a dozen cheap consistency wins, and ships the first real discovery capability — without a single risky rewrite.
