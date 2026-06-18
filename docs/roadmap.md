# Roadmap — Enterprise Data & Process Platform

Single living status doc for the enterprise evolution described in
[`IMPLEMENTATION_PLAN_ENTERPRISE_APP.md`](./IMPLEMENTATION_PLAN_ENTERPRISE_APP.md) (the *how*) and
[`GROUND_UP_APP_REVIEW.md`](./GROUND_UP_APP_REVIEW.md) (the findings). This file tracks what has
shipped, what is in flight, and what is deliberately deferred (with the reason and the safe way to
do it later). Update it as items land.

Legend: ✅ done · 🟡 partial · ⛔ deferred (reason in the row).

---

## Shipped (recent PRs)

| Item | Plan ref | Notes |
|---|---|---|
| ✅ Federated record search | Sprint 2 #8 | `SearchService` over `LookupService`; app-bar `GlobalSearch` surfaces Products, Customers, Sales/Purchase orders, Vendors, Employees, Persons, Work orders. |
| ✅ Per-record detail pages | Sprint 3 #14 | `/aw/<entity>/{id}` for Products, Customers, Sales orders, Vendors, Employees, Persons, Work orders, Purchase orders. |
| ✅ Deep-linked cross-entity links | Sprint 3 #14 | Expanded rows, detail related sections, and landing worklists point at specific records; every detail-page grid has a "View details" row action. |
| ✅ `PageHeader` revived (h1) + `EmptyState` defaults | Sprint 2 #7 | Title renders as semantic `<h1>`; `CrudPage` defaults `NoRecordsContent` to `EmptyState`. |
| ✅ Analytics nav surfaced | Nav #5 | `analytics/*` under an Insights "Analytics" group; `ErrorBoundary` + friendly error pages. |

## In this PR (`feat/enterprise-plan-finish`)

| Item | Plan ref | Notes |
|---|---|---|
| ✅ Account lockout on failed login | Sprint 1 #2 | `lockoutOnFailure: true`; lockout options were already configured but never engaged. Removed the hardcoded `Demo:AutofillLogin` credentials. |
| ✅ Security audit log + dead-alert revival | Sprint 1 #5 | `ISecurityAuditService` writes login/lockout (+ password/api-key/role constants) to `SecurityAuditLog`, lighting up the previously-dead `FailedLoginsLast24h` notification metric. |
| ✅ RFC-7807 problem+json for `/api/*` | Sprint 1 #3 / API | `AddProblemDetails()` + `ApiProblemDetailsMiddleware`: `DbUpdateException`→409, validation→400, else 500; non-`/api` falls through to the existing `/Error` page. |
| ✅ Rate limiting actually applied to `/api` | Sprint 1 #3 | `GlobalLimiter` throttles `/api/*` (100/min by API key, else IP); `"auth"` now partitioned by client IP instead of one shared bucket. |
| ✅ Related-records rail + synthesized activity timelines | Sprint 3 #15 / Explorer #5 | `RelatedLinksCard` + `RecordActivityTimeline` + the pure `RecordActivity` synthesizer (temporal-column → lifecycle events) on Sales/Purchase/Work-order/Employee/Customer detail pages. |
| ✅ FK ids resolved to names | Explorer | Work order scrap-reason, purchase-order employee + ship-method shown as names. |
| ✅ Role-aware Home public landing | Nav #3 | Replaced the dev-jargon "Blazor Server / .NET 10" blurb with a value-prop + sign-in CTA. |
| ✅ Fast unit-test tier | Testing #2 | `[Category("Unit")]` `TestValidate` tests (Customer/WorkOrder/PurchaseOrder validators) + `RecordActivity` tests — no SQL, run in ~100 ms. `[NonParallelizable]` on the DB-mutating fixtures. |

---

## Deferred — needs a live environment (not verifiable from this workstation)

| Item | Plan ref | Why deferred / how to do it safely |
|---|---|---|
| ⛔ CI runs the test suite | Testing #1 | Needs a CI `mssql` service container + a `ConnectionStrings__DefaultConnection` env override wired into the test fixtures. Add a GitHub Actions workflow with a health-gated `mcr.microsoft.com/mssql/server` service and `dotnet test --logger trx --collect "XPlat Code Coverage"`. **This is the gating prerequisite for everything else — do it first.** |
| ⛔ OpenTelemetry + OTLP/Prometheus export | Sprint 4 #20 | Needs a collector/endpoint to export to. Wire `AddOpenTelemetry().WithTracing/WithMetrics` behind a config flag with a no-op default so local runs aren't affected; turn on the exporter only where a collector exists. |
| ⛔ Docker `HEALTHCHECK` + Serilog file fallback + prod TLS | Sprint 1 #5 / Security #7 | Validated only against the DigitalOcean droplet. Add `HEALTHCHECK` hitting `/healthz`, a Serilog rolling-file sink fallback, and `Encrypt=True` with a trusted cert (drop `TrustServerCertificate`) as a deployment-side change. |

## Deferred — too breaking for one PR (needs a coordinated change)

| Item | Plan ref | Why deferred / how to do it safely |
|---|---|---|
| ⛔ `/api/v1` version prefix | API | Renaming every `/api/...` group to `/api/v1/...` breaks existing API-key clients, the in-app fetch calls, and the endpoint tests at once. Do it as its own PR: introduce `/api/v1` as the parent group, update callers + tests, then add a redirect/alias from the unversioned paths. |
| ⛔ Authorization `FallbackPolicy` + narrowed Employee floor | Sprint 3 #13 | Adding a global auth requirement + dropping the Employee permission floor to "None + explicit grants" risks locking real users out of pages they currently reach. Stage it: add the explicit per-area Read grants first, verify against each role, *then* flip the floor — behind a feature flag with an "explain access" admin tool. |
| ⛔ Per-entity `IEntityTypeConfiguration` extraction | Folder org / Sprint 4 #22 | Moving ~90 entities' fluent config out of `ApplicationDbContext.OnModelCreating` is large and changes the model snapshot; risky to bundle. Do one domain at a time, each snapshot-verified, in its own commit. |
| ⛔ Global soft-delete query filters | Sprint 2 #10 | Needs an audit of which entities actually have an `IsDeleted`/`DeletedDate` column before adding `HasQueryFilter`, plus an `IgnoreQueryFilters()` admin toggle. Scope per-entity. |

## Still open (safe, incremental — pick up next)

| Item | Plan ref |
|---|---|
| 🟡 Breadcrumbs everywhere via `PageHeader` | Nav #2 |
| 🟡 Central `PageRegistry` (one source for nav/search/breadcrumbs/`/all-pages`) | Nav #1 |
| 🟡 `EntityCatalog` + `/catalog` data glossary; schema relationship map | Explorer #3–4 |
| 🟡 Validated write path (`Result<T>` service shared by endpoint + UI) on the highest-risk slices | Services/API |
| 🟡 Parallelize the dashboard count round-trips on cache-miss | Dashboards |
| 🟡 HTTP-level CRUD round-trip tests per domain; bUnit tests for shared primitives | Testing #3, #5 |
| 🟡 Migrate hand-rolled `Index` pages onto `CrudPage` in batches | Components |

---

_Re-baseline after CI is gating (Testing #1): the existing integration suite will then catch
regressions that the rest of this roadmap might otherwise introduce._
