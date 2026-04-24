# Scheduling — Slice 1 Design

**Date:** 2026-04-23
**Status:** Approved, ready for implementation planning
**Scope:** First end-to-end slice of `Features/Scheduling`

## Problem

AdventureWorks lacks a sales-order-driven line-scheduling control plane. The
factory runs takt-time mixed-model assembly; incoming `SalesOrderHeader` rows
drive per-line demand, and the delivery sequence must adapt to kitting
shortages, hot orders, and customer changes rather than running a frozen
weekly batch. Slice 1 proves the control plane end-to-end on one line before
material sequencing (`Features/MaterialSequencing`, separate slice) begins.

## Goals

1. One product line (`Production.Location` ID 60, Final Assembly), one week
   horizon, end-to-end.
2. Monday baseline + live current-schedule derivation model, with observable
   drift between the two.
3. Rule-dispatched recalc subsystem reacting to new `SalesOrderHeader`
   inserts, with three seed rules covering the dispatch framework.
4. Planner UI for plan generation, live delivery schedule, exceptions, and an
   alert triage queue.

## Non-goals

Explicitly deferred to later slices so the design stays honest:

- BOM cascade (Final Assembly → Subassembly → Frame Welding). Hooks into
  `Production.WorkOrderRouting` later; slice 2.
- `Features/MaterialSequencing`. Separate folder, separate slice.
- Floor-execution capture (actual start/end from shop floor). Slice 3.
- Monday cron freeze. Same code path as the manual generate button, wired
  later.
- Rules-admin UI polish. Slice 1 ships seed rules and the raw `SchedulingRule`
  table; no dedicated editor.
- Multi-line / multi-week / routing matrix.

## Architectural invariants

Three non-negotiables. The whole design protects these.

1. **`WeeklyPlanItem` is immutable once its week has started.** Takt changes,
   SO cancellations, in-week replans, and drift events never mutate existing
   items — new `WeeklyPlan.Version` = new rows. Future-week plans
   (`WeekStart > today`) are *not* yet live baselines; regenerating them
   hard-deletes the old plan and items (see Generation §2). The invariant
   protects *drift-observability*, which only applies once real execution is
   drifting against a plan.
2. **Current schedule is derived, never persisted as mutable state.**
   `vw_CurrentDeliverySchedule` is the authoritative "now." A sparse
   `SchedulingException` table stores human-intent overrides only (manual
   pins, kitting holds, hot-order bumps). No mutable mirror of the plan.
3. **Dispatcher runs inside the SO-insert transaction.** A rule failure rolls
   the SO back. Re-entrancy is prevented by an `AsyncLocal<bool>` guard so
   dispatcher-triggered writes don't recursively re-fire dispatch;
   `AuditLogInterceptor` still captures those writes.

## Folder layout

```
Features/Scheduling/
  WeeklyPlans/         { Api, Application, Domain, Dtos, UI }
  DeliverySchedules/   { Api, Application, Domain, Dtos, UI }
  Rules/               { Api, Application, Domain, Dtos, UI }
  LineConfigurations/  { Api, Application, Domain, Dtos, UI }
  LineProductAssignments/ { Api, Application, Domain, Dtos, UI }
  Alerts/              { Api, Application, Domain, Dtos, UI }
  Services/            (dispatcher, interceptor, evaluator, resolver)
```

All tables use a new SQL `Scheduling` schema (not `dbo`).

## Data model

### `Scheduling.LineConfiguration`

Sidecar to `Production.Location`. Only locations with an active
`LineConfiguration` are eligible for scheduling.

| Column | Type | Notes |
|---|---|---|
| `LineConfigurationID` | `int IDENTITY` PK | |
| `LocationID` | `smallint` | FK → `Production.Location`, unique |
| `TaktSeconds` | `int` | Changes 1–2× per year; `AuditLogInterceptor` captures history |
| `ShiftsPerDay` | `tinyint` | 1–3 |
| `MinutesPerShift` | `smallint` | |
| `FrozenLookaheadHours` | `int` | Default 72 |
| `IsActive` | `bit` | Soft-disable without deleting |
| `ModifiedDate` | `datetime2` | |

### `Scheduling.WeeklyPlan`

One row per `(WeekId, LocationID, Version)`. Append-only.

| Column | Type | Notes |
|---|---|---|
| `WeeklyPlanID` | `int IDENTITY` PK | |
| `WeekId` | `int` | ISO year × 100 + week (`202618` = 2026-W18) |
| `LocationID` | `smallint` | FK → `Production.Location` |
| `Version` | `int` | 1, 2, 3… bumped on in-week regeneration |
| `PublishedAt` | `datetime2` | UTC |
| `PublishedBy` | `nvarchar(256)` | Identity username |
| `BaselineDiverged` | `bit` | Set by `HardReplanAction` |
| `GenerationOptionsJson` | `nvarchar(max)` | `StrictCapacity`, sort weights |
| Unique | `(WeekId, LocationID, Version)` | |

### `Scheduling.WeeklyPlanItem`

The frozen baseline. **Never updated after INSERT.**

| Column | Type | Notes |
|---|---|---|
| `WeeklyPlanItemID` | `int IDENTITY` PK | |
| `WeeklyPlanID` | `int` | FK |
| `SalesOrderID` | `int` | FK → `Sales.SalesOrderHeader` |
| `SalesOrderDetailID` | `int` | FK → `Sales.SalesOrderDetail` |
| `ProductID` | `int` | Denormalized for fast view joins |
| `PlannedSequence` | `int` | 1..N within plan |
| `PlannedStart` | `datetime2` | **Absolute UTC** (not computed-on-read) |
| `PlannedEnd` | `datetime2` | Absolute UTC |
| `PlannedQty` | `smallint` | From `SalesOrderDetail.OrderQty` at freeze time |
| `OverCapacity` | `bit` | Planned past week's capacity window |

Index: `(WeeklyPlanID, PlannedSequence)`; `(SalesOrderDetailID)` for view joins.

### `Scheduling.SchedulingException`

Sparse overlay. One row only where a human overrides the derived schedule.

| Column | Type | Notes |
|---|---|---|
| `SchedulingExceptionID` | `int IDENTITY` PK | |
| `WeekId` | `int` | |
| `LocationID` | `smallint` | |
| `SalesOrderDetailID` | `int` | FK — item being overridden |
| `ExceptionType` | `tinyint` | `ManualSequencePin=1`, `KittingHold=2`, `HotOrderBump=3` |
| `PinnedSequence` | `int NULL` | Only for `ManualSequencePin` |
| `Reason` | `nvarchar(500)` | Required — planner must state why |
| `CreatedAt`, `CreatedBy` | | |
| `ResolvedAt`, `ResolvedBy` | `NULL` | Null while active |

Filtered unique index: `(WeekId, LocationID, SalesOrderDetailID)
WHERE ResolvedAt IS NULL` — one active override per item.

Filtered index for the view: `(WeekId, LocationID) INCLUDE
(SalesOrderDetailID, ExceptionType, PinnedSequence) WHERE ResolvedAt IS NULL`.

### `Scheduling.SchedulingRule`

Dispatch table. Key is `(EventType, InFrozenWindow)`.

| Column | Type | Notes |
|---|---|---|
| `SchedulingRuleID` | `int IDENTITY` PK | |
| `EventType` | `tinyint` | `NewSO=1` (slice 1 only) |
| `InFrozenWindow` | `bit` | Dispatch key |
| `Action` | `tinyint` | `SoftResort=1`, `AlertOnly=2`, `HardReplan=3` |
| `ParametersJson` | `nvarchar(max)` | e.g. `{"minOrderValue":5000}` |
| `Priority` | `int` | Tiebreaker; higher runs first |
| `IsActive` | `bit` | |

Seed rows (3):
- `(NewSO, InFrozenWindow=false, SoftResort, null, priority=100)`
- `(NewSO, InFrozenWindow=true, AlertOnly, '{"minOrderValue":5000}', priority=100)`
- `(NewSO, InFrozenWindow=true, HardReplan, null, priority=50)` — fallback

### `Scheduling.SchedulingAlert`

Planner triage queue. Dedicated table (not Serilog) because alerts are
actionable domain state, not an ops log.

| Column | Type | Notes |
|---|---|---|
| `SchedulingAlertID` | `int IDENTITY` PK | |
| `CreatedAt` | `datetime2` | |
| `Severity` | `tinyint` | `Info=1`, `Warning=2`, `Critical=3` |
| `EventType` | `tinyint` | Mirrors `SchedulingRule.EventType` |
| `WeekId`, `LocationID` | | |
| `SalesOrderID` | `int NULL` | |
| `Message` | `nvarchar(1000)` | |
| `PayloadJson` | `nvarchar(max)` | Structured context |
| `AcknowledgedAt`, `AcknowledgedBy` | `NULL` | |

Index: `(AcknowledgedAt, CreatedAt DESC)` for the triage queue.

### `Scheduling.LineProductAssignment`

Explicit `Product.ProductModel → Location` mapping. Slice 1 seeds bike models
→ Location 60; slice 2 adds a CrudPage for multi-line editing.

| Column | Type | Notes |
|---|---|---|
| `LineProductAssignmentID` | `int IDENTITY` PK | |
| `LocationID` | `smallint` | FK → `Production.Location` |
| `ProductModelID` | `int` | FK → `Production.ProductModel` |
| `IsActive` | `bit` | |
| `ModifiedDate` | `datetime2` | |
| Unique | `(LocationID, ProductModelID)` | |

### `Scheduling.vw_CurrentDeliverySchedule`

SQL view created via raw-SQL migration (EF does not generate views). One row
per planned-or-current delivery item for a `(WeekId, LocationID)`.

**Output columns:**

```
WeekId, LocationID, SalesOrderID, SalesOrderDetailID, ProductID,
PlannedSequence, PlannedStart, PlannedEnd, PlannedQty,
CurrentSequence, CurrentStart, CurrentEnd, CurrentQty,
SequenceDrift, StartDriftMinutes,
PromiseDate, PromiseDriftMinutes,
ExceptionType, ExceptionReason,
SoStatus, IsCancelled, IsHotOrder
```

**Derivation logic (inside view body):**

1. Start from latest-`Version` `WeeklyPlanItem` rows for
   `(WeekId, LocationID)`.
2. LEFT JOIN current `SalesOrderHeader` / `SalesOrderDetail`. Cancelled rows
   still appear with `IsCancelled=1` so drift is visible.
3. LEFT JOIN active `SchedulingException` (`ResolvedAt IS NULL`).
4. `CurrentSequence` = `ROW_NUMBER()` over the remaining non-cancelled items,
   ordered by:
   ```
   PinnedSequence asc NULLS LAST,
   ExceptionType=HotOrderBump desc,
   SalesOrderHeader.DueDate asc,
   (CASE WHEN SalesOrderHeader.OnlineOrderFlag = 0 THEN 1 ELSE 0 END) desc,  -- CustomerPriority proxy
   Product.ProductModelID asc,                                               -- view joins Product to reach model
   SalesOrderHeader.TotalDue desc,
   SalesOrderHeader.ModifiedDate asc,
   SalesOrderDetailID asc
   ```
   Same 5-factor key as generation, with pins and bumps overlaid.
5. `CurrentStart`/`CurrentEnd` computed fresh at query time using the same
   `weekStart + Σ(takt × CurrentQty[..k-1])` math as generation, honoring
   `ShiftsPerDay × MinutesPerShift` overflow wrapping.

## Event flow & dispatcher

Synchronous, inside the SO-insert transaction.

```
SalesOrderHeader INSERT
        │
        ▼
ApplicationDbContext.SaveChangesAsync()
        │
        ▼
[Interceptor chain, deterministic order]
  1. AuditLogInterceptor         — SavingChanges phase
  2. SchedulingDispatchInterceptor — SavedChanges phase
        │
        ▼
SchedulingDispatcher.OnSalesOrderCreated(soh)
        │
        ▼
resolve (EventType=NewSO, InFrozenWindow=?) → ordered rule list
        │
        ▼
walk rules in priority order until one reports Handled=true
        │
        ▼
  ├── SoftResortAction     → insert SchedulingAlert(Info)
  ├── AlertOnlyAction      → check minOrderValue; if below threshold,
  │                          Handled=false (falls through); else
  │                          insert SchedulingAlert(Warning)
  └── HardReplanAction     → insert SchedulingAlert(Critical) +
                              update WeeklyPlan.BaselineDiverged=1
        │
        ▼
transaction commits (or rolls back as a unit on any throw)
```

### Key mechanics

**Interceptor ordering.** `AuditLogInterceptor` uses *both* phases: it
records pending changes in `SavingChanges` (to see the entity set before EF
clears it), and patches audit-row `EntityId` values in `SavedChanges` once
IDENTITY keys are assigned. `SchedulingDispatchInterceptor` only implements
`SavedChanges` — it needs the SO's generated `SalesOrderID`. Registration
order in `ApplicationDbContext.OnConfiguring` is `AuditLogInterceptor` first,
`SchedulingDispatchInterceptor` second, so `SavedChanges` runs audit-patch
before dispatch. (The dispatcher's own writes — alerts, baseline-diverged
flag — go through a separate `SaveChanges` call on the same context, which
re-enters `AuditLogInterceptor` normally and is blocked from re-entering
`SchedulingDispatchInterceptor` by the re-entrancy guard below.)

**Re-entrancy guard.** The dispatcher sets an `AsyncLocal<bool>` flag before
invoking an action and clears it in `finally`. The interceptor's first check
is `if (_dispatching.Value) return;`. Dispatcher-originated writes still flow
through `AuditLogInterceptor` normally.

**Rule resolution.** `ISchedulingRuleResolver.Resolve(eventType,
inFrozenWindow)` returns rules ordered by
`Priority DESC, SchedulingRuleID ASC`. The dispatcher walks the list and
stops when an action reports `Handled=true`. Parameterized rules
(e.g., `AlertOnly` with `minOrderValue`) self-skip by returning
`Handled=false` when their predicate fails — the resolver is unaware of
per-rule predicates.

**`InFrozenWindow` evaluation.**

```csharp
bool InFrozenWindow(SalesOrderHeader soh, DateTime nowUtc)
{
    var weekId = IsoWeek.FromDate(soh.DueDate);
    var planExists = db.WeeklyPlans.Any(p => p.WeekId == weekId && p.LocationID == 60);
    if (!planExists) return false;
    var hoursUntilDue = (soh.DueDate - nowUtc).TotalHours;
    var lookahead = db.LineConfigurations.Single(l => l.LocationID == 60).FrozenLookaheadHours;
    return hoursUntilDue < lookahead;
}
```

A SO is "in frozen window" iff (a) a `WeeklyPlan` exists for its `DueDate`'s
week AND (b) the SO's `DueDate` is inside `FrozenLookaheadHours` of now.
Conjunction — week-level alone is too coarse, pure lookahead alone is too
loose.

Results are cached per-dispatch-call; a single `SaveChanges` processing N
added SOs performs one plan-existence check, not N.

**Failure policy.** Any `IRecalcAction` exception propagates out of
`SavedChangesAsync`. Because all SO-creating code paths run inside the
transaction opened by `AddWithAuditAsync`, the throw disposes the transaction
and rolls back the SO insert. Invariant: never "SO exists but schedule is
stale."

**Transaction boundary.** Relies on `AddWithAuditAsync` already wrapping SO
creation in an `IDbContextTransaction`. Dispatcher writes participate in the
same transaction. Any new SO-creating code path MUST use `AddWithAuditAsync`
or its own explicit `BeginTransactionAsync` — this is a conventions note.

**Absence guard.** If the affected line has no `LineConfiguration`, the
dispatcher logs at Debug and returns. Scheduling is opt-in per location;
absence of configuration is valid, not an error.

### File list

- `Features/Scheduling/Services/ISchedulingDispatcher.cs` + `SchedulingDispatcher.cs`
- `Features/Scheduling/Services/SchedulingDispatchInterceptor.cs`
- `Features/Scheduling/Services/IFrozenWindowEvaluator.cs` + impl
- `Features/Scheduling/Services/ISchedulingRuleResolver.cs` + impl
- `Features/Scheduling/Rules/Application/IRecalcAction.cs`
- `Features/Scheduling/Rules/Application/SoftResortAction.cs`
- `Features/Scheduling/Rules/Application/AlertOnlyAction.cs`
- `Features/Scheduling/Rules/Application/HardReplanAction.cs`
- Registration: `App/Extensions/ServiceRegistration.cs`
- Interceptor wiring: `ApplicationDbContext.OnConfiguring`

## Weekly plan generation

Same code path used by the manual "Generate Plan" button today and the
Monday cron later.

### Contract

```csharp
public interface IWeeklyPlanGenerator
{
    Task<WeeklyPlanGenerationResult> GenerateAsync(
        int weekId,
        short locationId,
        WeeklyPlanGenerationOptions options,
        string requestedBy,
        CancellationToken ct);
}

public sealed record WeeklyPlanGenerationOptions(
    bool StrictCapacity = false,
    bool DryRun = false,
    IReadOnlySet<byte>? PlannableSalesOrderStatuses = null, // default: InProcess (1) + Approved (5)
    SortWeights? SortWeights = null);                        // null → defaults

public sealed record WeeklyPlanGenerationResult(
    int WeeklyPlanId,
    int Version,
    int ItemCount,
    int OverCapacityCount,
    decimal UtilizationPercent,
    IReadOnlyList<string> Warnings);
```

### Algorithm

Single pass, inside a transaction opened by `AddWithAuditAsync`.

1. **Precondition.** `LineConfiguration` exists and `IsActive=1` for
   `locationId`. Else `InvalidOperationException`.
2. **Regeneration decision.**
   - No existing plan → insert at `Version = 1`.
   - Existing plan AND `WeekStart > today` → **hard-delete** old
     `WeeklyPlan` + its items, reinsert at `Version = 1`. (Future-planning
     loop stays clean.)
   - Existing plan AND `WeekStart ≤ today` → bump to
     `Version = MAX(Version) + 1`. Old rows preserved. (Audit for mid-week
     replans.)
3. **Pull SO scope.** `SalesOrderHeader` rows where
   `DueDate` is in ISO week `WeekId` AND
   `Status` ∈ `options.PlannableSalesOrderStatuses` AND
   has ≥1 `SalesOrderDetail` whose `Product.ProductModelID` is in
   `LineProductAssignment` for `locationId` with `IsActive=1`.
4. **Explode to detail level.** One `WeeklyPlanItem` per qualifying
   `SalesOrderDetail`.
5. **Sort** (5-factor + final tiebreak):
   ```
   DueDate asc, CustomerPriority desc, ProductModelID asc,
   TotalDue desc, ModifiedDate asc, SalesOrderDetailID asc

   where CustomerPriority = CASE WHEN SalesOrderHeader.OnlineOrderFlag = 0 THEN 1 ELSE 0 END
   (dealer/reseller orders rank above online retail — slice 1 proxy;
   slice 2 may replace with a real customer-tier column when needed)
   ```
6. **Assign sequence + absolute timestamps.**
   - `weekStart = IsoWeek.ToMondayUtc(weekId)` (Monday 00:00 UTC)
   - `duration[k] = OrderQty[k] × TaktSeconds`
   - `plannedStart[k] = weekStart + Σ duration[1..k-1]`, advancing a cursor
     through working windows of `ShiftsPerDay × MinutesPerShift × 60`
     seconds per day, wrapping to next day's window start on overflow.
   - Items are **not** split across shifts; `plannedEnd[k] = plannedStart[k]
     + duration[k]` even if it crosses a shift boundary. Simplification
     slice 2 can refine.
7. **Overflow flagging.** `OverCapacity = plannedStart > weekEnd`. If
   `options.StrictCapacity && any(OverCapacity)`, throw
   `CapacityExceededException(excessHours)` before any INSERT.
8. **Bulk INSERT** `WeeklyPlan` + `WeeklyPlanItem` via `AddWithAuditAsync`.
   Audit logs the insert set.
9. **DryRun.** If `options.DryRun`, compute steps 1–7 and return result
   without inserting. Used by the "preview before generate" dialog.
10. **Return** `WeeklyPlanGenerationResult`.

### Files

- `Features/Scheduling/WeeklyPlans/Application/IWeeklyPlanGenerator.cs` + impl
- `Features/Scheduling/WeeklyPlans/Application/SortWeights.cs`
- `Features/Scheduling/WeeklyPlans/Application/WeeklyPlanGenerationOptions.cs`
- `Features/Scheduling/WeeklyPlans/Application/WeeklyPlanGenerationResult.cs`
- `Features/Scheduling/WeeklyPlans/Api/WeeklyPlanEndpoints.cs`
- `Features/Scheduling/WeeklyPlans/Domain/CapacityExceededException.cs`

### API

```
POST /api/scheduling/weekly-plans/generate
GET  /api/scheduling/weekly-plans?weekId=&locationId=
```

Policy `"ApiOrCookie"` + roles `Admin, Planner`.

## UI

Five pages under `/scheduling/*`, all `InteractiveServer` render mode
(no Identity scaffolding involved, so the static-SSR MudBlazor-input
restriction does not apply). Policy `"ApiOrCookie"` + roles `Admin, Planner`.

### `/scheduling/lines` — `LineConfiguration` CrudPage

Straight CrudPage over `LineConfiguration`. Columns: `Location`,
`TaktSeconds`, `ShiftsPerDay`, `MinutesPerShift`, `FrozenLookaheadHours`,
`IsActive`. Create dialog's location picker is limited to locations that
don't already have a row. Matches the 51 existing CrudPage rollouts in
pattern.

### `/scheduling/lines/{locationId}/products` — `LineProductAssignment` CrudPage

Nested under the line. Columns: `ProductModel`, `IsActive`. Create dialog
filters `Production.ProductModel` to unassigned models for the current line.
Entry point is from the Lines page row action.

### `/scheduling/weekly-plans` — plan history + generate

Not a plain CrudPage. Custom page with a generate action and version
stacking.

- Top row: `MudAutocomplete` line picker + ISO week picker + `Generate Plan`
  button.
- Clicking Generate opens a confirm dialog that first calls
  `POST /api/scheduling/weekly-plans/generate` with `DryRun=true`. The
  response populates the preview (SO count, total takt-hours, capacity %).
  Confirming re-posts with `DryRun=false`.
- `MudDataGrid` grouped by `(WeekId, LocationID)` showing all versions:
  `PublishedAt`, `PublishedBy`, `ItemCount`, `OverCapacityCount`,
  `UtilizationPercent`, `BaselineDiverged` badge.
- Row expansion (`<HierarchyColumn>` + `<ChildRowContent>`) shows the items
  of the plan version: read-only grid of `WeeklyPlanItem` columns.

### `/scheduling/delivery` — the live delivery schedule (headline view)

`CrudPage` over `vw_CurrentDeliverySchedule`. Planner's main working surface.

**Filter bar:** line selector, week picker, "hide resolved exceptions" toggle.

**Columns:**

| Column | Source | Rendering |
|---|---|---|
| Seq (planned / current) | `PlannedSequence` / `CurrentSequence` | `"4 → 2"` with arrow on drift, `"4"` otherwise |
| Start (planned / current) | `PlannedStart` / `CurrentStart` | Same drift-arrow pattern; local time |
| SO | `SalesOrderID` | `MudLink` to `/sales/orders/{id}` |
| Product | `ProductID` | `MudLink` to product page |
| Qty | `CurrentQty` (fallback `PlannedQty` if cancelled) | |
| Promise Δ | `PromiseDriftMinutes` | Green ≤ 0, amber ≤ 2h late, red > 2h |
| Exception | `ExceptionType` + `ExceptionReason` | `MudChip` with type color; click opens override dialog |
| Status | `SoStatus`, `IsCancelled`, `IsHotOrder` | Compact flag stack |

**Row actions** (each writes `SchedulingException`, never the view):
`Pin sequence…`, `Mark kitting hold…`, `Mark hot order…`, `Clear exception`.
All route through `POST /api/scheduling/exceptions`; the view reflects on
next refresh. No "save grid" affordance — each action is its own
transactional write.

**KPI cards** (reuse `Shared/UI/Components/KpiCard`): items planned, items
drifted, overcapacity count, open exceptions, open alerts.

### `/scheduling/alerts` — triage queue

`CrudPage` over `SchedulingAlert`. Columns: `CreatedAt`, `Severity`
(colored chip), `EventType`, `Week`, `Line`, `SO` (link), `Message`,
`AcknowledgedAt`. Row action `Acknowledge` sets `AcknowledgedAt` and
`AcknowledgedBy`. Default filter: unacknowledged only.

### Cross-cutting

- New top-level `Scheduling` menu group in `MainLayout.razor` with four
  entries: Lines, Weekly Plans, Delivery Schedule, Alerts.
  `/lines/{id}/products` is reached from the Lines row action only.
- Home dashboard tile: "Scheduling: N drifted / M alerts", linking to
  `/scheduling/delivery`. Uses the `AnalyticsCacheService` pattern
  (5-min TTL).

## Testing

Real SQL Server (`ELITE / AdventureWorks2022_dev`) via
`WebApplicationFactory<Program>` per existing `IntegrationTest.cs`. No EF or
DB mocks. Hangfire and `RequestLogs` feature-flagged off for the suite, same
as today.

### Coverage by layer

| Layer | Test type | Files |
|---|---|---|
| `WeeklyPlanGenerator` | Integration | `Scheduling/Generator/WeeklyPlanGeneratorTests.cs` |
| `FrozenWindowEvaluator` | Integration | `Scheduling/Services/FrozenWindowEvaluatorTests.cs` |
| `SchedulingRuleResolver` | Unit | `Scheduling/Services/SchedulingRuleResolverTests.cs` |
| `IRecalcAction` impls | Integration | `Scheduling/Rules/SoftResortActionTests.cs`, `AlertOnlyActionTests.cs`, `HardReplanActionTests.cs` |
| Dispatcher end-to-end | Integration | `Scheduling/Dispatcher/InterceptorIntegrationTests.cs` |
| View | Integration | `Scheduling/Views/CurrentDeliveryScheduleViewTests.cs` |
| Minimal APIs | Integration | `Scheduling/Api/WeeklyPlanEndpointsTests.cs`, `ExceptionEndpointsTests.cs`, `AlertEndpointsTests.cs` |
| Page render smoke | Integration | `Scheduling/Pages/*PageTests.cs` |

### Must-have cases (acceptance floor)

**Generation:**
- Generates V1 when no prior plan exists.
- Future-week regenerate hard-deletes V1 and reinserts at V1 (count stays 1).
- Current-week regenerate bumps to V2; V1 rows preserved unchanged.
- Cancelled SOs excluded by default.
- Multi-factor sort verified on 4+ items with every tier differentiating.
- Overflow, `StrictCapacity=false` → `OverCapacity=true` on excess items.
- Overflow, `StrictCapacity=true` → `CapacityExceededException`, zero rows
  inserted (transactional).
- `LineProductAssignment` gates scope: unassigned models excluded.

**Dispatch:**
- New SO outside frozen window → `SoftResort` → one Info alert, no
  `BaselineDiverged` flip.
- New SO inside frozen window, `TotalDue ≥ 5000` → `AlertOnly` fires
  (Warning); `HardReplan` fallback does not fire (priority walk stops).
- New SO inside frozen window, `TotalDue < 5000` → `AlertOnly` self-skips;
  `HardReplan` fires (Critical + `BaselineDiverged=1`).
- Action throws → SO rolls back; zero `SalesOrderHeader` rows, zero alerts,
  zero plan updates. **Most critical correctness test.**
- Re-entrancy: dispatcher's own `SchedulingAlert` insert does not recursively
  invoke the interceptor.
- No `LineConfiguration` for the affected location → dispatcher
  short-circuits; SO still inserts cleanly.

**View:**
- Baseline-only: drift columns zero.
- Cancelled SO appears with `IsCancelled=1`, excluded from `CurrentSequence`
  renumbering.
- Active `ManualSequencePin` honored; other items renumber around it.
- `HotOrderBump` sorts item to top of its `DueDate` tier.
- Resolved exceptions ignored.

**API:**
- `generate` with `dryRun=true` returns preview, inserts nothing.
- All endpoints return 401/403 without cookie or API key.
- `POST /exceptions` reflected in view on subsequent `GET /delivery`.

### Test data plumbing

Follows `ToolSlotConfigurations` precedent: self-cleaning tests, schema
drift handled idempotently at fixture setup.

- New `SchedulingTestFixture` base class:
  - Wraps each test in a rolled-back `IDbContextTransaction` where possible.
  - Interceptor-exercising tests can't use implicit rollback; they clean up
    explicitly.
  - Helpers: `SeedLineAsync(locationId=60, takt=600, shifts=2, mps=480,
    lookahead=72)`, `SeedRulesAsync()`, `SeedWeeklyPlanAsync(weekId,
    locationId, itemCount)`.
- `OneTimeSetUp` runs idempotent `CREATE SCHEMA IF NOT EXISTS Scheduling;`
  and `CREATE OR ALTER VIEW Scheduling.vw_CurrentDeliverySchedule…`.
  Matches what `DatabaseInitializer` does in production — test startup is
  self-healing.

No scheduling seed is added to `DatabaseInitializer.SeedAsync`. Forecast
definitions follow the same "user-creates" convention; scheduling
configuration does too. Dev demo data is hand-seeded.

### Explicit non-coverage for slice 1

- Concurrency / load (double-click `Generate` is covered by the
  `(WeekId, LocationID, Version)` unique constraint; UX of the race isn't
  tested).
- Long-horizon drift simulations — dedicated to slice 2.
- Non-UTC time zones — slice 1 is UTC-only by design.

### Coverage target

Match current repo bar: every `IRecalcAction` tested, every public service
method has positive + negative coverage, every endpoint has an auth test. No
formal % target. The must-have list above is the floor.

## Dependencies on existing code

- `AddWithAuditAsync` / `DeleteWithAuditAsync` in
  `Infrastructure/Persistence/AuditedSaveExtensions.cs` — transaction
  boundary the dispatcher relies on.
- `AuditLogInterceptor` at `Infrastructure/Persistence/AuditLogInterceptor.cs`
  — runs alongside the new `SchedulingDispatchInterceptor`; ordering set in
  `ApplicationDbContext.OnConfiguring`.
- `IDbContextFactory<ApplicationDbContext>` pattern for Blazor components
  (scheduling pages follow this).
- `CrudPage` template from `Shared/UI/Components/` for 4 of 5 pages.
- `KpiCard` from `Shared/UI/Components/` for the delivery-schedule KPIs.
- `AnalyticsCacheService` pattern for the dashboard tile.
- `ApiOrCookie` authorization policy from `App/Extensions/ServiceRegistration.cs`.
- `DatabaseInitializer`: new migration markers must be added to
  `MigrationMarkers` per CLAUDE.md Section 4.

## Migration notes

- New migration creates the `Scheduling` schema, six tables, indexes, and
  the view. Add to `DatabaseInitializer.MigrationMarkers`.
- The view is created via `migrationBuilder.Sql("CREATE OR ALTER VIEW …")`
  in both `Up` and `Down` (drop in `Down`).
- `PatchMissingColumnsAsync` in `DatabaseInitializer` walks the design-time
  model; verify the view doesn't confuse it (views aren't in the EF model —
  safe).

## Open items intentionally left for the plan

These are mechanical enough that they belong in the implementation plan, not
the design:

- Exact migration naming.
- Concrete `SortWeights` numeric defaults for the tier weighting — the
  algorithm is locked; the constants are a tuning concern.
- Serilog event names / properties for dispatcher debug logs.
- Index names.

## Slice 2 preview (not for implementation now)

Captured here so reviewers can see the direction.

- `Features/MaterialSequencing` — station-level kit readiness, material pull
  against delivery schedule. Cross-references Scheduling via DTOs only; no
  shared tables.
- BOM cascade: Final Assembly demand → implied demand at Subassembly / Frame
  Welding via `Production.WorkOrderRouting`.
- Floor-execution capture table `Scheduling.ActualExecution`; completes the
  third drift leg (plan-vs-actual).
- Monday cron wiring around `IWeeklyPlanGenerator`.
- Multi-line `LineProductAssignment` editing UI polish.
- Rules-admin CrudPage for `SchedulingRule` (slice 1 ships raw-table only).
