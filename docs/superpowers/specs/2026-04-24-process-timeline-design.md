# Process Timeline — Slice 1 Design

**Date:** 2026-04-24
**Status:** Approved, ready for implementation planning
**Scope:** First visibility slice of cross-entity process chains — read-only timeline view driven by the existing `audit.AuditLog` + seeded `ProcessChainDefinition` rows.

## Problem

The platform runs a handful of inter-connected business flows whose steps are spread across different entities in different schemas: a sales order becomes a shipment which becomes shipment lines; a purchase order becomes a goods receipt; a work order becomes a production run. Today there is no way for a planner to point at one of those flows and see the end-to-end history — who did what, when, across every entity that participated.

This slice adds a read-only **process timeline** feature that stitches those entities together into a single time-ordered view. It uses the existing consolidated `audit.AuditLog` as its event source — no new audit plumbing, no new event-capture instrumentation.

## Goals

1. A standalone `/processes/timeline` page that, given a named chain + a root entity ID, renders a linear merged timeline of every AuditLog event for every entity in that chain.
2. Deep-link buttons on four existing entity detail pages (`SalesOrderHeader`, `PurchaseOrderHeader`, `Shipment`, `GoodsReceipt`) that navigate to the standalone page pre-filled.
3. A "Browse recent" mode: filterable list of recent chain instances grouped by root entity, driven by date range + chain + owner filters.
4. Two seeded chains: **Sales-to-Ship** and **Purchase-to-Receive** — the two named by the user as first priorities, both with clean existing FKs.

## Non-goals (deferred to later slices)

- Admin UI for editing `ProcessChainDefinition` rows — slice 2 concern.
- Sales-order → work-order chain — no direct FK exists; requires demand pegging, which is its own feature.
- Custom business events beyond what `audit.AuditLog` already captures — out of scope.
- Chain-instance caching / materialization — every render is a fresh query; we add caching when volume/SLA demands it.
- Inbox / queue / assignee model — slice B.
- SLA / stuck-at-step alerts — slice B.

## Architectural invariants

Four non-negotiables. The whole design protects these.

1. **No new audit plumbing.** Everything reads existing `audit.AuditLog`. If an event isn't captured there today, it's not in a slice-1 timeline.
2. **Chains are data.** `ProcessChainDefinition` is a table; walker *code* knows how to execute hops, but chain identity (name, steps, ownership semantics) lives in rows. Slice 2+ admin editor can mutate them without re-deploy.
3. **Timeline is derived.** No cached chain-instance table. Every render is a fresh FK walk + AuditLog query.
4. **Ownership is historical, not prescriptive.** Slice 1's "owner" = any user whose name appears in any `AuditLog.ChangedBy` for the chain's entities. Slice B introduces a separate "assigned owner" concept with its own table.

## Folder layout

```
Features/Processes/Timelines/
  Api/
    ProcessTimelineEndpoints.cs
  Application/
    IProcessChainResolver.cs          + ProcessChainResolver.cs
    IProcessTimelineComposer.cs       + ProcessTimelineComposer.cs
    IChainHopQuery.cs                 + 4 hop impls
    ChainInstance.cs
    ChainInstanceSummary.cs
    ChainQuery.cs
    ProcessTimeline.cs
    TimelineEvent.cs
  Domain/
    ProcessChainDefinition.cs
    ChainStep.cs
  Dtos/
    ChainDescriptorDto.cs
    TimelinePayloadDto.cs
    TimelineEventDto.cs
    ChainInstanceSummaryDto.cs
  UI/
    Pages/Index.razor
    Components/TimelineEventItem.razor
    Components/EntityTypeChip.razor
  ProcessTimelineServiceRegistration.cs
```

The `Features/Processes/Timelines/` namespace deliberately sits beside the existing `Features/ProcessManagement/` — keeps slice B's workflow engine work co-located with slice 1's timeline feature.

## Data model

### `processes.ProcessChainDefinition`

New table in a new `processes` SQL schema. Slice-2 admin UI and slice-B workflow engine will both live here.

| Column | Type | Notes |
|---|---|---|
| `ProcessChainDefinitionID` | `int IDENTITY` PK | |
| `Code` | `nvarchar(64)` | Stable slug (`"sales-to-ship"`). Unique index. URL-safe. |
| `Name` | `nvarchar(128)` | Display name (`"Sales to Ship"`). |
| `Description` | `nvarchar(500) NULL` | |
| `StepsJson` | `nvarchar(max)` | Ordered JSON array of hop descriptors. |
| `IsActive` | `bit` | Soft-disable without deleting. |
| `SortOrder` | `int` | Dropdown ordering. |
| `ModifiedDate` | `datetime2` | |

**`StepsJson` shape** (deserialized to `ChainStep[]`):

```json
[
  { "entity": "SalesOrderHeader", "role": "Root" },
  { "entity": "Shipment", "role": "Child",
    "parentEntity": "SalesOrderHeader", "foreignKey": "SalesOrderId" },
  { "entity": "ShipmentLine", "role": "Child",
    "parentEntity": "Shipment", "foreignKey": "ShipmentId" }
]
```

- `entity` — CLR short type name; matches `AuditLog.EntityType`.
- `role` — `Root` (exactly one per chain) or `Child`.
- `parentEntity` / `foreignKey` — how to join this step to its parent's collected IDs.

JSON-in-a-column (not a separate `ProcessChainStep` child table) because the shape is small, nobody queries inside it, and editors just round-trip JSON.

**Seed rows** (idempotent, inside `DatabaseInitializer.SeedReferenceDataAsync`):

- `"sales-to-ship"` / `"Sales to Ship"` / 3 steps as above.
- `"purchase-to-receive"` / `"Purchase to Receive"` / `PurchaseOrderHeader → GoodsReceipt (PurchaseOrderId) → GoodsReceiptLine (GoodsReceiptId)`.

No other tables — chain instances are derived, not stored.

### Existing tables (consumed, not modified)

- `audit.AuditLog` — sole event source. Columns already include `EntityType`, `EntityId` (stringified), `Action`, `ChangedBy`, `ChangedDate`, `ChangesJson`, `Summary`.
- `Sales.SalesOrderHeader`, `lgx.Shipment`, `lgx.ShipmentLine` — walked by the Sales-to-Ship hops.
- `Purchasing.PurchaseOrderHeader`, `lgx.GoodsReceipt`, `lgx.GoodsReceiptLine` — walked by the Purchase-to-Receive hops.

### Indexes to verify

Already on `audit.AuditLog` (required for performant queries; verify during plan execution, add in a small follow-up migration if missing):

- Composite `(EntityType, EntityId)` — used by the Phase-2 event query per `ChainInstance`.
- `(ChangedDate DESC)` — used by Mode 2's recent-activity ordering.

New on `processes.ProcessChainDefinition`: unique on `Code`.

## Chain walker & event composition

Two cooperating services, both singletons, both stateless.

### `IProcessChainResolver`

```csharp
public interface IProcessChainResolver
{
    Task<ChainInstance> ResolveAsync(string chainCode, string rootEntityId, CancellationToken ct);
    Task<IReadOnlyList<ChainInstanceSummary>> RecentAsync(ChainQuery query, CancellationToken ct);
}

public sealed record ChainInstance(
    ProcessChainDefinition Definition,
    string RootEntityId,
    IReadOnlyDictionary<string, IReadOnlyList<string>> CollectedIds);

public sealed record ChainInstanceSummary(
    string ChainCode,
    string RootEntityId,
    string? RootLabel,
    DateTime FirstEventAt,
    DateTime LastEventAt,
    int EventCount,
    IReadOnlyList<string> ContributorUsers);

public sealed record ChainQuery(
    string? ChainCode = null,
    string? Owner = null,
    DateTime? Since = null,
    DateTime? Until = null,
    int Limit = 100);
```

#### `ResolveAsync` — Phase-1 walk

1. Load `ProcessChainDefinition` by `Code`; deserialize `StepsJson`.
2. Walk steps in order. Step 0 is Root, seeded with the supplied `rootEntityId`. Each subsequent step executes a registered `IChainHopQuery` that accepts the parent step's collected IDs and returns child IDs.
3. Return `ChainInstance` with the id set per entity type. Total queries = number of steps (one of which is the root fetch, which may be a no-op for Root-role steps).

Unknown chain code → throws `ChainDefinitionNotFoundException`.

#### `RecentAsync` — Mode 2 payload

1. Query `audit.AuditLog` filtered by `ChangedDate` range + `EntityType` constrained to the types that appear in `ChainCode`'s steps (or all chain-referenced types if `ChainCode` is null).
2. Group the returned `(EntityType, EntityId)` pairs by their chain-rooted parent. For a child event, find its parent ID via the live entity's FK column (same FK metadata from `StepsJson`, reverse-walked using each `IChainHopQuery.GetParentIdAsync`). Repeat until `Role = Root`.
3. De-duplicate by `(chainCode, rootEntityId)`, project into `ChainInstanceSummary` with event counts + distinct user names.
4. Apply `Owner` filter post-aggregation: keep only summaries whose `ContributorUsers` contains `Owner`.
5. Cap at `query.Limit` (max 500; default 100).

### `IChainHopQuery` — one concrete impl per (parent, child, FK) triple

```csharp
public interface IChainHopQuery
{
    string ParentEntity { get; }
    string ChildEntity { get; }
    string ForeignKey { get; }

    Task<IReadOnlyList<string>> GetChildIdsAsync(
        ApplicationDbContext db, IReadOnlyList<string> parentIds, CancellationToken ct);

    Task<string?> GetParentIdAsync(
        ApplicationDbContext db, string childId, CancellationToken ct);
}
```

Slice-1 implementations (4 — one per seeded hop):

- `ShipmentFromSalesOrderHeader` (`Shipment.SalesOrderId` → `SalesOrderHeader.Id`)
- `ShipmentLineFromShipment` (`ShipmentLine.ShipmentId` → `Shipment.Id`)
- `GoodsReceiptFromPurchaseOrderHeader` (`GoodsReceipt.PurchaseOrderId` → `PurchaseOrderHeader.Id`)
- `GoodsReceiptLineFromGoodsReceipt` (`GoodsReceiptLine.GoodsReceiptId` → `GoodsReceipt.Id`)

Each is ~15 lines — two LINQ queries + three properties. Registered in DI.

`ProcessChainResolver` receives `IEnumerable<IChainHopQuery>` and looks up the right implementation by `(ParentEntity, ChildEntity, ForeignKey)` triple at resolve time. If a chain step references a hop with no registered query, the resolver throws `ChainStepNotSupportedException` on first resolve — lazy failure so partially-supported chains still respond.

**Design choice: explicit hop classes, not EF-model reflection.** Tempting to reflect on `DbContext.Model` and find every FK automatically. Rejected because (a) it would pick up FKs that aren't workflow-meaningful (e.g., `SalesOrderHeader.CustomerId`), and (b) the chain definition already names the FK it wants — being explicit matches that intent.

### `IProcessTimelineComposer`

```csharp
public interface IProcessTimelineComposer
{
    Task<ProcessTimeline> ComposeAsync(ChainInstance instance, CancellationToken ct);
}

public sealed record ProcessTimeline(
    ChainInstance Instance, IReadOnlyList<TimelineEvent> Events, bool Truncated);

public sealed record TimelineEvent(
    long AuditLogId, string EntityType, string EntityId,
    string Action, DateTime At, string? ChangedBy,
    string? Summary, string? ChangesJson);
```

Single Phase-2 query against `audit.AuditLog`:

```sql
SELECT TOP 501 AuditLogId, EntityType, EntityId, Action, ChangedBy,
       ChangedDate, Summary, ChangesJson
FROM audit.AuditLog
WHERE (EntityType = 'SalesOrderHeader' AND EntityId IN (...))
   OR (EntityType = 'Shipment'         AND EntityId IN (...))
   OR (EntityType = 'ShipmentLine'     AND EntityId IN (...))
ORDER BY ChangedDate ASC
```

501 rows fetched so the composer can detect the truncation cliff; if >500, `Truncated=true` and the 501st is discarded.

### Ownership semantics (slice 1)

- `ChainInstanceSummary.ContributorUsers` = distinct non-null `AuditLog.ChangedBy` across all events in the chain instance.
- Mode 2's `Owner` filter is a post-aggregation `WHERE` on that set.
- No `AssignedOwner` concept in slice 1 — slice B adds that as a separate field; the DTO shape stays backward-compatible.

### Error modes

- Unknown `chainCode` → `ChainDefinitionNotFoundException` → API 404.
- Root entity doesn't exist (no AuditLog row, no live row) → empty `ChainInstance`, empty `ProcessTimeline`, UI shows "No activity found."
- Hop query throws (DB error) → bubbles; API returns 500; UI shows retry button.
- Hard-deleted entity (AuditLog has `Deleted` row, live row gone) → Phase 1 walks nothing further, Phase 2 still pulls the entity's AuditLog events, UI badges them as "(deleted)."

## API

Three Minimal API endpoints under `/api/processes`. All gated by `"ApiOrCookie"`.

```
GET  /api/processes/chains
GET  /api/processes/chains/{chainCode}/timeline?rootEntityId=...
GET  /api/processes/chains/recent?chainCode=&owner=&since=&until=&limit=
```

### `GET /api/processes/chains` — dropdown data source

Returns active chain definitions (omits `StepsJson` — UI doesn't need it). Sorted by `SortOrder, Name`.

```json
[
  { "code": "sales-to-ship",       "name": "Sales to Ship",       "description": "…" },
  { "code": "purchase-to-receive", "name": "Purchase to Receive", "description": "…" }
]
```

### `GET /api/processes/chains/{chainCode}/timeline?rootEntityId=...` — Mode 1

Calls `ResolveAsync` → `ComposeAsync`. Returns:

```json
{
  "chain": { "code": "sales-to-ship", "name": "Sales to Ship" },
  "rootEntityId": "73581",
  "rootLabel": "SO #73581",
  "truncated": false,
  "events": [
    { "auditLogId": 1234567, "entityType": "SalesOrderHeader", "entityId": "73581",
      "action": "Created", "at": "2013-07-22T09:14:02Z", "changedBy": "sally@",
      "summary": "Customer 29825, Status InProcess" }
  ]
}
```

Four root labelers (slice 1), one per root entity type — produce `"SO #{Id}"`, `"PO #{Id}"`, `"Shipment #{Id}"`, `"Receipt #{Id}"`. Null if hard-deleted.

Status codes:
- `200 OK` with zero events if rootId has no AuditLog history (not an error).
- `404 Not Found` if `chainCode` unknown.
- `400 Bad Request` if `rootEntityId` missing/malformed.

### `GET /api/processes/chains/recent?...` — Mode 2

Calls `RecentAsync`. Returns array of `ChainInstanceSummary`.

Defaults: `chainCode` any; `owner` any; `since` = `now-30d`; `until` = `now`; `limit` = 100 (capped at 500).

### Umbrella mapper

`Features/Processes/Timelines/Api/ProcessTimelineEndpoints.cs` exposes `MapProcessTimelineEndpoints()`, called from `App/Routing/EndpointMappingExtensions.cs` alongside other batches.

### Auth

All three endpoints `.RequireAuthorization("ApiOrCookie")`. No role gating — read-only. Slice B's inbox will have role logic.

## UI

One new page + additions to four existing detail pages. All `InteractiveServer`.

### `/processes/timeline` — standalone page

Single `Index.razor` with two modes in `MudTabs`:

**Tab 1 — Lookup (Mode 1):**
- `MudSelect` bound to `GET /api/processes/chains`
- `MudTextField` for root entity ID
- "Load timeline" button → `GET /api/processes/chains/{code}/timeline?rootEntityId=...`
- `MudTimeline` renders the payload, one `MudTimelineItem` per event

Each timeline item:
- Timestamp (`yyyy-MM-dd HH:mm`, converted to local time in MudBlazor)
- Entity chip via `EntityTypeChip` component (color-coded — SO blue, Shipment orange, etc.)
- Action chip (Created green, Updated amber, Deleted red)
- Changed-by avatar + name
- Summary
- Expandable `ChangesJson` viewer on click

If `Truncated=true`, a `MudAlert` at top: "Showing most recent 500 events. Older activity trimmed."

**Tab 2 — Browse (Mode 2):**
- Filter bar: chain dropdown, owner text, date range picker (default last 30d), limit spinner
- `MudDataGrid` of `ChainInstanceSummary` rows
- Columns: Chain, Root, First event, Last event, Event count, Contributors (comma-joined)
- Row click → Lookup tab with chain+root pre-filled (via URL params)

### URL parameters

`/processes/timeline?chain=sales-to-ship&root=73581` deep-links to Tab 1 with form pre-filled + auto-load.

### Deep-link buttons on entity detail pages

| Page route | Chain | Root source |
|---|---|---|
| `/sales/orders/{id}` | `sales-to-ship` | route `{id}` |
| `/purchasing/purchase-orders/{id}` | `purchase-to-receive` | route `{id}` |
| `/logistics/shipments/{id}` | `sales-to-ship` | `SalesOrderId` from loaded entity (button hidden if null) |
| `/logistics/receipts/{id}` | `purchase-to-receive` | `PurchaseOrderId` from loaded entity |

Each addition is ~5 lines:

```razor
<MudTooltip Text="Open process timeline">
    <MudButton StartIcon="@Icons.Material.Filled.Timeline"
               Href="@($"/processes/timeline?chain={chainCode}&root={rootId}")"
               Variant="Variant.Outlined"
               Size="Size.Small">
        Timeline
    </MudButton>
</MudTooltip>
```

### Nav

One new `MudNavLink` under the existing Insights section in `Shared/UI/Layout/NavMenu.razor`, placed next to the existing `/processes` link:

```razor
<MudNavLink Href="processes/timeline"
            Icon="@Icons.Material.Filled.Timeline">
    Process timelines
</MudNavLink>
```

## Testing

Real SQL Server, `WebApplicationFactory<Program>`, inherit from `IntegrationTestFixtureBase`. Same pattern as scheduling tests.

### Coverage by layer

| Layer | Test type | File |
|---|---|---|
| `ProcessChainResolver.ResolveAsync` | Integration | `Processes/Timelines/ResolverTests.cs` |
| `ProcessChainResolver.RecentAsync` | Integration | same |
| Each `IChainHopQuery` (4 impls) | Integration | `Processes/Timelines/HopQueryTests.cs` |
| `ProcessTimelineComposer` | Integration | `Processes/Timelines/ComposerTests.cs` |
| API endpoints | `HttpClient` integration | `Processes/Timelines/Api/EndpointTests.cs` |
| `ProcessChainDefinition` seed | Integration | `Processes/Timelines/Seed/ChainDefinitionSeedTests.cs` |
| Page render smoke | Integration | `Processes/Timelines/Pages/IndexPageTests.cs` |

### Must-have cases

**Resolver — `ResolveAsync`:**
- Known root with downstream data → returns dictionary with Root + every step populated.
- Root with no downstream → Root set has `{rootId}`, child sets empty.
- Unknown chainCode → throws `ChainDefinitionNotFoundException`.
- Deleted root → Root set empty, downstream empty, no exception.

**Resolver — `RecentAsync`:**
- No filters → returns up-to-limit summaries ordered by `LastEventAt DESC`.
- `chainCode` filter applied correctly.
- `owner` filter applied correctly.
- Date range applied correctly.
- Reverse-walk-up-to-root works correctly for ShipmentLine-rooted events.

**Hop queries** (one parametric test per impl):
- Happy path: known parent IDs → expected child IDs.
- Empty parent list → empty result.
- Reverse `GetParentIdAsync` returns parent for known child, null for unknown.

**Composer:**
- Chain with N events → all N, sorted ASC by `ChangedDate`.
- Chain with >500 events → returns 500, `Truncated=true`.
- Chain with no AuditLog history → empty events, `Truncated=false`.

**Endpoints:**
- `/api/processes/chains` returns active chains, hides inactive.
- `/api/processes/chains/{code}/timeline` — 200 with events for a seeded test entity, 404 for unknown code, 400 for missing `rootEntityId`.
- `/api/processes/chains/recent` — applies defaults, honors `limit` cap of 500.
- All three — 401/403 without auth.

**Seed:**
- After startup, `ProcessChainDefinition` has rows for `sales-to-ship` and `purchase-to-receive`; `StepsJson` parses to the expected `ChainStep[]`.

**Page smoke:**
- `GET /processes/timeline` → 200 for authenticated user.
- `GET /processes/timeline?chain=sales-to-ship&root=43659` → 200 with expected page markers.

### Test data strategy

**For resolver/composer/hop tests** — use real AdventureWorks data (SO `43659`, PO `1` — both first-insertion rows, stable since 2011). AuditLog will be sparse for historical rows; we test the *shape* of the result, not event volume.

**For AuditLog-behavior tests** (truncation, filtering, ordering) — insert test AuditLog rows keyed by sentinel `EntityType = "__ProcessTimelineTest"`. Teardown wipes by prefix.

**For endpoint tests** — mix: real AW entities for 200-OK cases; throwaway AuditLog rows for edge cases.

### Fixture helpers

New `ProcessTimelineTestFixture : IntegrationTestFixtureBase` with:
- `SeedAuditLogEventsAsync(entityType, entityId, count, baseTime)`
- `CleanupTestAuditLogsAsync()` — `WHERE EntityType LIKE '__%Test'`
- `KnownAwSalesOrderId = 43659`
- `KnownAwPurchaseOrderId = 1`

Per-test `[SetUp]/[TearDown]` pattern, same as scheduling.

### Out of coverage (slice 1)

- Stress/perf with 100k+ AuditLog rows — deferred until cap insufficiency observed.
- Concurrency — read-only feature, no consistency issues.
- Time-zone correctness — slice 1 UTC-only; display conversion is MudBlazor's concern.
- Slice-B features — don't exist yet.

### Acceptance floor

Every hop has positive + empty coverage. Every endpoint has positive + auth + bad-input coverage. End-to-end path (resolver → composer → API → page) works on a real AW SO. Seed verified. Zero new warnings in `dotnet build`.

## Dependencies on existing code

- `audit.AuditLog` (schema + `AuditingInterceptor` writer, `Shared/Audit/AuditLog.cs`).
- `IDbContextFactory<ApplicationDbContext>` pattern — all services use it.
- `ApiOrCookie` authorization policy (`App/Extensions/ServiceRegistration.cs`).
- `DatabaseInitializer.SeedReferenceDataAsync` — new seed calls added here.
- `DatabaseInitializer.MigrationMarkers` — new migration marker added per CLAUDE.md.
- `App/Routing/EndpointMappingExtensions.cs` — new `MapProcessTimelineEndpoints()` call added.
- `Shared/UI/Layout/NavMenu.razor` — new nav link added.
- Existing entity pages for the four deep-link additions (see UI §).

## Migration notes

- New migration creates the `processes` schema + `ProcessChainDefinition` table + its unique index on `Code`. Add to `DatabaseInitializer.MigrationMarkers` with `LineConfiguration`-pattern entry; marker table is `ProcessChainDefinition`.
- Verify `audit.AuditLog` has composite `(EntityType, EntityId)` and `(ChangedDate)` indexes. If either is missing, add in a second small migration. Non-breaking.

## Open items intentionally left for the plan

These are implementation-detail enough that they belong in the plan, not the design:

- Exact migration naming.
- Concrete four-per-root labeler content (format strings).
- Whether `ChainStep` is a `record class` or a `record struct`.
- Serilog log-level choices inside services.

## Slice-B preview (not for implementation now)

Captured here so reviewers can see the direction:

- Extend `Features/ProcessManagement` — `Process` rows link to a `ProcessChainDefinition` code and define step-level state machines that tie to entity state transitions.
- New `ProcessInstance` and `ProcessStepInstance` tables representing an in-flight chain as a materialized state tracker, distinct from slice 1's derived view.
- `AssignedOwner` on `ProcessInstance` + routing rules for assignment.
- `/processes/inbox` — "my work": list of `ProcessInstance` rows assigned to the current user, grouped by status.
- SLA tracking — step deadlines, stuck-at-step alerts, overdue badges.
- Admin UI to edit `ProcessChainDefinition` + define state machines.

The slice-1 API surface (`ChainInstanceSummary`, `TimelineEvent`) is designed to remain source-compatible when slice B layers on; new fields append rather than replace.
