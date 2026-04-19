# Shared

Cross-feature code. Lives here when **two or more** features depend on it.

- `Components/Layout/` — app bar, drawer, main layout.
- `Components/Widgets/` — KpiCard, TimeSeriesChart, GlobalSearch,
  NotificationListener, reusable UI.
- `Domain/` — `AuditableEntity`, base records, value objects.
- `Services/` — cross-cutting singletons: cache, CSV export, lookups,
  number formatting.
- `Endpoints/` — helper extension methods for the minimal-API setup.
- `Utilities/` — small pure helpers with no DI dependency.

**If only one feature uses a thing, put it in that feature's folder** — don't
park it here "in case" something else needs it later. Move it on demand.
