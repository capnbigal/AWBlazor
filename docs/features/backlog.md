# Feature Backlog

Prioritized feature suggestions. Each item has effort estimate (S/M/L) and impact estimate (Low/Med/High).

## Quick wins (S effort, Med-High impact)

### QW1. Generic CRUD endpoint helper (S, High)
Extract `MapCrudEndpoints<TEntity, TDto, TCreate, TUpdate, TAuditLog>` to eliminate 67-fold endpoint duplication. ~7000 LOC removed.

### QW2. ARIA labels audit (S, Med)
Add `aria-label` to every icon-only button across all pages. WCAG compliance + screen-reader UX.

### QW3. Loading state on list pages (S, Med)
Add `<MudProgressLinear Indeterminate="true" />` to every grid container during initial load. Currently dialogs disable save button but list pages have no visual loading cue.

### QW4. Lookup cache TTL upgrade (S, Med)
`AddressTypes`, `Cultures`, `Currencies`, `UnitMeasures` are reference data that never change. Bump TTL to 1 hour with admin cache-clear endpoint.

### QW5. Magic strings → constants (S, Low)
Extract hardcoded product line/class/style codes (`ProductDialog.razor:62-82`) to `ProductConstants` static class.

### QW6. Per-user notification preferences (S, Med)
User-toggleable: dark mode (already done), email digests, snackbar position, default page size for grids.

## Roadmap items (M effort, High impact)

### R1. Source-generated audit services (M, Med)
Use Roslyn source generators to produce the 67 `{Entity}AuditService.RecordCreate/Update/Delete` methods from a single attribute on the entity. Eliminates ~70 boilerplate files.

### R2. Unified `Result<T>` envelope (M, Med)
Standard response shape for all endpoints. Improves OpenAPI contract consistency and makes API consumers' lives easier. Defer until external API consumers exist.

### R3. Component test suite via bUnit (M, Med)
Currently zero unit tests for components. Start with `KpiCard`, `EmptyState`, `GlobalSearch`, then expand to dialogs.

### R4. Activity dashboard split (M, Low)
`Components/Pages/Admin/Activity.razor` is 454 lines. Split into `<ActivityCharts>`, `<ActivityTable>`, `<ActivityFilters>` for testability and reuse.

### R5. Real-time notifications via SignalR (M, High)
NotificationHub already exists. Wire up: when admin creates an announcement, push to all active circuits. When a long-running forecast finishes, notify the originating user.

### R6. CSV/Excel export across all grids (M, High)
`TimeSeriesChart` already has CSV download. Generalize: every `MudDataGrid` gets an "Export visible rows" button that exports the current filtered+sorted view to CSV.

### R7. Audit log viewer in admin (M, Med)
A page that lets an admin search across all `*AuditLog` tables by user, date range, action, entity. Currently audit history is per-entity-page only.

### R8. API key expiration enforcement (M, Med)
Already supported in the data model — wire up the UI to set expiration dates at creation. Add a daily Hangfire job that emails users when their keys are 7 days from expiry.

### R9. Saved filters per user (M, High)
On any list page, let users save their current filter+sort+columns combination as a named view. Store in user preferences table.

### R10. Bulk operations on grids (M, High)
Multi-select rows + apply bulk actions (delete, change status, export). Currently every operation is single-row.

## Larger initiatives (L effort, High impact)

### L1. Tenant/multi-org support (L, Variable)
Add `TenantId` to relevant entities, query filters via `HasQueryFilter`, tenant resolution middleware. Massive change — only do this if there's a real demand.

### L2. Workflow engine for processes (L, High)
The `Processes` module exists but is basic. Build a real workflow engine: branching, conditions, approvals, timeouts, scheduled triggers.

### L3. Mobile-friendly app shell (L, Med)
Audit every page for mobile responsiveness, add a mobile-optimized navigation drawer, test on iOS/Android. Most grids are too dense for phones currently.

### L4. AI-assisted forecast suggestion (L, High)
For the Forecasts module: integrate Anthropic API to recommend the best algorithm + parameters based on historical data shape.

### L5. Email template management (L, Med)
Admin UI to edit email templates (currently hardcoded in code). Use Razor or a templating library.

### L6. Audit log analytics dashboard (L, Med)
Pull from `*AuditLog` tables to show: most-modified entities, most-active users, anomalous activity (e.g. mass deletions).

### L7. Plugin/module system (L, Med)
Allow new feature modules to be added by dropping a DLL in a folder. Useful if this app gets customized for different deployments.

## Future ideas (don't build yet)

### F1. WebAssembly migration for analytics
The analytics dashboards do a lot of client-side filtering after data loads. Migrating just those pages to Interactive WebAssembly + offline cache would dramatically improve perceived perf for power users.

### F2. GraphQL endpoint
Optional alternative to the 67 REST endpoints. Hot Chocolate library makes this trivial. Defer until consumers ask.

### F3. OpenTelemetry observability
Replace/augment Serilog with OTel for distributed tracing. Useful when this app calls out to other services or has perf bottlenecks.

### F4. Read replica for analytics
Point analytics queries at a read replica to isolate dashboard load from transactional load.

### F5. Database backups + restore admin UI
Currently relies on external backup. Add a daily automated backup + an admin "restore from backup" button (with multiple approvals).

### F6. Localization (i18n)
.NET localization works fine; the question is whether there's demand for non-English UIs.

---

## What NOT to build

These have been considered and rejected — don't propose them again without new context:

- **SignalR-based collaborative editing** — over-engineering for a CRUD app
- **Custom UI library** — MudBlazor is mature and well-maintained; building one is a multi-year project
- **Microservices split** — codebase is well-organized as a monolith; splitting just adds operational complexity
- **Replace EF Core with Dapper** — EF Core 10 perf is fine for this workload
- **Replace Hangfire with Quartz** — Hangfire works; switching costs are not worth it
- **Replace MudBlazor with Tailwind/HeadlessUI** — explicitly off the table per CLAUDE.md
