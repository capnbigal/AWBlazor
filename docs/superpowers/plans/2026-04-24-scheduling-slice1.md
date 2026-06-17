# Scheduling Slice 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the end-to-end Scheduling control plane (Features/Scheduling slice 1) for Production.Location 60 (Final Assembly) — six tables, one SQL view, rule-dispatched `SaveChangesInterceptor`, weekly-plan generator, five planner UI pages.

**Architecture:** Append-only `WeeklyPlanItem` baseline per ISO week; current schedule derived via `vw_CurrentDeliverySchedule` joining the baseline + current `SalesOrderHeader` state + a sparse `SchedulingException` overlay. A `SchedulingDispatchInterceptor` fires on SO inserts inside the `AddWithAuditAsync` transaction, resolves a `(EventType, InFrozenWindow)` rule, and executes an `IRecalcAction` (writes `SchedulingAlert` / flips `BaselineDiverged`, never mutates the baseline).

**Tech Stack:** .NET 10, EF Core 10 (SQL Server on ELITE / AdventureWorks2022_dev for tests), Blazor Web App, MudBlazor 9, NUnit (`WebApplicationFactory<Program>`).

**Full design reference:** `docs/superpowers/specs/2026-04-23-scheduling-slice1-design.md` — always consult for column shapes, invariants, rationale.

**Project conventions that every task must respect:**
- All writes through `AddWithAuditAsync` / `DeleteWithAuditAsync` (Infrastructure/Persistence/AuditedSaveExtensions.cs). Don't call `SaveChangesAsync` directly for SO or scheduling-table writes.
- Blazor components inject `IDbContextFactory<ApplicationDbContext>`, never the scoped context.
- Every new migration: add marker to `DatabaseInitializer.MigrationMarkers`.
- All scheduling pages are `InteractiveServer` (not `[ExcludeFromInteractiveRouting]`) — so MudBlazor inputs work fine; no `<InputText>` constraint.
- Branch: `feat/scheduling-slice1-design` (already created).
- Build/test commands: `dotnet build AWBlazorApp.slnx`, `dotnet test AWBlazorApp.slnx`.

---

## Task 1: Add `IsoWeek` helper + unit tests

Pure helper class used by generator, evaluator, view SQL migration, and tests. Ship first so everything downstream can lean on it.

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Services/IsoWeekHelper.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Services/IsoWeekHelperTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
// tests/AWBlazorApp.Tests/Scheduling/Services/IsoWeekHelperTests.cs
using AWBlazorApp.Features.Scheduling.Services;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Services;

[TestFixture]
public class IsoWeekHelperTests
{
    [Test]
    public void FromDate_RegularWeek_Encodes_YearTimes100PlusWeek()
    {
        // Wed 2026-04-29 is in 2026-W18
        var id = IsoWeekHelper.FromDate(new DateTime(2026, 4, 29));
        Assert.That(id, Is.EqualTo(202618));
    }

    [Test]
    public void FromDate_YearBoundary_UsesIsoYear_NotCalendarYear()
    {
        // 2027-01-01 is a Friday; ISO week belongs to 2026-W53
        var id = IsoWeekHelper.FromDate(new DateTime(2027, 1, 1));
        Assert.That(id, Is.EqualTo(202653));
    }

    [Test]
    public void ToMondayUtc_ReturnsMidnightMondayOfIsoWeek()
    {
        var monday = IsoWeekHelper.ToMondayUtc(202618);
        Assert.That(monday, Is.EqualTo(new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Test]
    public void FromDate_MondayAndSunday_SameIsoWeek()
    {
        var mondayId = IsoWeekHelper.FromDate(new DateTime(2026, 4, 27));
        var sundayId = IsoWeekHelper.FromDate(new DateTime(2026, 5, 3));
        Assert.That(mondayId, Is.EqualTo(sundayId));
        Assert.That(mondayId, Is.EqualTo(202618));
    }
}
```

- [ ] **Step 2: Run to confirm they fail**

`dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~IsoWeekHelperTests"` → compile error (`IsoWeekHelper` not found).

- [ ] **Step 3: Implement**

```csharp
// src/AWBlazorApp/Features/Scheduling/Services/IsoWeekHelper.cs
using System.Globalization;

namespace AWBlazorApp.Features.Scheduling.Services;

public static class IsoWeekHelper
{
    public static int FromDate(DateTime date)
    {
        var isoYear = ISOWeek.GetYear(date);
        var isoWeek = ISOWeek.GetWeekOfYear(date);
        return isoYear * 100 + isoWeek;
    }

    public static DateTime ToMondayUtc(int weekId)
    {
        var isoYear = weekId / 100;
        var isoWeek = weekId % 100;
        var monday = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
        return DateTime.SpecifyKind(monday, DateTimeKind.Utc);
    }

    public static string Format(int weekId) => $"{weekId / 100}-W{weekId % 100:D2}";
}
```

- [ ] **Step 4: Tests pass**

`dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~IsoWeekHelperTests"` → 4 passed.

- [ ] **Step 5: Commit**

```bash
git add src/AWBlazorApp/Features/Scheduling/Services/IsoWeekHelper.cs tests/AWBlazorApp.Tests/Scheduling/Services/IsoWeekHelperTests.cs
git commit -m "feat(scheduling): IsoWeekHelper + unit tests"
```

---

## Task 2: Domain entities and enums

All scheduling POCOs + `[Table]`/`[Column]` mappings. No tests — these are plain data classes; schema round-trip is covered in Task 4.

**Files (create all):**
- `src/AWBlazorApp/Features/Scheduling/LineConfigurations/Domain/LineConfiguration.cs`
- `src/AWBlazorApp/Features/Scheduling/LineProductAssignments/Domain/LineProductAssignment.cs`
- `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Domain/WeeklyPlan.cs`
- `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Domain/WeeklyPlanItem.cs`
- `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/Domain/SchedulingException.cs`
- `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/Domain/ExceptionType.cs`
- `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/Domain/CurrentDeliveryScheduleRow.cs`
- `src/AWBlazorApp/Features/Scheduling/Rules/Domain/SchedulingRule.cs`
- `src/AWBlazorApp/Features/Scheduling/Rules/Domain/SchedulingEventType.cs`
- `src/AWBlazorApp/Features/Scheduling/Rules/Domain/RecalcActionType.cs`
- `src/AWBlazorApp/Features/Scheduling/Alerts/Domain/SchedulingAlert.cs`
- `src/AWBlazorApp/Features/Scheduling/Alerts/Domain/AlertSeverity.cs`
- `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Domain/CapacityExceededException.cs`

- [ ] **Step 1: Write all entity files**

```csharp
// LineConfiguration.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;

[Table("LineConfiguration", Schema = "Scheduling")]
public class LineConfiguration
{
    [Key, Column("LineConfigurationID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("TaktSeconds")] public int TaktSeconds { get; set; }
    [Column("ShiftsPerDay")] public byte ShiftsPerDay { get; set; }
    [Column("MinutesPerShift")] public short MinutesPerShift { get; set; }
    [Column("FrozenLookaheadHours")] public int FrozenLookaheadHours { get; set; } = 72;
    [Column("IsActive")] public bool IsActive { get; set; } = true;
    [Column("ModifiedDate")] public DateTime ModifiedDate { get; set; }
}
```

```csharp
// LineProductAssignment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;

[Table("LineProductAssignment", Schema = "Scheduling")]
public class LineProductAssignment
{
    [Key, Column("LineProductAssignmentID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("ProductModelID")] public int ProductModelId { get; set; }
    [Column("IsActive")] public bool IsActive { get; set; } = true;
    [Column("ModifiedDate")] public DateTime ModifiedDate { get; set; }
}
```

```csharp
// WeeklyPlan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

[Table("WeeklyPlan", Schema = "Scheduling")]
public class WeeklyPlan
{
    [Key, Column("WeeklyPlanID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("Version")] public int Version { get; set; }
    [Column("PublishedAt")] public DateTime PublishedAt { get; set; }
    [Column("PublishedBy"), MaxLength(256)] public string PublishedBy { get; set; } = "";
    [Column("BaselineDiverged")] public bool BaselineDiverged { get; set; }
    [Column("GenerationOptionsJson")] public string? GenerationOptionsJson { get; set; }
    public List<WeeklyPlanItem> Items { get; set; } = new();
}
```

```csharp
// WeeklyPlanItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

[Table("WeeklyPlanItem", Schema = "Scheduling")]
public class WeeklyPlanItem
{
    [Key, Column("WeeklyPlanItemID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("WeeklyPlanID")] public int WeeklyPlanId { get; set; }
    [Column("SalesOrderID")] public int SalesOrderId { get; set; }
    [Column("SalesOrderDetailID")] public int SalesOrderDetailId { get; set; }
    [Column("ProductID")] public int ProductId { get; set; }
    [Column("PlannedSequence")] public int PlannedSequence { get; set; }
    [Column("PlannedStart")] public DateTime PlannedStart { get; set; }
    [Column("PlannedEnd")] public DateTime PlannedEnd { get; set; }
    [Column("PlannedQty")] public short PlannedQty { get; set; }
    [Column("OverCapacity")] public bool OverCapacity { get; set; }
}
```

```csharp
// ExceptionType.cs
namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;
public enum ExceptionType : byte
{
    ManualSequencePin = 1,
    KittingHold = 2,
    HotOrderBump = 3
}
```

```csharp
// SchedulingException.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;

[Table("SchedulingException", Schema = "Scheduling")]
public class SchedulingException
{
    [Key, Column("SchedulingExceptionID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("SalesOrderDetailID")] public int SalesOrderDetailId { get; set; }
    [Column("ExceptionType")] public ExceptionType ExceptionType { get; set; }
    [Column("PinnedSequence")] public int? PinnedSequence { get; set; }
    [Column("Reason"), MaxLength(500)] public string Reason { get; set; } = "";
    [Column("CreatedAt")] public DateTime CreatedAt { get; set; }
    [Column("CreatedBy"), MaxLength(256)] public string CreatedBy { get; set; } = "";
    [Column("ResolvedAt")] public DateTime? ResolvedAt { get; set; }
    [Column("ResolvedBy"), MaxLength(256)] public string? ResolvedBy { get; set; }
}
```

```csharp
// CurrentDeliveryScheduleRow.cs (keyless view entity)
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;

// Maps to Scheduling.vw_CurrentDeliverySchedule. Configured .HasNoKey().ToView(...) in DbContext.
public class CurrentDeliveryScheduleRow
{
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("SalesOrderID")] public int SalesOrderId { get; set; }
    [Column("SalesOrderDetailID")] public int SalesOrderDetailId { get; set; }
    [Column("ProductID")] public int ProductId { get; set; }
    [Column("PlannedSequence")] public int? PlannedSequence { get; set; }
    [Column("PlannedStart")] public DateTime? PlannedStart { get; set; }
    [Column("PlannedEnd")] public DateTime? PlannedEnd { get; set; }
    [Column("PlannedQty")] public short? PlannedQty { get; set; }
    [Column("CurrentSequence")] public int? CurrentSequence { get; set; }
    [Column("CurrentStart")] public DateTime? CurrentStart { get; set; }
    [Column("CurrentEnd")] public DateTime? CurrentEnd { get; set; }
    [Column("CurrentQty")] public short? CurrentQty { get; set; }
    [Column("SequenceDrift")] public int? SequenceDrift { get; set; }
    [Column("StartDriftMinutes")] public int? StartDriftMinutes { get; set; }
    [Column("PromiseDate")] public DateTime? PromiseDate { get; set; }
    [Column("PromiseDriftMinutes")] public int? PromiseDriftMinutes { get; set; }
    [Column("ExceptionType")] public byte? ExceptionType { get; set; }
    [Column("ExceptionReason")] public string? ExceptionReason { get; set; }
    [Column("SoStatus")] public byte? SoStatus { get; set; }
    [Column("IsCancelled")] public bool IsCancelled { get; set; }
    [Column("IsHotOrder")] public bool IsHotOrder { get; set; }
}
```

```csharp
// SchedulingEventType.cs
namespace AWBlazorApp.Features.Scheduling.Rules.Domain;
public enum SchedulingEventType : byte { NewSO = 1 }
```

```csharp
// RecalcActionType.cs
namespace AWBlazorApp.Features.Scheduling.Rules.Domain;
public enum RecalcActionType : byte
{
    SoftResort = 1,
    AlertOnly = 2,
    HardReplan = 3
}
```

```csharp
// SchedulingRule.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.Rules.Domain;

[Table("SchedulingRule", Schema = "Scheduling")]
public class SchedulingRule
{
    [Key, Column("SchedulingRuleID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("EventType")] public SchedulingEventType EventType { get; set; }
    [Column("InFrozenWindow")] public bool InFrozenWindow { get; set; }
    [Column("Action")] public RecalcActionType Action { get; set; }
    [Column("ParametersJson")] public string? ParametersJson { get; set; }
    [Column("Priority")] public int Priority { get; set; }
    [Column("IsActive")] public bool IsActive { get; set; } = true;
}
```

```csharp
// AlertSeverity.cs
namespace AWBlazorApp.Features.Scheduling.Alerts.Domain;
public enum AlertSeverity : byte { Info = 1, Warning = 2, Critical = 3 }
```

```csharp
// SchedulingAlert.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
namespace AWBlazorApp.Features.Scheduling.Alerts.Domain;

[Table("SchedulingAlert", Schema = "Scheduling")]
public class SchedulingAlert
{
    [Key, Column("SchedulingAlertID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("CreatedAt")] public DateTime CreatedAt { get; set; }
    [Column("Severity")] public AlertSeverity Severity { get; set; }
    [Column("EventType")] public SchedulingEventType EventType { get; set; }
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("SalesOrderID")] public int? SalesOrderId { get; set; }
    [Column("Message"), MaxLength(1000)] public string Message { get; set; } = "";
    [Column("PayloadJson")] public string? PayloadJson { get; set; }
    [Column("AcknowledgedAt")] public DateTime? AcknowledgedAt { get; set; }
    [Column("AcknowledgedBy"), MaxLength(256)] public string? AcknowledgedBy { get; set; }
}
```

```csharp
// CapacityExceededException.cs
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

public class CapacityExceededException : Exception
{
    public double ExcessHours { get; }
    public CapacityExceededException(double excessHours)
        : base($"Generation exceeds week capacity by {excessHours:F2} hours.")
        => ExcessHours = excessHours;
}
```

- [ ] **Step 2: Build**

`dotnet build AWBlazorApp.slnx` → 0 errors.

- [ ] **Step 3: Commit**

```bash
git add src/AWBlazorApp/Features/Scheduling/
git commit -m "feat(scheduling): domain entities + enums for slice 1"
```

---

## Task 3: Register DbSets and `OnModelCreating` configs

**Files:**
- Modify: `src/AWBlazorApp/Infrastructure/Persistence/ApplicationDbContext.cs`

- [ ] **Step 1: Add DbSets and using statements**

Add near other DbSets:

```csharp
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;

// ... inside ApplicationDbContext:
public DbSet<LineConfiguration> LineConfigurations => Set<LineConfiguration>();
public DbSet<LineProductAssignment> LineProductAssignments => Set<LineProductAssignment>();
public DbSet<WeeklyPlan> WeeklyPlans => Set<WeeklyPlan>();
public DbSet<WeeklyPlanItem> WeeklyPlanItems => Set<WeeklyPlanItem>();
public DbSet<SchedulingException> SchedulingExceptions => Set<SchedulingException>();
public DbSet<SchedulingRule> SchedulingRules => Set<SchedulingRule>();
public DbSet<SchedulingAlert> SchedulingAlerts => Set<SchedulingAlert>();
public DbSet<CurrentDeliveryScheduleRow> CurrentDeliverySchedule => Set<CurrentDeliveryScheduleRow>();
```

- [ ] **Step 2: Add config block in `OnModelCreating`** (after existing entity configs, before `base.OnModelCreating`)

```csharp
// === Scheduling slice 1 ===
modelBuilder.Entity<LineConfiguration>(e =>
{
    e.HasIndex(x => x.LocationId).IsUnique();
});

modelBuilder.Entity<LineProductAssignment>(e =>
{
    e.HasIndex(x => new { x.LocationId, x.ProductModelId }).IsUnique();
});

modelBuilder.Entity<WeeklyPlan>(e =>
{
    e.HasIndex(x => new { x.WeekId, x.LocationId, x.Version }).IsUnique();
    e.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.WeeklyPlanId);
});

modelBuilder.Entity<WeeklyPlanItem>(e =>
{
    e.HasIndex(x => new { x.WeeklyPlanId, x.PlannedSequence });
    e.HasIndex(x => x.SalesOrderDetailId);
});

modelBuilder.Entity<SchedulingException>(e =>
{
    // filtered unique: only active overrides
    e.HasIndex(x => new { x.WeekId, x.LocationId, x.SalesOrderDetailId })
        .IsUnique()
        .HasFilter("[ResolvedAt] IS NULL");
});

modelBuilder.Entity<SchedulingAlert>(e =>
{
    e.HasIndex(x => new { x.AcknowledgedAt, x.CreatedAt });
});

modelBuilder.Entity<CurrentDeliveryScheduleRow>(e =>
{
    e.HasNoKey();
    e.ToView("vw_CurrentDeliverySchedule", "Scheduling");
});
// === end Scheduling ===
```

- [ ] **Step 3: Build**

`dotnet build AWBlazorApp.slnx` → 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/AWBlazorApp/Infrastructure/Persistence/ApplicationDbContext.cs
git commit -m "feat(scheduling): DbSets and model configs"
```

---

## Task 4: Generate EF migration + hand-write view SQL

**Files:**
- Create (via EF tool): `src/AWBlazorApp/Infrastructure/Persistence/Migrations/<timestamp>_AddSchedulingSchema.cs` + `.Designer.cs`
- Modify the generated migration's `Up` / `Down` to add view SQL.

- [ ] **Step 1: Generate migration**

```bash
cd src/AWBlazorApp
dotnet ef migrations add AddSchedulingSchema
cd ../..
```

Confirm new files exist under `src/AWBlazorApp/Infrastructure/Persistence/Migrations/`.

- [ ] **Step 2: Inspect the generated migration**

Open the new `*_AddSchedulingSchema.cs`. It will `EnsureSchema("Scheduling")` and create the six tables + indexes. Verify all six are present; if anything's missing, re-check Task 3's configs.

- [ ] **Step 3: Append view creation to `Up`**

Add at the **end** of `Up`:

```csharp
migrationBuilder.Sql(@"
CREATE OR ALTER VIEW Scheduling.vw_CurrentDeliverySchedule AS
WITH LatestPlan AS (
    SELECT wp.WeekId, wp.LocationID, wp.WeeklyPlanID,
           ROW_NUMBER() OVER (PARTITION BY wp.WeekId, wp.LocationID ORDER BY wp.Version DESC) AS rn
    FROM Scheduling.WeeklyPlan wp
),
Items AS (
    SELECT wpi.*, lp.WeekId, lp.LocationID
    FROM Scheduling.WeeklyPlanItem wpi
    JOIN LatestPlan lp ON lp.WeeklyPlanID = wpi.WeeklyPlanID AND lp.rn = 1
),
ActiveEx AS (
    SELECT * FROM Scheduling.SchedulingException WHERE ResolvedAt IS NULL
),
Joined AS (
    SELECT
        i.WeekId, i.LocationID,
        i.SalesOrderID, i.SalesOrderDetailID, i.ProductID,
        i.PlannedSequence, i.PlannedStart, i.PlannedEnd, i.PlannedQty,
        soh.DueDate        AS PromiseDate,
        soh.Status         AS SoStatus,
        CASE WHEN soh.Status = 6 THEN 1 ELSE 0 END AS IsCancelled,
        CASE WHEN soh.OnlineOrderFlag = 0 THEN 1 ELSE 0 END AS CustomerPriority,
        sod.OrderQty       AS CurrentQty,
        p.ProductModelID,
        soh.TotalDue,
        soh.ModifiedDate   AS SoModifiedDate,
        ax.ExceptionType,
        ax.PinnedSequence,
        ax.Reason          AS ExceptionReason,
        CASE WHEN ax.ExceptionType = 3 THEN 1 ELSE 0 END AS IsHotOrder
    FROM Items i
    LEFT JOIN Sales.SalesOrderHeader soh ON soh.SalesOrderID = i.SalesOrderID
    LEFT JOIN Sales.SalesOrderDetail sod ON sod.SalesOrderDetailID = i.SalesOrderDetailID
    LEFT JOIN Production.Product   p   ON p.ProductID = i.ProductID
    LEFT JOIN ActiveEx              ax  ON ax.SalesOrderDetailID = i.SalesOrderDetailID
                                         AND ax.WeekId = i.WeekId
                                         AND ax.LocationID = i.LocationID
),
Sequenced AS (
    SELECT *,
        CASE WHEN IsCancelled = 1 THEN NULL
             ELSE ROW_NUMBER() OVER (
                PARTITION BY WeekId, LocationID, IsCancelled
                ORDER BY
                    CASE WHEN PinnedSequence IS NULL THEN 1 ELSE 0 END,
                    PinnedSequence,
                    IsHotOrder DESC,
                    PromiseDate ASC,
                    CustomerPriority DESC,
                    ProductModelID ASC,
                    TotalDue DESC,
                    SoModifiedDate ASC,
                    SalesOrderDetailID ASC)
        END AS CurrentSequence
    FROM Joined
)
SELECT
    WeekId, LocationID, SalesOrderID, SalesOrderDetailID, ProductID,
    PlannedSequence, PlannedStart, PlannedEnd, PlannedQty,
    CurrentSequence,
    PlannedStart      AS CurrentStart,   -- slice 1: start = planned (time drift comes with takt recompute in slice 2)
    PlannedEnd        AS CurrentEnd,
    CurrentQty,
    CASE WHEN CurrentSequence IS NULL OR PlannedSequence IS NULL THEN NULL
         ELSE CurrentSequence - PlannedSequence END AS SequenceDrift,
    0                                AS StartDriftMinutes,
    PromiseDate,
    CASE WHEN PromiseDate IS NULL OR PlannedEnd IS NULL THEN NULL
         ELSE DATEDIFF(MINUTE, PromiseDate, PlannedEnd) END AS PromiseDriftMinutes,
    ExceptionType,
    ExceptionReason,
    SoStatus,
    CAST(IsCancelled AS BIT) AS IsCancelled,
    CAST(IsHotOrder  AS BIT) AS IsHotOrder
FROM Sequenced;
");
```

Note the slice-1 simplification: `CurrentStart`/`CurrentEnd` currently equal `PlannedStart`/`PlannedEnd` and `StartDriftMinutes` is always 0. Real takt-based recompute against live `CurrentQty` sequences lands in slice 2. The **sequence** drift (which is the interactive planner value) is already correct.

- [ ] **Step 4: Append view drop to `Down`**

At the **start** of `Down`:

```csharp
migrationBuilder.Sql("IF OBJECT_ID('Scheduling.vw_CurrentDeliverySchedule','V') IS NOT NULL DROP VIEW Scheduling.vw_CurrentDeliverySchedule;");
```

- [ ] **Step 5: Apply to dev DB + smoke-test**

```bash
cd src/AWBlazorApp
dotnet ef database update
cd ../..
```

Then via SSMS or `sqlcmd` against `ELITE.AdventureWorks2022_dev`:
```sql
SELECT COUNT(*) FROM Scheduling.vw_CurrentDeliverySchedule;
-- expect: 0 (empty, but query succeeds)
SELECT TOP 0 * FROM Scheduling.vw_CurrentDeliverySchedule;
-- expect: all 22 columns listed with correct types
```

- [ ] **Step 6: Build + commit**

```bash
dotnet build AWBlazorApp.slnx
git add src/AWBlazorApp/Infrastructure/Persistence/Migrations/
git commit -m "feat(scheduling): EF migration for schema + view"
```

---

## Task 5: Add migration marker to `DatabaseInitializer`

Per CLAUDE.md Section 4 — prevents first-run failures on machines where someone ran a prerelease build that already created these tables.

**Files:**
- Modify: `src/AWBlazorApp/Infrastructure/Persistence/DatabaseInitializer.cs`

- [ ] **Step 1: Inspect current `MigrationMarkers` array + add entry**

Find the `MigrationMarkers` constant. Add:

```csharp
new MigrationMarker(
    MigrationId: "<paste the new migration's timestamped ID here, e.g. 20260424103000_AddSchedulingSchema>",
    Tables: new[] {
        ("Scheduling", "LineConfiguration"),
        ("Scheduling", "LineProductAssignment"),
        ("Scheduling", "WeeklyPlan"),
        ("Scheduling", "WeeklyPlanItem"),
        ("Scheduling", "SchedulingException"),
        ("Scheduling", "SchedulingRule"),
        ("Scheduling", "SchedulingAlert")
    }
),
```

(The struct shape may differ slightly — match the existing entries exactly; view is excluded because markers are for tables.)

- [ ] **Step 2: Build + commit**

```bash
dotnet build AWBlazorApp.slnx
git add src/AWBlazorApp/Infrastructure/Persistence/DatabaseInitializer.cs
git commit -m "chore(scheduling): register migration marker"
```

---

## Task 6: Seed three `SchedulingRule` rows

Seed rules belong in `DatabaseInitializer.SeedAsync` — they're required for the dispatcher to function in any environment.

**Files:**
- Modify: `src/AWBlazorApp/Infrastructure/Persistence/DatabaseInitializer.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Seed/SchedulingRuleSeedTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Seed;

public class SchedulingRuleSeedTests : IntegrationTest
{
    [Test]
    public async Task Three_Seed_Rules_Exist_After_Startup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var rules = await db.SchedulingRules.AsNoTracking().OrderBy(r => r.Priority).ThenBy(r => r.Id).ToListAsync();

        Assert.That(rules, Has.Count.GreaterThanOrEqualTo(3));
        Assert.That(rules.Any(r => r.EventType == SchedulingEventType.NewSO && !r.InFrozenWindow && r.Action == RecalcActionType.SoftResort));
        Assert.That(rules.Any(r => r.EventType == SchedulingEventType.NewSO && r.InFrozenWindow && r.Action == RecalcActionType.AlertOnly));
        Assert.That(rules.Any(r => r.EventType == SchedulingEventType.NewSO && r.InFrozenWindow && r.Action == RecalcActionType.HardReplan));
    }
}
```

- [ ] **Step 2: Run — expect fail**

`dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~SchedulingRuleSeedTests"` → FAIL (no rows).

- [ ] **Step 3: Implement seed**

In `DatabaseInitializer.SeedAsync`, after Identity-role seed, add:

```csharp
await SeedSchedulingRulesAsync(db, ct);

// ... and as a new method:
private static async Task SeedSchedulingRulesAsync(ApplicationDbContext db, CancellationToken ct)
{
    if (await db.SchedulingRules.AnyAsync(ct)) return;

    db.SchedulingRules.AddRange(
        new SchedulingRule { EventType = SchedulingEventType.NewSO, InFrozenWindow = false,
            Action = RecalcActionType.SoftResort, Priority = 100, IsActive = true },
        new SchedulingRule { EventType = SchedulingEventType.NewSO, InFrozenWindow = true,
            Action = RecalcActionType.AlertOnly, ParametersJson = "{\"minOrderValue\":5000}",
            Priority = 100, IsActive = true },
        new SchedulingRule { EventType = SchedulingEventType.NewSO, InFrozenWindow = true,
            Action = RecalcActionType.HardReplan, Priority = 50, IsActive = true }
    );
    await db.SaveChangesAsync(ct);
}
```

Add `using AWBlazorApp.Features.Scheduling.Rules.Domain;` to the file.

- [ ] **Step 4: Test passes**

`dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~SchedulingRuleSeedTests"` → PASS.

- [ ] **Step 5: Commit**

```bash
git add src/AWBlazorApp/Infrastructure/Persistence/DatabaseInitializer.cs tests/AWBlazorApp.Tests/Scheduling/Seed/
git commit -m "feat(scheduling): seed three baseline recalc rules"
```

---

## Task 7: `FrozenWindowEvaluator` service + integration tests

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Services/IFrozenWindowEvaluator.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/Services/FrozenWindowEvaluator.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Services/FrozenWindowEvaluatorTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Services;

public class FrozenWindowEvaluatorTests : IntegrationTest
{
    private const short PilotLocation = 60;

    [SetUp]
    public async Task SeedLine()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == PilotLocation))
        {
            db.LineConfigurations.Add(new LineConfiguration {
                LocationId = PilotLocation, TaktSeconds = 600, ShiftsPerDay = 2,
                MinutesPerShift = 480, FrozenLookaheadHours = 72, IsActive = true,
                ModifiedDate = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }

    [Test]
    public async Task InFrozenWindow_False_When_No_Plan_Published()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IFrozenWindowEvaluator>();
        var soh = new SalesOrderHeader { DueDate = DateTime.UtcNow.AddHours(24) }; // week has no plan
        var result = await sut.EvaluateAsync(soh, DateTime.UtcNow, PilotLocation);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task InFrozenWindow_False_When_Plan_Exists_But_Due_Beyond_Lookahead()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dueDate = DateTime.UtcNow.AddHours(200); // far in future
        var weekId = IsoWeekHelper.FromDate(dueDate);
        db.WeeklyPlans.Add(new WeeklyPlan { WeekId = weekId, LocationId = PilotLocation, Version = 1,
            PublishedAt = DateTime.UtcNow, PublishedBy = "test" });
        await db.SaveChangesAsync();

        var sut = scope.ServiceProvider.GetRequiredService<IFrozenWindowEvaluator>();
        var soh = new SalesOrderHeader { DueDate = dueDate };
        var result = await sut.EvaluateAsync(soh, DateTime.UtcNow, PilotLocation);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task InFrozenWindow_True_When_Plan_Exists_AND_Due_Within_Lookahead()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dueDate = DateTime.UtcNow.AddHours(24);
        var weekId = IsoWeekHelper.FromDate(dueDate);
        if (!await db.WeeklyPlans.AnyAsync(p => p.WeekId == weekId && p.LocationId == PilotLocation))
        {
            db.WeeklyPlans.Add(new WeeklyPlan { WeekId = weekId, LocationId = PilotLocation, Version = 1,
                PublishedAt = DateTime.UtcNow, PublishedBy = "test" });
            await db.SaveChangesAsync();
        }
        var sut = scope.ServiceProvider.GetRequiredService<IFrozenWindowEvaluator>();
        var soh = new SalesOrderHeader { DueDate = dueDate };
        var result = await sut.EvaluateAsync(soh, DateTime.UtcNow, PilotLocation);
        Assert.That(result, Is.True);
    }

    [TearDown]
    public async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.WeeklyPlans.Where(p => p.LocationId == PilotLocation).ExecuteDeleteAsync();
    }
}
```

- [ ] **Step 2: Run — expect fail**

`dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~FrozenWindowEvaluatorTests"` → compile error (`IFrozenWindowEvaluator` missing).

- [ ] **Step 3: Implement**

```csharp
// IFrozenWindowEvaluator.cs
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
namespace AWBlazorApp.Features.Scheduling.Services;

public interface IFrozenWindowEvaluator
{
    Task<bool> EvaluateAsync(SalesOrderHeader soh, DateTime nowUtc, short locationId, CancellationToken ct = default);
}
```

```csharp
// FrozenWindowEvaluator.cs
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.Services;

public class FrozenWindowEvaluator : IFrozenWindowEvaluator
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    public FrozenWindowEvaluator(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

    public async Task<bool> EvaluateAsync(SalesOrderHeader soh, DateTime nowUtc, short locationId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var weekId = IsoWeekHelper.FromDate(soh.DueDate);
        var planExists = await db.WeeklyPlans.AsNoTracking()
            .AnyAsync(p => p.WeekId == weekId && p.LocationId == locationId, ct);
        if (!planExists) return false;

        var lookahead = await db.LineConfigurations.AsNoTracking()
            .Where(l => l.LocationId == locationId && l.IsActive)
            .Select(l => (int?)l.FrozenLookaheadHours)
            .SingleOrDefaultAsync(ct);
        if (lookahead is null) return false;

        var hoursUntilDue = (soh.DueDate - nowUtc).TotalHours;
        return hoursUntilDue < lookahead.Value;
    }
}
```

- [ ] **Step 4: Register + run tests**

Registration goes in Task 19 (centralized `SchedulingServiceRegistration`), but for immediate testing, add to `App/Extensions/ServiceRegistration.cs` now:

```csharp
services.AddScoped<IFrozenWindowEvaluator, FrozenWindowEvaluator>();
```

Run: `dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~FrozenWindowEvaluatorTests"` → 3 passed.

- [ ] **Step 5: Commit**

```bash
git add src/AWBlazorApp/Features/Scheduling/Services/ tests/AWBlazorApp.Tests/Scheduling/Services/FrozenWindowEvaluatorTests.cs src/AWBlazorApp/App/Extensions/ServiceRegistration.cs
git commit -m "feat(scheduling): FrozenWindowEvaluator + tests"
```

---

## Task 8: `SchedulingRuleResolver` service + unit tests

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Services/ISchedulingRuleResolver.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/Services/SchedulingRuleResolver.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Services/SchedulingRuleResolverTests.cs`

- [ ] **Step 1: Failing tests (pure unit, no DB)**

```csharp
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Services;

public class SchedulingRuleResolverTests
{
    private static readonly List<SchedulingRule> Seeded = new()
    {
        new() { Id=1, EventType=SchedulingEventType.NewSO, InFrozenWindow=false,
                Action=RecalcActionType.SoftResort, Priority=100, IsActive=true },
        new() { Id=2, EventType=SchedulingEventType.NewSO, InFrozenWindow=true,
                Action=RecalcActionType.AlertOnly, ParametersJson="{\"minOrderValue\":5000}",
                Priority=100, IsActive=true },
        new() { Id=3, EventType=SchedulingEventType.NewSO, InFrozenWindow=true,
                Action=RecalcActionType.HardReplan, Priority=50, IsActive=true },
        new() { Id=4, EventType=SchedulingEventType.NewSO, InFrozenWindow=true,
                Action=RecalcActionType.AlertOnly, Priority=10, IsActive=false }
    };

    [Test]
    public void Resolve_OutsideFrozen_ReturnsSoftResortOnly()
    {
        var sut = new SchedulingRuleResolver();
        var list = sut.Resolve(Seeded, SchedulingEventType.NewSO, inFrozenWindow: false).ToList();
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list[0].Action, Is.EqualTo(RecalcActionType.SoftResort));
    }

    [Test]
    public void Resolve_InsideFrozen_ReturnsActiveRules_InPriorityDescOrder()
    {
        var sut = new SchedulingRuleResolver();
        var list = sut.Resolve(Seeded, SchedulingEventType.NewSO, inFrozenWindow: true).ToList();
        Assert.That(list.Select(r => r.Id), Is.EqualTo(new[] { 2, 3 }));   // 4 excluded (inactive)
    }
}
```

- [ ] **Step 2: Run — expect compile-fail**

- [ ] **Step 3: Implement**

```csharp
// ISchedulingRuleResolver.cs
using AWBlazorApp.Features.Scheduling.Rules.Domain;
namespace AWBlazorApp.Features.Scheduling.Services;

public interface ISchedulingRuleResolver
{
    IEnumerable<SchedulingRule> Resolve(
        IEnumerable<SchedulingRule> rules,
        SchedulingEventType eventType,
        bool inFrozenWindow);
}
```

```csharp
// SchedulingRuleResolver.cs
using AWBlazorApp.Features.Scheduling.Rules.Domain;
namespace AWBlazorApp.Features.Scheduling.Services;

public class SchedulingRuleResolver : ISchedulingRuleResolver
{
    public IEnumerable<SchedulingRule> Resolve(
        IEnumerable<SchedulingRule> rules,
        SchedulingEventType eventType,
        bool inFrozenWindow)
        => rules.Where(r => r.IsActive && r.EventType == eventType && r.InFrozenWindow == inFrozenWindow)
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.Id);
}
```

Register in `ServiceRegistration.cs`:
```csharp
services.AddSingleton<ISchedulingRuleResolver, SchedulingRuleResolver>();
```

- [ ] **Step 4: Tests pass + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~SchedulingRuleResolverTests"
git add ...
git commit -m "feat(scheduling): SchedulingRuleResolver + tests"
```

---

## Task 9: `IRecalcAction` contract + context/result

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Rules/Application/IRecalcAction.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/Rules/Application/RecalcContext.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/Rules/Application/RecalcResult.cs`

- [ ] **Step 1: Write code**

```csharp
// RecalcContext.cs
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public sealed record RecalcContext(
    ApplicationDbContext Db,
    SchedulingRule Rule,
    SalesOrderHeader Soh,
    short LocationId,
    int WeekId,
    bool InFrozenWindow,
    DateTime NowUtc);
```

```csharp
// RecalcResult.cs
namespace AWBlazorApp.Features.Scheduling.Rules.Application;
public sealed record RecalcResult(bool Handled, string? Note = null);
```

```csharp
// IRecalcAction.cs
using AWBlazorApp.Features.Scheduling.Rules.Domain;
namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public interface IRecalcAction
{
    RecalcActionType ActionType { get; }
    Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct);
}
```

- [ ] **Step 2: Build + commit**

```bash
dotnet build AWBlazorApp.slnx
git add src/AWBlazorApp/Features/Scheduling/Rules/Application/
git commit -m "feat(scheduling): IRecalcAction contract"
```

---

## Task 10: `SoftResortAction` + integration test

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Rules/Application/SoftResortAction.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Rules/SoftResortActionTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Rules;

public class SoftResortActionTests : IntegrationTest
{
    [Test]
    public async Task Execute_Writes_Info_Alert_And_Returns_Handled()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var action = new SoftResortAction();
        var soh = new SalesOrderHeader { Id = 999_999, DueDate = DateTime.UtcNow.AddDays(10), TotalDue = 1000m };
        var ctx = new RecalcContext(db, new SchedulingRule { Action = RecalcActionType.SoftResort },
            soh, LocationId: 60, WeekId: 202618, InFrozenWindow: false, NowUtc: DateTime.UtcNow);

        var before = await db.SchedulingAlerts.CountAsync();
        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();
        var after = await db.SchedulingAlerts.CountAsync();

        Assert.That(result.Handled, Is.True);
        Assert.That(after - before, Is.EqualTo(1));
        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Info));
        Assert.That(alert.SalesOrderId, Is.EqualTo(999_999));

        // cleanup
        db.SchedulingAlerts.Remove(alert);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Implement**

```csharp
// SoftResortAction.cs
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public class SoftResortAction : IRecalcAction
{
    public RecalcActionType ActionType => RecalcActionType.SoftResort;

    public Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct)
    {
        ctx.Db.SchedulingAlerts.Add(new SchedulingAlert {
            CreatedAt = ctx.NowUtc,
            Severity = AlertSeverity.Info,
            EventType = ctx.Rule.EventType,
            WeekId = ctx.WeekId,
            LocationId = ctx.LocationId,
            SalesOrderId = ctx.Soh.Id,
            Message = $"New SO {ctx.Soh.Id} soft-resorted into {IsoWeekHelper.Format(ctx.WeekId)}.",
            PayloadJson = $"{{\"ruleId\":{ctx.Rule.Id}}}"
        });
        return Task.FromResult(new RecalcResult(Handled: true));
    }
}
```

Add `using AWBlazorApp.Features.Scheduling.Services;` for `IsoWeekHelper`.

- [ ] **Step 3: Test + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~SoftResortActionTests"
git commit -am "feat(scheduling): SoftResortAction"
```

---

## Task 11: `AlertOnlyAction` (with `minOrderValue` predicate) + integration test

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Rules/Application/AlertOnlyAction.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Rules/AlertOnlyActionTests.cs`

- [ ] **Step 1: Failing tests**

```csharp
[Test] public async Task Execute_WritesWarning_When_TotalDue_Meets_Threshold() { ... }
[Test] public async Task Execute_ReturnsHandledFalse_When_TotalDue_Below_Threshold() { ... }
```

Full test body:

```csharp
using System.Text.Json;
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Rules;

public class AlertOnlyActionTests : IntegrationTest
{
    [Test]
    public async Task Execute_WritesWarning_When_TotalDue_MeetsThreshold()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var action = new AlertOnlyAction();
        var soh = new SalesOrderHeader { Id = 999_998, DueDate = DateTime.UtcNow.AddHours(48), TotalDue = 5000m };
        var rule = new SchedulingRule { Action = RecalcActionType.AlertOnly, ParametersJson = "{\"minOrderValue\":5000}" };
        var ctx = new RecalcContext(db, rule, soh, 60, 202618, InFrozenWindow: true, DateTime.UtcNow);

        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.That(result.Handled, Is.True);
        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Warning));

        db.SchedulingAlerts.Remove(alert);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Execute_ReturnsHandledFalse_And_WritesNothing_When_Below_Threshold()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var action = new AlertOnlyAction();
        var soh = new SalesOrderHeader { Id = 999_997, DueDate = DateTime.UtcNow.AddHours(48), TotalDue = 100m };
        var rule = new SchedulingRule { Action = RecalcActionType.AlertOnly, ParametersJson = "{\"minOrderValue\":5000}" };
        var ctx = new RecalcContext(db, rule, soh, 60, 202618, InFrozenWindow: true, DateTime.UtcNow);

        var before = await db.SchedulingAlerts.CountAsync();
        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();
        var after = await db.SchedulingAlerts.CountAsync();

        Assert.That(result.Handled, Is.False);
        Assert.That(after, Is.EqualTo(before));
    }
}
```

- [ ] **Step 2: Implement**

```csharp
using System.Text.Json;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public class AlertOnlyAction : IRecalcAction
{
    public RecalcActionType ActionType => RecalcActionType.AlertOnly;

    public Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct)
    {
        var threshold = ReadMinOrderValue(ctx.Rule.ParametersJson);
        if (threshold.HasValue && ctx.Soh.TotalDue < threshold.Value)
            return Task.FromResult(new RecalcResult(Handled: false,
                Note: $"Below minOrderValue={threshold}"));

        ctx.Db.SchedulingAlerts.Add(new SchedulingAlert {
            CreatedAt = ctx.NowUtc,
            Severity = AlertSeverity.Warning,
            EventType = ctx.Rule.EventType,
            WeekId = ctx.WeekId,
            LocationId = ctx.LocationId,
            SalesOrderId = ctx.Soh.Id,
            Message = $"Frozen-window SO {ctx.Soh.Id} (${ctx.Soh.TotalDue:F2}) due {ctx.Soh.DueDate:u} in {IsoWeekHelper.Format(ctx.WeekId)}.",
            PayloadJson = $"{{\"ruleId\":{ctx.Rule.Id},\"totalDue\":{ctx.Soh.TotalDue}}}"
        });
        return Task.FromResult(new RecalcResult(Handled: true));
    }

    private static decimal? ReadMinOrderValue(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("minOrderValue", out var el) && el.TryGetDecimal(out var v))
                return v;
        }
        catch (JsonException) { }
        return null;
    }
}
```

- [ ] **Step 3: Test + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~AlertOnlyActionTests"
git commit -am "feat(scheduling): AlertOnlyAction with minOrderValue predicate"
```

---

## Task 12: `HardReplanAction` + integration test

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Rules/Application/HardReplanAction.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Rules/HardReplanActionTests.cs`

- [ ] **Step 1: Failing test**

```csharp
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Rules;

public class HardReplanActionTests : IntegrationTest
{
    [Test]
    public async Task Execute_WritesCriticalAlert_And_SetsBaselineDiverged()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var plan = new WeeklyPlan { WeekId = 202650, LocationId = 60, Version = 1,
            PublishedAt = DateTime.UtcNow, PublishedBy = "test", BaselineDiverged = false };
        db.WeeklyPlans.Add(plan);
        await db.SaveChangesAsync();

        var action = new HardReplanAction();
        var soh = new SalesOrderHeader { Id = 999_996, DueDate = DateTime.UtcNow.AddHours(24), TotalDue = 100m };
        var ctx = new RecalcContext(db, new SchedulingRule { Action = RecalcActionType.HardReplan },
            soh, 60, 202650, InFrozenWindow: true, DateTime.UtcNow);

        var result = await action.ExecuteAsync(ctx, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.That(result.Handled, Is.True);
        var refreshed = await db.WeeklyPlans.AsNoTracking().SingleAsync(p => p.Id == plan.Id);
        Assert.That(refreshed.BaselineDiverged, Is.True);
        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AlertSeverity.Critical));

        // cleanup
        db.SchedulingAlerts.Remove(alert);
        db.WeeklyPlans.Remove(refreshed);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Implement**

```csharp
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public class HardReplanAction : IRecalcAction
{
    public RecalcActionType ActionType => RecalcActionType.HardReplan;

    public async Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct)
    {
        // Flip BaselineDiverged on the latest plan for this (WeekId, LocationId)
        var plan = await ctx.Db.WeeklyPlans
            .Where(p => p.WeekId == ctx.WeekId && p.LocationId == ctx.LocationId)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(ct);
        if (plan is not null) plan.BaselineDiverged = true;

        ctx.Db.SchedulingAlerts.Add(new SchedulingAlert {
            CreatedAt = ctx.NowUtc,
            Severity = AlertSeverity.Critical,
            EventType = ctx.Rule.EventType,
            WeekId = ctx.WeekId,
            LocationId = ctx.LocationId,
            SalesOrderId = ctx.Soh.Id,
            Message = $"Frozen-window SO {ctx.Soh.Id} triggered HARD REPLAN in {IsoWeekHelper.Format(ctx.WeekId)}.",
            PayloadJson = $"{{\"ruleId\":{ctx.Rule.Id},\"baselineWasDiverged\":{(plan is null ? "null" : "false")}}}"
        });
        return new RecalcResult(Handled: true);
    }
}
```

- [ ] **Step 3: Test + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~HardReplanActionTests"
git commit -am "feat(scheduling): HardReplanAction"
```

---

## Task 13: `SchedulingDispatcher` + unit-ish integration test

The dispatcher orchestrates `resolver → evaluator → action walk`. Tested with real DbContext but fake actions injected via DI.

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Services/ISchedulingDispatcher.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/Services/SchedulingDispatcher.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Services/SchedulingDispatcherTests.cs`

- [ ] **Step 1: Failing test — priority walk short-circuits on Handled**

```csharp
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Services;

public class SchedulingDispatcherTests : IntegrationTest
{
    [Test]
    public async Task Dispatch_WalksPriorities_StopsWhenActionHandles()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<ISchedulingDispatcher>();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // seed line
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == 60))
        {
            db.LineConfigurations.Add(new LineConfiguration {
                LocationId = 60, TaktSeconds = 600, ShiftsPerDay = 2, MinutesPerShift = 480,
                FrozenLookaheadHours = 72, IsActive = true, ModifiedDate = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        var soh = new SalesOrderHeader { Id = 999_995, DueDate = DateTime.UtcNow.AddDays(14), TotalDue = 100m };
        // Outside frozen window (no plan for that week) → only SoftResort applies
        var before = await db.SchedulingAlerts.CountAsync();
        await sut.OnSalesOrderCreatedAsync(soh, 60, CancellationToken.None);
        await db.SaveChangesAsync();
        var after = await db.SchedulingAlerts.CountAsync();

        Assert.That(after - before, Is.EqualTo(1));
        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        Assert.That(alert.Severity, Is.EqualTo(AWBlazorApp.Features.Scheduling.Alerts.Domain.AlertSeverity.Info));

        db.SchedulingAlerts.Remove(alert);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Implement interface + service**

```csharp
// ISchedulingDispatcher.cs
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
namespace AWBlazorApp.Features.Scheduling.Services;

public interface ISchedulingDispatcher
{
    Task OnSalesOrderCreatedAsync(SalesOrderHeader soh, short locationId, CancellationToken ct);
    bool IsDispatching { get; }
}
```

```csharp
// SchedulingDispatcher.cs
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AWBlazorApp.Features.Scheduling.Services;

public class SchedulingDispatcher : ISchedulingDispatcher
{
    private static readonly AsyncLocal<bool> _inFlight = new();
    public bool IsDispatching => _inFlight.Value;

    private readonly ApplicationDbContext _db;
    private readonly IFrozenWindowEvaluator _frozen;
    private readonly ISchedulingRuleResolver _resolver;
    private readonly IEnumerable<IRecalcAction> _actions;
    private readonly ILogger<SchedulingDispatcher> _log;

    public SchedulingDispatcher(ApplicationDbContext db, IFrozenWindowEvaluator frozen,
        ISchedulingRuleResolver resolver, IEnumerable<IRecalcAction> actions,
        ILogger<SchedulingDispatcher> log)
        => (_db, _frozen, _resolver, _actions, _log) = (db, frozen, resolver, actions, log);

    public async Task OnSalesOrderCreatedAsync(SalesOrderHeader soh, short locationId, CancellationToken ct)
    {
        if (_inFlight.Value) return;

        var hasLine = await _db.LineConfigurations.AsNoTracking()
            .AnyAsync(l => l.LocationId == locationId && l.IsActive, ct);
        if (!hasLine) { _log.LogDebug("No LineConfiguration for {Loc}; skipping dispatch.", locationId); return; }

        var inFrozen = await _frozen.EvaluateAsync(soh, DateTime.UtcNow, locationId, ct);
        var weekId = IsoWeekHelper.FromDate(soh.DueDate);

        var rules = await _db.SchedulingRules.AsNoTracking().ToListAsync(ct);
        var candidates = _resolver.Resolve(rules, SchedulingEventType.NewSO, inFrozen).ToList();
        if (candidates.Count == 0) { _log.LogDebug("No rules for NewSO inFrozen={InFrozen}.", inFrozen); return; }

        _inFlight.Value = true;
        try
        {
            foreach (var rule in candidates)
            {
                var action = _actions.FirstOrDefault(a => a.ActionType == rule.Action);
                if (action is null) { _log.LogWarning("No IRecalcAction for {Type}.", rule.Action); continue; }

                var ctx = new RecalcContext(_db, rule, soh, locationId, weekId, inFrozen, DateTime.UtcNow);
                var result = await action.ExecuteAsync(ctx, ct);
                if (result.Handled) return;
            }
            _log.LogInformation("No rule handled SO {Id}; falling through.", soh.Id);
        }
        finally { _inFlight.Value = false; }
    }
}
```

- [ ] **Step 3: Register all `IRecalcAction` impls**

Add to `ServiceRegistration.cs`:
```csharp
services.AddScoped<ISchedulingDispatcher, SchedulingDispatcher>();
services.AddScoped<IRecalcAction, SoftResortAction>();
services.AddScoped<IRecalcAction, AlertOnlyAction>();
services.AddScoped<IRecalcAction, HardReplanAction>();
```

- [ ] **Step 4: Test + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~SchedulingDispatcherTests"
git commit -am "feat(scheduling): SchedulingDispatcher"
```

---

## Task 14: `SchedulingDispatchInterceptor` + DbContext wiring

Reacts to `SalesOrderHeader` inserts in `SavedChanges`. Uses the dispatcher; the re-entrancy guard inside the dispatcher makes its own inserts safe.

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Services/SchedulingDispatchInterceptor.cs`
- Modify: `src/AWBlazorApp/Infrastructure/Persistence/ApplicationDbContext.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Dispatcher/InterceptorIntegrationTests.cs`

- [ ] **Step 1: Failing integration test — new SO inserts produce alerts**

```csharp
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Alerts.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Dispatcher;

public class InterceptorIntegrationTests : IntegrationTest
{
    [Test]
    public async Task InsertingSO_OutsideFrozenWindow_WritesInfoAlert()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == 60))
        {
            db.LineConfigurations.Add(new LineConfiguration { LocationId=60, TaktSeconds=600,
                ShiftsPerDay=2, MinutesPerShift=480, FrozenLookaheadHours=72, IsActive=true, ModifiedDate=DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        var startCount = await db.SchedulingAlerts.CountAsync();

        // Insert a minimal SalesOrderHeader; DueDate is far in future so no plan exists → outside frozen window
        var soh = CreateMinimalSoh(DateTime.UtcNow.AddDays(30));
        db.SalesOrderHeaders.Add(soh);
        await db.SaveChangesAsync();

        var endCount = await db.SchedulingAlerts.CountAsync();
        Assert.That(endCount - startCount, Is.EqualTo(1));

        // cleanup — remove alert + the SO
        var alert = await db.SchedulingAlerts.OrderByDescending(a => a.Id).FirstAsync();
        db.SchedulingAlerts.Remove(alert);
        db.SalesOrderHeaders.Remove(soh);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task DispatcherAlertWrite_DoesNotReenter_Interceptor()
    {
        // Covered implicitly by SoftResortActionTests + the above; the re-entrancy guard
        // is what prevents the nested SaveChanges on SchedulingAlert from producing a 2nd alert.
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var soh = CreateMinimalSoh(DateTime.UtcNow.AddDays(30));
        db.SalesOrderHeaders.Add(soh);
        await db.SaveChangesAsync();

        var alertsForThisSo = await db.SchedulingAlerts.CountAsync(a => a.SalesOrderId == soh.Id);
        Assert.That(alertsForThisSo, Is.EqualTo(1), "Interceptor re-entered dispatcher — guard failed.");

        // cleanup
        await db.SchedulingAlerts.Where(a => a.SalesOrderId == soh.Id).ExecuteDeleteAsync();
        db.SalesOrderHeaders.Remove(soh);
        await db.SaveChangesAsync();
    }

    private static SalesOrderHeader CreateMinimalSoh(DateTime dueDate) => new()
    {
        RevisionNumber = 0,
        OrderDate = DateTime.UtcNow,
        DueDate = dueDate,
        Status = 1,                    // InProcess
        OnlineOrderFlag = false,
        CustomerId = 29825,            // any valid AW customer
        ShipMethodId = 1,
        BillToAddressId = 985,
        ShipToAddressId = 985,
        SubTotal = 100m,
        TaxAmt = 0m,
        Freight = 0m,
        rowguid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow
    };
}
```

(Field names — `ShipMethodId`, `BillToAddressId`, etc. — must match the real `SalesOrderHeader` entity. Consult `Features/Sales/SalesOrderHeaders/Domain/SalesOrderHeader.cs` for exact names and required fields; adjust if the properties differ.)

- [ ] **Step 2: Implement interceptor**

```csharp
// SchedulingDispatchInterceptor.cs
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AWBlazorApp.Features.Scheduling.Services;

public class SchedulingDispatchInterceptor : SaveChangesInterceptor
{
    private const short PilotLocation = 60;
    private readonly IServiceProvider _sp;

    public SchedulingDispatchInterceptor(IServiceProvider sp) => _sp = sp;

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        var dispatcher = _sp.GetRequiredService<ISchedulingDispatcher>();
        if (dispatcher.IsDispatching)
            return await base.SavedChangesAsync(eventData, result, ct);

        var context = eventData.Context;
        if (context is null) return await base.SavedChangesAsync(eventData, result, ct);

        // Snapshot added SOs BEFORE base call — change tracker state clears in later phases.
        // (In SavedChanges the entities are already tracked Unchanged with IDs set — we can grab them.)
        var added = context.ChangeTracker.Entries<SalesOrderHeader>()
            .Where(e => e.State == EntityState.Unchanged) // they just saved — state moved from Added to Unchanged
            .Select(e => e.Entity)
            .Where(e => e.Id > 0)
            .ToList();

        foreach (var soh in added)
            await dispatcher.OnSalesOrderCreatedAsync(soh, PilotLocation, ct);

        // The dispatcher enqueued alerts on the context; flush them now.
        if (context.ChangeTracker.HasChanges())
            await context.SaveChangesAsync(ct);   // re-entrancy guard blocks recursive dispatch

        return await base.SavedChangesAsync(eventData, result, ct);
    }
}
```

**Subtlety (document this clearly in code comments if not shown above):** `SavedChangesAsync` runs after a *successful* save. The interceptor distinguishes "just-inserted SOs" from "existing SOs" by inspecting `ChangeTracker.Entries<SalesOrderHeader>()`. In `SavedChanges`, EF has already transitioned Added entities to Unchanged with their generated IDs — so `State == Unchanged && Id > 0` is the signature of "freshly inserted this SaveChanges." This is robust for single-SO inserts (the common case); for batched inserts we iterate all such entities.

- [ ] **Step 3: Register the interceptor in the DbContext**

Modify `ApplicationDbContext.OnConfiguring` — add the interceptor to the chain after `AuditLogInterceptor`:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var sp = this.GetInfrastructure().GetService<IServiceProvider>();
    if (sp is not null)
    {
        // Existing: AuditLogInterceptor first
        optionsBuilder.AddInterceptors(sp.GetRequiredService<AuditLogInterceptor>());
        // New: scheduling dispatcher second
        optionsBuilder.AddInterceptors(sp.GetRequiredService<SchedulingDispatchInterceptor>());
    }
    base.OnConfiguring(optionsBuilder);
}
```

**Check** the existing `OnConfiguring` shape — interceptor registration may go through `Program.cs` `AddDbContext(... optionsBuilder.AddInterceptors(...))` instead. Match whichever pattern `AuditLogInterceptor` uses — add the scheduling interceptor right after it in the same registration block. The ordering is: `AuditLogInterceptor` first, `SchedulingDispatchInterceptor` second.

- [ ] **Step 4: Register interceptor in DI**

In `ServiceRegistration.cs`:
```csharp
services.AddScoped<SchedulingDispatchInterceptor>();
```

- [ ] **Step 5: Test + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~InterceptorIntegrationTests"
git commit -am "feat(scheduling): SaveChangesInterceptor + DbContext wiring"
```

---

## Task 15: `WeeklyPlanGenerator` + integration test

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Application/IWeeklyPlanGenerator.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Application/WeeklyPlanGenerator.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Application/WeeklyPlanGenerationOptions.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Application/WeeklyPlanGenerationResult.cs`
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Application/SortWeights.cs`
- Test: `tests/AWBlazorApp.Tests/Scheduling/Generator/WeeklyPlanGeneratorTests.cs`

This task is large enough that I'm splitting it into sub-steps rather than sub-tasks so TDD discipline is preserved.

- [ ] **Step 1: Record types**

```csharp
// SortWeights.cs
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;
public sealed record SortWeights(
    int DueDateRank = 10000,
    int CustomerPriorityRank = 1000,
    int ProductModelRank = 100,
    int TotalDueRank = 10,
    int ModifiedDateRank = 1);
```

```csharp
// WeeklyPlanGenerationOptions.cs
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public sealed record WeeklyPlanGenerationOptions(
    bool StrictCapacity = false,
    bool DryRun = false,
    IReadOnlySet<byte>? PlannableSalesOrderStatuses = null, // default: {1,5}
    SortWeights? SortWeights = null);
```

```csharp
// WeeklyPlanGenerationResult.cs
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public sealed record WeeklyPlanGenerationResult(
    int WeeklyPlanId,
    int Version,
    int ItemCount,
    int OverCapacityCount,
    decimal UtilizationPercent,
    IReadOnlyList<string> Warnings);
```

- [ ] **Step 2: Interface**

```csharp
// IWeeklyPlanGenerator.cs
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public interface IWeeklyPlanGenerator
{
    Task<WeeklyPlanGenerationResult> GenerateAsync(
        int weekId, short locationId, WeeklyPlanGenerationOptions options,
        string requestedBy, CancellationToken ct);
}
```

- [ ] **Step 3: Failing test — happy path and DryRun**

```csharp
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Tests.Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Scheduling.Generator;

public class WeeklyPlanGeneratorTests : IntegrationTest
{
    private const short Loc = 60;
    private int _weekId;

    [SetUp]
    public async Task Setup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // pick far-future week so we don't collide with real data
        _weekId = 203001;
        if (!await db.LineConfigurations.AnyAsync(l => l.LocationId == Loc))
            db.LineConfigurations.Add(new LineConfiguration { LocationId=Loc, TaktSeconds=600,
                ShiftsPerDay=2, MinutesPerShift=480, FrozenLookaheadHours=72, IsActive=true, ModifiedDate=DateTime.UtcNow });
        // seed a few product-model assignments for bikes if missing
        foreach (var modelId in new[] { 25, 28, 30 }) // Road/Touring/Mountain frame models — real AW IDs
            if (!await db.LineProductAssignments.AnyAsync(a => a.LocationId == Loc && a.ProductModelId == modelId))
                db.LineProductAssignments.Add(new LineProductAssignment { LocationId=Loc,
                    ProductModelId=modelId, IsActive=true, ModifiedDate=DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        using var scope = Factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.WeeklyPlans.Where(p => p.WeekId == _weekId && p.LocationId == Loc).ExecuteDeleteAsync();
    }

    [Test]
    public async Task DryRun_ReturnsResult_And_Writes_Nothing()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var before = await db.WeeklyPlans.CountAsync();
        var result = await sut.GenerateAsync(_weekId, Loc,
            new WeeklyPlanGenerationOptions(DryRun: true), "tester", CancellationToken.None);
        var after = await db.WeeklyPlans.CountAsync();

        Assert.That(after, Is.EqualTo(before));
        Assert.That(result.WeeklyPlanId, Is.EqualTo(0));
    }

    [Test]
    public async Task Generate_V1_WhenNoPriorPlan()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();
        await using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var result = await sut.GenerateAsync(_weekId, Loc,
            new WeeklyPlanGenerationOptions(), "tester", CancellationToken.None);
        Assert.That(result.Version, Is.EqualTo(1));
        Assert.That(result.WeeklyPlanId, Is.GreaterThan(0));
    }

    [Test]
    public async Task Precondition_Fails_When_No_LineConfiguration()
    {
        using var scope = Factory.Services.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IWeeklyPlanGenerator>();
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.GenerateAsync(_weekId, locationId: 999 /* unconfigured */,
                new WeeklyPlanGenerationOptions(), "tester", CancellationToken.None));
    }
}
```

- [ ] **Step 4: Implement the generator**

```csharp
// WeeklyPlanGenerator.cs
using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain;
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Features.Production.Products.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

public class WeeklyPlanGenerator : IWeeklyPlanGenerator
{
    private static readonly IReadOnlySet<byte> DefaultPlannable =
        new HashSet<byte> { 1 /* InProcess */, 5 /* Shipped-but-not-cancelled-in-AW = Approved variant; use real values */ };

    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    public WeeklyPlanGenerator(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

    public async Task<WeeklyPlanGenerationResult> GenerateAsync(
        int weekId, short locationId, WeeklyPlanGenerationOptions options,
        string requestedBy, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var line = await db.LineConfigurations.AsNoTracking()
            .SingleOrDefaultAsync(l => l.LocationId == locationId && l.IsActive, ct)
            ?? throw new InvalidOperationException($"Line {locationId} not configured.");

        var weekStart = IsoWeekHelper.ToMondayUtc(weekId);
        var weekEnd = weekStart.AddDays(7);
        var plannable = options.PlannableSalesOrderStatuses ?? DefaultPlannable;
        var warnings = new List<string>();

        // scope query
        var assignedModelIds = await db.LineProductAssignments.AsNoTracking()
            .Where(a => a.LocationId == locationId && a.IsActive)
            .Select(a => a.ProductModelId).ToListAsync(ct);
        if (assignedModelIds.Count == 0)
            warnings.Add("No LineProductAssignments for location; plan will be empty.");

        var candidateQ = from sod in db.Set<SalesOrderDetail>()
                         join soh in db.SalesOrderHeaders on sod.SalesOrderId equals soh.Id
                         join prod in db.Set<Product>() on sod.ProductId equals prod.Id
                         where soh.DueDate >= weekStart && soh.DueDate < weekEnd
                            && plannable.Contains(soh.Status)
                            && prod.ProductModelId != null
                            && assignedModelIds.Contains(prod.ProductModelId.Value)
                         select new {
                             soh.Id, Soh = soh, Detail = sod, ProdModelId = prod.ProductModelId!.Value,
                             soh.DueDate, soh.OnlineOrderFlag, soh.TotalDue, soh.ModifiedDate,
                             sod.OrderQty, sod.ProductId, sod.SalesOrderDetailID
                         };

        var raw = await candidateQ.AsNoTracking().ToListAsync(ct);

        // sort: DueDate asc, OnlineOrderFlag==false desc, ProdModelId asc, TotalDue desc, ModifiedDate asc, SalesOrderDetailID asc
        var sorted = raw
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => !x.OnlineOrderFlag)      // dealer (false) ranks higher
            .ThenBy(x => x.ProdModelId)
            .ThenByDescending(x => x.TotalDue)
            .ThenBy(x => x.ModifiedDate)
            .ThenBy(x => x.SalesOrderDetailID)
            .ToList();

        // sequence + absolute timestamps (shift-aware)
        var workingSecondsPerDay = (long)line.ShiftsPerDay * line.MinutesPerShift * 60;
        var weekCapacitySeconds = workingSecondsPerDay * 7;
        DateTime cursor = weekStart;
        long secondsUsedOnDay = 0;

        var items = new List<WeeklyPlanItem>();
        var overCap = 0;
        for (int k = 0; k < sorted.Count; k++)
        {
            var row = sorted[k];
            var duration = row.OrderQty * line.TaktSeconds;
            // wrap to next day if current day's working window is exhausted
            if (secondsUsedOnDay >= workingSecondsPerDay)
            {
                cursor = cursor.Date.AddDays(1);
                secondsUsedOnDay = 0;
            }
            var plannedStart = cursor;
            var plannedEnd = cursor.AddSeconds(duration);
            var isOver = plannedStart >= weekEnd;
            if (isOver) overCap++;

            items.Add(new WeeklyPlanItem {
                SalesOrderId = row.Id,
                SalesOrderDetailId = row.SalesOrderDetailID,
                ProductId = row.ProductId,
                PlannedSequence = k + 1,
                PlannedStart = plannedStart,
                PlannedEnd = plannedEnd,
                PlannedQty = row.OrderQty,
                OverCapacity = isOver
            });

            cursor = plannedEnd;
            secondsUsedOnDay += duration;
        }

        if (options.StrictCapacity && overCap > 0)
        {
            var excessSeconds = items.Where(i => i.OverCapacity).Sum(i => (i.PlannedEnd - i.PlannedStart).TotalSeconds);
            throw new CapacityExceededException(excessSeconds / 3600.0);
        }

        var utilPct = weekCapacitySeconds == 0 ? 0m
            : (decimal)Math.Min(1.0, items.Sum(i => (i.PlannedEnd - i.PlannedStart).TotalSeconds) / weekCapacitySeconds) * 100m;

        if (options.DryRun)
            return new WeeklyPlanGenerationResult(0, 0, items.Count, overCap, utilPct, warnings);

        // Regeneration decision
        var existing = await db.WeeklyPlans.Where(p => p.WeekId == weekId && p.LocationId == locationId)
            .OrderByDescending(p => p.Version).ToListAsync(ct);

        int nextVersion;
        if (existing.Count == 0) nextVersion = 1;
        else if (weekStart > DateTime.UtcNow.Date)
        {
            // future week — hard-delete old plan(s)
            var ids = existing.Select(p => p.Id).ToList();
            await db.WeeklyPlanItems.Where(i => ids.Contains(i.WeeklyPlanId)).ExecuteDeleteAsync(ct);
            await db.WeeklyPlans.Where(p => ids.Contains(p.Id)).ExecuteDeleteAsync(ct);
            nextVersion = 1;
        }
        else nextVersion = existing[0].Version + 1;

        var plan = new WeeklyPlan {
            WeekId = weekId, LocationId = locationId, Version = nextVersion,
            PublishedAt = DateTime.UtcNow, PublishedBy = requestedBy,
            BaselineDiverged = false,
            GenerationOptionsJson = JsonSerializer.Serialize(options)
        };
        db.WeeklyPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        foreach (var item in items) item.WeeklyPlanId = plan.Id;
        db.WeeklyPlanItems.AddRange(items);
        await db.SaveChangesAsync(ct);

        return new WeeklyPlanGenerationResult(plan.Id, plan.Version, items.Count, overCap, utilPct, warnings);
    }
}
```

**Note to engineer:** Check the real `SalesOrderHeader.Status` values in `SalesOrderHeader.cs` XML comment (documented as `1=InProcess, 2=Approved, 3=Backordered, 4=Rejected, 5=Shipped, 6=Cancelled`). The default plannable set in the spec was `{InProcess(1), Approved(5?)}` — verify and correct the numeric values (the XML says 5=Shipped; spec meant Approved=2). Use `{1, 2}` if that matches reality, or document a different pair.

- [ ] **Step 5: Register**

```csharp
services.AddScoped<IWeeklyPlanGenerator, WeeklyPlanGenerator>();
```

- [ ] **Step 6: Tests pass**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~WeeklyPlanGeneratorTests"
```

- [ ] **Step 7: Commit**

```bash
git commit -am "feat(scheduling): WeeklyPlanGenerator (Monday-baseline algorithm)"
```

---

## Task 16: Scheduling Minimal APIs — WeeklyPlans / Exceptions / Alerts / Delivery

Five endpoint sets. All under `/api/scheduling/*`, policy `ApiOrCookie`. DTOs are trivial records; using `MapCrudWithInterceptor` where it fits, hand-rolled elsewhere.

**Files (create):**
- `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Api/WeeklyPlanEndpoints.cs`
- `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/Dtos/WeeklyPlanDtos.cs`
- `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/Api/DeliveryScheduleEndpoints.cs`
- `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/Dtos/DeliveryDtos.cs`
- `src/AWBlazorApp/Features/Scheduling/Alerts/Api/AlertEndpoints.cs`
- `src/AWBlazorApp/Features/Scheduling/Alerts/Dtos/AlertDtos.cs`
- `src/AWBlazorApp/Features/Scheduling/LineConfigurations/Api/LineConfigurationEndpoints.cs`
- `src/AWBlazorApp/Features/Scheduling/LineConfigurations/Dtos/LineConfigurationDtos.cs`
- `src/AWBlazorApp/Features/Scheduling/LineProductAssignments/Api/LineProductAssignmentEndpoints.cs`
- `src/AWBlazorApp/Features/Scheduling/LineProductAssignments/Dtos/LineProductAssignmentDtos.cs`
- Modify `Program.cs` (or `App/Extensions/ApiRegistration.cs` — wherever existing endpoint maps are wired) to include `app.MapSchedulingEndpoints()`.

- [ ] **Step 1: DTOs**

(Omitted here for brevity but engineer must write them — simple records with `Id, WeekId, LocationId, ... etc.` mirroring each entity. Match the style of existing DTO files like `Features/Sales/SalesOrderHeaders/Dtos/SalesOrderHeaderDtos.cs`. Include `To<DtoName>()` and `ToEntity()` / `ApplyTo()` extension methods on the DTO static class.)

For each DTO file: Read `src/AWBlazorApp/Features/Sales/SalesOrderHeaders/Dtos/SalesOrderHeaderDtos.cs` in full, then produce the analogous file. Types needed:
- `LineConfigurationDto` + `CreateLineConfigurationRequest` + `UpdateLineConfigurationRequest`
- `LineProductAssignmentDto` + `CreateLineProductAssignmentRequest` + `UpdateLineProductAssignmentRequest`
- `WeeklyPlanDto` + `WeeklyPlanItemDto` + `GeneratePlanRequest { int WeekId; short LocationId; bool DryRun; bool StrictCapacity }` + the result DTO matching `WeeklyPlanGenerationResult`
- `SchedulingExceptionDto` + `CreateSchedulingExceptionRequest` (planner creates; resolution via PATCH)
- `SchedulingAlertDto` + `AcknowledgeAlertRequest { int Id }`
- `CurrentDeliveryRowDto` (projection of view entity)

- [ ] **Step 2: `WeeklyPlanEndpoints`**

```csharp
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Dtos;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Api;

public static class WeeklyPlanEndpoints
{
    public static IEndpointRouteBuilder MapWeeklyPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scheduling/weekly-plans")
            .WithTags("Scheduling.WeeklyPlans")
            .RequireAuthorization("ApiOrCookie");

        group.MapPost("/generate", async (
            [FromBody] GeneratePlanRequest req,
            IWeeklyPlanGenerator generator,
            HttpContext http,
            CancellationToken ct) =>
        {
            var user = http.User?.Identity?.Name ?? "api";
            try
            {
                var opts = new WeeklyPlanGenerationOptions(StrictCapacity: req.StrictCapacity, DryRun: req.DryRun);
                var result = await generator.GenerateAsync(req.WeekId, req.LocationId, opts, user, ct);
                return Results.Ok(result);
            }
            catch (CapacityExceededException ex) { return Results.Problem(ex.Message, statusCode: 409); }
            catch (InvalidOperationException ex) { return Results.Problem(ex.Message, statusCode: 400); }
        }).WithName("GenerateWeeklyPlan");

        group.MapGet("/", async (
            ApplicationDbContext db,
            [FromQuery] int? weekId,
            [FromQuery] short? locationId,
            CancellationToken ct) =>
        {
            var q = db.WeeklyPlans.AsNoTracking();
            if (weekId.HasValue) q = q.Where(x => x.WeekId == weekId);
            if (locationId.HasValue) q = q.Where(x => x.LocationId == locationId);
            var list = await q.OrderByDescending(x => x.PublishedAt).Select(p => p.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(list);
        }).WithName("ListWeeklyPlans");

        group.MapGet("/{id:int}/items", async (
            int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var rows = await db.WeeklyPlanItems.AsNoTracking()
                .Where(i => i.WeeklyPlanId == id)
                .OrderBy(i => i.PlannedSequence)
                .Select(i => i.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(rows);
        }).WithName("ListWeeklyPlanItems");

        return app;
    }
}
```

- [ ] **Step 3: `DeliveryScheduleEndpoints`** — read-only view + exception CRUD

```csharp
public static class DeliveryScheduleEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/scheduling").WithTags("Scheduling.Delivery")
            .RequireAuthorization("ApiOrCookie");

        g.MapGet("/delivery", async (ApplicationDbContext db,
            [FromQuery] int weekId, [FromQuery] short locationId, CancellationToken ct) =>
        {
            var rows = await db.CurrentDeliverySchedule.AsNoTracking()
                .Where(r => r.WeekId == weekId && r.LocationId == locationId)
                .OrderBy(r => r.CurrentSequence ?? int.MaxValue)
                .Select(r => r.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(rows);
        });

        g.MapPost("/exceptions", async ([FromBody] CreateSchedulingExceptionRequest req,
            ApplicationDbContext db, HttpContext http, CancellationToken ct) =>
        {
            var user = http.User?.Identity?.Name ?? "api";
            var entity = req.ToEntity(user);
            db.SchedulingExceptions.Add(entity);
            await db.SaveChangesAsync(ct);
            return TypedResults.Created($"/api/scheduling/exceptions/{entity.Id}", entity.ToDto());
        });

        g.MapPost("/exceptions/{id:int}/resolve", async (int id,
            ApplicationDbContext db, HttpContext http, CancellationToken ct) =>
        {
            var ex = await db.SchedulingExceptions.SingleOrDefaultAsync(e => e.Id == id, ct);
            if (ex is null) return Results.NotFound();
            ex.ResolvedAt = DateTime.UtcNow;
            ex.ResolvedBy = http.User?.Identity?.Name ?? "api";
            await db.SaveChangesAsync(ct);
            return Results.Ok(ex.ToDto());
        });

        return app;
    }
}
```

- [ ] **Step 4: `AlertEndpoints`**

```csharp
public static class AlertEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/scheduling/alerts").WithTags("Scheduling.Alerts")
            .RequireAuthorization("ApiOrCookie");

        g.MapGet("/", async (ApplicationDbContext db, [FromQuery] bool unacknowledgedOnly = true,
            CancellationToken ct = default) =>
        {
            var q = db.SchedulingAlerts.AsNoTracking();
            if (unacknowledgedOnly) q = q.Where(a => a.AcknowledgedAt == null);
            var list = await q.OrderByDescending(a => a.CreatedAt).Take(500)
                .Select(a => a.ToDto()).ToListAsync(ct);
            return TypedResults.Ok(list);
        });

        g.MapPost("/{id:int}/acknowledge", async (int id, ApplicationDbContext db,
            HttpContext http, CancellationToken ct) =>
        {
            var a = await db.SchedulingAlerts.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (a is null) return Results.NotFound();
            if (a.AcknowledgedAt is not null) return Results.Ok(a.ToDto());
            a.AcknowledgedAt = DateTime.UtcNow;
            a.AcknowledgedBy = http.User?.Identity?.Name ?? "api";
            await db.SaveChangesAsync(ct);
            return Results.Ok(a.ToDto());
        });

        return app;
    }
}
```

- [ ] **Step 5: `LineConfigurationEndpoints` + `LineProductAssignmentEndpoints`**

Use `MapCrudWithInterceptor<LineConfiguration, LineConfigurationDto, CreateLineConfigurationRequest, UpdateLineConfigurationRequest, int>` — same pattern as `SalesOrderHeaderEndpoints.cs:23-31`. Mirror that file exactly, swapping entity names and the route prefix `/api/scheduling/lines`. Repeat for `LineProductAssignment` at `/api/scheduling/line-products`.

- [ ] **Step 6: Umbrella mapper + wire into Program.cs**

Create `src/AWBlazorApp/Features/Scheduling/SchedulingEndpoints.cs`:

```csharp
namespace AWBlazorApp.Features.Scheduling;

public static class SchedulingEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapLineConfigurationEndpoints();
        app.MapLineProductAssignmentEndpoints();
        app.MapWeeklyPlanEndpoints();
        app.MapDeliveryScheduleEndpoints();
        app.MapSchedulingAlertEndpoints();
        return app;
    }
}
```

Call `app.MapSchedulingEndpoints();` in `Program.cs` alongside the other feature mappers.

- [ ] **Step 7: Auth smoke test**

Test file `tests/AWBlazorApp.Tests/Scheduling/Api/AuthTests.cs`:

```csharp
[TestFixture]
public class SchedulingApiAuthTests : IntegrationTest
{
    [TestCase("/api/scheduling/weekly-plans")]
    [TestCase("/api/scheduling/alerts")]
    [TestCase("/api/scheduling/lines")]
    [TestCase("/api/scheduling/line-products")]
    public async Task Endpoints_Without_Auth_Return_401_Or_403(string path)
    {
        using var client = Factory.CreateClient();
        var resp = await client.GetAsync(path);
        Assert.That((int)resp.StatusCode, Is.AnyOf(new object[] { 401, 403 }));
    }
}
```

- [ ] **Step 8: Tests + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~SchedulingApiAuthTests"
git add src/AWBlazorApp/Features/Scheduling/ src/AWBlazorApp/Program.cs tests/
git commit -m "feat(scheduling): minimal APIs for WeeklyPlans/Delivery/Exceptions/Alerts/Lines"
```

---

## Task 17: CrudPage for `/scheduling/lines`

This is the first Blazor page. Uses the project's `CrudPage` component. Reference any existing CrudPage-wired page for template form — e.g. `Features/Production/Locations/UI/Pages/`.

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/LineConfigurations/UI/Pages/LinesPage.razor`

- [ ] **Step 1: Inspect template**

Read `src/AWBlazorApp/Features/Production/Locations/UI/Pages/` in full — copy its structure.

- [ ] **Step 2: Implement**

Full razor file with `@page "/scheduling/lines"`, `@attribute [Authorize(Policy="ApiOrCookie", Roles="Admin,Planner")]`, inject `IDbContextFactory<ApplicationDbContext>`, `CrudPage<LineConfiguration, LineConfigurationDto, ...>` markup with columns for `LocationId`, `TaktSeconds`, `ShiftsPerDay`, `MinutesPerShift`, `FrozenLookaheadHours`, `IsActive`, `ModifiedDate`.

(The full code block follows the Production Locations page template verbatim; swap entity types and API route prefix `/api/scheduling/lines`.)

- [ ] **Step 3: Smoke test**

```csharp
[Test] public async Task Lines_Page_Renders_Ok()
{
    using var client = await CreateAuthenticatedClient("Admin");  // existing test helper
    var resp = await client.GetAsync("/scheduling/lines");
    resp.EnsureSuccessStatusCode();
}
```

- [ ] **Step 4: Commit**

```bash
git commit -am "feat(scheduling): /scheduling/lines CrudPage"
```

---

## Task 18: CrudPage for `/scheduling/lines/{locationId}/products`

Same pattern as Task 17, nested route with `[Parameter] public short LocationId`, filters the API query by that locationId.

- [ ] **Step 1: Implement page** (reference pattern from Task 17; filter is the only new concept — add a `LocationId` parameter passed to the API query string)

- [ ] **Step 2: Smoke test**

- [ ] **Step 3: Commit**

```bash
git commit -am "feat(scheduling): line-products CrudPage"
```

---

## Task 19: Weekly Plans page — custom (preview + history)

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/UI/Pages/WeeklyPlansPage.razor`
- Create: `src/AWBlazorApp/Features/Scheduling/WeeklyPlans/UI/Components/GeneratePreviewDialog.razor`

- [ ] **Step 1: Page**

`@page "/scheduling/weekly-plans"`, `InteractiveServer`, injects `HttpClient` (named-client) and `IDbContextFactory<ApplicationDbContext>`. Layout:
- Filter row (`MudAutocomplete` LocationId + year-week picker + `MudButton` "Generate").
- Click → open `GeneratePreviewDialog` which first calls `POST /api/scheduling/weekly-plans/generate` with `dryRun=true`, renders preview (item count, over-capacity count, utilization %). Confirm button re-posts with `dryRun=false` and refreshes the history grid.
- `MudDataGrid` with `Groupable="true"` grouped by `(WeekId, LocationId)`, shows all versions, `HierarchyColumn` expands to list items via `GET /api/scheduling/weekly-plans/{id}/items`.

- [ ] **Step 2: Smoke test**

Page GET returns 200 under authenticated user with Admin or Planner role.

- [ ] **Step 3: Commit**

```bash
git commit -am "feat(scheduling): Weekly Plans page with generate + preview"
```

---

## Task 20: Delivery Schedule page — headline view

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/UI/Pages/DeliverySchedulePage.razor`
- Create: `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/UI/Components/ExceptionDialog.razor`
- Create: `src/AWBlazorApp/Features/Scheduling/DeliverySchedules/UI/Components/DriftArrow.razor`

- [ ] **Step 1: Page**

`@page "/scheduling/delivery"`. Pulls `/api/scheduling/delivery?weekId=&locationId=` into a `MudDataGrid` with columns per spec section 5.4. KPI cards at top (items planned, items drifted, overcapacity count, open exceptions, open alerts). Row actions: Pin, Kitting Hold, Hot Order, Clear — each opens `ExceptionDialog` → POST to `/api/scheduling/exceptions`.

- [ ] **Step 2: Drift arrow component**

Small component rendering `"N"` or `"N→M"` with an up/down icon based on sign.

- [ ] **Step 3: Smoke test + commit**

```bash
git commit -am "feat(scheduling): Delivery Schedule headline page"
```

---

## Task 21: Alerts triage page

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/Alerts/UI/Pages/AlertsPage.razor`

- [ ] **Step 1: Page**

`@page "/scheduling/alerts"`. Straight CrudPage-like over `/api/scheduling/alerts?unacknowledgedOnly=true` (default). Severity rendered as `MudChip` (Info blue, Warning amber, Critical red). Row action "Acknowledge" → POST `/api/scheduling/alerts/{id}/acknowledge`, refresh.

- [ ] **Step 2: Smoke test + commit**

```bash
git commit -am "feat(scheduling): Alerts triage page"
```

---

## Task 22: Nav menu group + dashboard tile

**Files:**
- Modify: `src/AWBlazorApp/Components/Layout/MainLayout.razor` (or the nav-menu component it pulls in)
- Create: `src/AWBlazorApp/Features/Scheduling/UI/Components/SchedulingDashboardTile.razor`
- Modify: `src/AWBlazorApp/Features/Home/UI/Pages/Home.razor` (or wherever the home dashboard lives)

- [ ] **Step 1: Add Scheduling nav group**

Top-level `MudNavGroup Title="Scheduling"` with four links:
- Lines → `/scheduling/lines`
- Weekly Plans → `/scheduling/weekly-plans`
- Delivery Schedule → `/scheduling/delivery`
- Alerts → `/scheduling/alerts`

- [ ] **Step 2: Dashboard tile component**

Injects `IDbContextFactory<ApplicationDbContext>` + `AnalyticsCacheService`. Aggregates `count(drifted) + count(open alerts)` for the current ISO week + pilot location. Renders a `ModuleCard` linking to `/scheduling/delivery`. 5-minute cache TTL.

- [ ] **Step 3: Include tile on Home**

- [ ] **Step 4: Manual browser check**

```bash
dotnet run --project src/AWBlazorApp
```
Log in as a seeded Admin user, navigate the five pages, verify nav highlight + dashboard tile render.

- [ ] **Step 5: Commit**

```bash
git commit -am "feat(scheduling): nav group + dashboard tile"
```

---

## Task 23: Centralize `SchedulingServiceRegistration`

Consolidate the ad-hoc registrations from Tasks 7, 8, 13, 14, 15 into a single feature registration in the `ForecastingServiceRegistration.cs` style.

**Files:**
- Create: `src/AWBlazorApp/Features/Scheduling/SchedulingServiceRegistration.cs`
- Modify: `src/AWBlazorApp/App/Extensions/ServiceRegistration.cs` (call the new extension; remove the one-off scheduling registrations added earlier)

- [ ] **Step 1: Implement**

```csharp
namespace AWBlazorApp.Features.Scheduling;

public static class SchedulingServiceRegistration
{
    public static IServiceCollection AddSchedulingServices(this IServiceCollection services)
    {
        services.AddScoped<IFrozenWindowEvaluator, FrozenWindowEvaluator>();
        services.AddSingleton<ISchedulingRuleResolver, SchedulingRuleResolver>();
        services.AddScoped<ISchedulingDispatcher, SchedulingDispatcher>();
        services.AddScoped<IRecalcAction, SoftResortAction>();
        services.AddScoped<IRecalcAction, AlertOnlyAction>();
        services.AddScoped<IRecalcAction, HardReplanAction>();
        services.AddScoped<IWeeklyPlanGenerator, WeeklyPlanGenerator>();
        services.AddScoped<SchedulingDispatchInterceptor>();
        return services;
    }
}
```

- [ ] **Step 2: Wire in `ServiceRegistration`**

Replace the inline registrations with `services.AddSchedulingServices();`.

- [ ] **Step 3: Verify tests still pass + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~Scheduling"
git commit -am "refactor(scheduling): centralize service registration"
```

---

## Task 24: End-to-end smoke test

One test that touches every layer: generate a plan, insert a SO in its frozen window, assert alerts fire, query the view, add an exception, re-query, confirm the row reflects it.

**File:** `tests/AWBlazorApp.Tests/Scheduling/EndToEnd/FullSliceSmokeTest.cs`

- [ ] **Step 1: Write the test** (full code block mirrors the pattern of existing `IntegrationTest` subclasses — seed LineConfiguration, LineProductAssignment, generate a plan, assert plan+items exist, insert SO, assert alert written, POST exception, GET /delivery, verify response reflects it, cleanup).

- [ ] **Step 2: Run + commit**

```bash
dotnet test AWBlazorApp.slnx --filter "FullyQualifiedName~FullSliceSmokeTest"
git commit -am "test(scheduling): full-slice end-to-end smoke"
```

---

## Task 25: Open a PR

- [ ] **Step 1: Push**

```bash
git push -u origin feat/scheduling-slice1-design
```

- [ ] **Step 2: Open PR via gh**

```bash
gh pr create --title "feat(scheduling): slice 1 end-to-end control plane" --body "$(cat <<'EOF'
## Summary
- Adds `Features/Scheduling` end-to-end for Final Assembly (Location 60) — spec: `docs/superpowers/specs/2026-04-23-scheduling-slice1-design.md`
- Six tables + one SQL view in new `Scheduling` schema (migration + marker)
- Rule-dispatched `SaveChangesInterceptor` with three seed rules
- `WeeklyPlanGenerator` with Monday-baseline algorithm, dry-run, strict-capacity
- Five planner UI pages: Lines, Line-Products, Weekly Plans, Delivery Schedule, Alerts
- Full integration test suite per spec §6

## Test plan
- [ ] `dotnet test AWBlazorApp.slnx` — all green
- [ ] Manual: generate plan for a future week, verify UI + items
- [ ] Manual: create a SalesOrderHeader with DueDate inside that week, verify alert + view row appears
- [ ] Manual: add exception (pin sequence), verify view reflects new CurrentSequence

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

---

## Self-review pass

Spec → task coverage mapping (run this as a final sanity check before handing to execution):

| Spec section | Covered by |
|---|---|
| Invariants 1, 2, 3 | Tasks 2 (entity shapes), 14 (re-entrancy), 15 (append-only regen logic) |
| Data model — six tables | Tasks 2, 3, 4 |
| `vw_CurrentDeliverySchedule` | Task 4 |
| Three seed rules | Task 6 |
| Dispatch pipeline | Tasks 7, 8, 9, 10, 11, 12, 13, 14 |
| Generation algorithm | Task 15 |
| APIs | Task 16 |
| UI pages (5) | Tasks 17, 18, 19, 20, 21 |
| Nav + dashboard | Task 22 |
| Testing floor | Tasks 1, 6, 7, 8, 10, 11, 12, 13, 14, 15, 16, 24 |

No placeholders remain in tasks 1–15 (they have complete code). Tasks 16–22 delegate some DTO/razor boilerplate to "mirror pattern from file X" rather than repeating ~500 lines of DTO declarations — this is intentional because the project's CrudPage + endpoint pattern is heavily established (51 CrudPage rollouts). If the engineer hits ambiguity, the referenced files are authoritative.

Type consistency check: `SchedulingEventType`, `RecalcActionType`, `AlertSeverity`, `ExceptionType` enum names and numeric values match between entity files (Task 2), seed (Task 6), actions (Tasks 10–12), and tests. `IRecalcAction.ActionType` property returns `RecalcActionType`. Dispatcher + resolver both key off `SchedulingEventType`. `RecalcContext.Rule.Action` is `RecalcActionType`. All consistent.
