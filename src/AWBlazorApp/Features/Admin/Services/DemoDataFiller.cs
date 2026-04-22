using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Admin.Services;

/// <summary>
/// Companion to <see cref="DemoDataSeeder"/>. Where the seeder is idempotent (one-shot
/// baseline), the filler is **additive** — each call appends fresh rows to the new module
/// transactional tables and to the org schema so the demo keeps growing.
///
/// Targets only tables that tolerate unlimited growth:
///   - Workforce: AttendanceEvent, LeaveRequest, ShiftHandoverNote, Announcement
///   - Engineering: ECO, EngineeringDocument, DeviationRequest
///   - Maintenance: MaintenanceWorkOrder (corrective), MeterReading, MaintenanceLog
///   - Performance: OeeSnapshot (next day), ProductionDailyMetric (next day)
///   - Enterprise: Station, Asset
///
/// Skips master/template tables (TrainingCourse, KpiDefinition, Scorecard, etc.) since
/// those have unique-code constraints and make less sense to grow indefinitely.
/// AdventureWorks reference data (Person, Production, Sales, etc.) is also skipped — those
/// tables ship populated and adding fake rows would mostly create FK-constraint headaches.
/// </summary>
public sealed class DemoDataFiller
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<DemoDataFiller> _logger;

    public DemoDataFiller(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<DemoDataFiller> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <summary>
    /// Adds approximately <paramref name="count"/>×N new rows across the targeted tables.
    /// Returns a per-category breakdown of what was actually inserted.
    /// </summary>
    public async Task<DemoFillResult> FillAsync(int count, CancellationToken ct)
    {
        if (count < 1) count = 1;
        if (count > 50) count = 50; // sanity cap to avoid accidental DoS via ?count=99999

        var result = new DemoFillResult();
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // Need at least one of each upstream FK target. If anything is missing, bail with
        // Skipped — the caller should run /seed-demo-data first.
        var employeeIds = await db.Employees.AsNoTracking().Select(e => e.Id).Take(20).ToListAsync(ct);
        var stationIds = await db.Stations.AsNoTracking().Select(s => s.Id).ToListAsync(ct);
        var assetIds = await db.Assets.AsNoTracking().Select(a => a.Id).ToListAsync(ct);
        var orgUnitIds = await db.OrgUnits.AsNoTracking().Select(o => o.Id).ToListAsync(ct);
        var orgIds = await db.Organizations.AsNoTracking().Select(o => o.Id).ToListAsync(ct);
        var productIds = await db.Database.SqlQueryRaw<int>("SELECT TOP 6 ProductID AS Value FROM Production.Product WHERE MakeFlag = 1 ORDER BY ProductID").ToListAsync(ct);

        if (employeeIds.Count == 0 || stationIds.Count == 0 || assetIds.Count == 0 || productIds.Count == 0 || orgUnitIds.Count == 0 || orgIds.Count == 0)
        {
            _logger.LogWarning("Filler skipped — upstream FK targets missing. Run /api/admin/seed-demo-data first.");
            result.Skipped = true;
            return result;
        }

        var rnd = new Random();

        result.Workforce = await FillWorkforceAsync(db, count, employeeIds, stationIds, rnd, ct);
        result.Engineering = await FillEngineeringAsync(db, count, productIds, rnd, ct);
        result.Maintenance = await FillMaintenanceAsync(db, count, assetIds, rnd, ct);
        result.Performance = await FillPerformanceAsync(db, count, stationIds, rnd, ct);
        result.Enterprise = await FillEnterpriseAsync(db, count, orgIds, orgUnitIds, rnd, ct);

        _logger.LogInformation("Demo fill (count={Count}): wf={Wf} eng={Eng} maint={Maint} perf={Perf} ent={Ent}",
            count, result.Workforce, result.Engineering, result.Maintenance, result.Performance, result.Enterprise);
        return result;
    }

    // ────────────────────────────────────────────────────────────────
    //  WORKFORCE — attendance, leave, handover, announcements
    // ────────────────────────────────────────────────────────────────

    private async Task<int> FillWorkforceAsync(
        ApplicationDbContext db, int count, List<int> employeeIds, List<int> stationIds, Random rnd, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = 0;

        // Attendance events — a few random employees on the next un-seeded date.
        // Using a wide forward window (1-90 days from now) keeps each call distinct without
        // colliding with seed dates in the past.
        for (var i = 0; i < count; i++)
        {
            var date = DateOnly.FromDateTime(now.AddDays(rnd.Next(-30, 31)));
            var empId = employeeIds[rnd.Next(employeeIds.Count)];
            var status = rnd.Next(10) switch { 0 => AttendanceStatus.Late, 1 => AttendanceStatus.Absent, _ => AttendanceStatus.Present };
            db.AttendanceEvents.Add(new AttendanceEvent
            {
                BusinessEntityId = empId,
                ShiftDate = date,
                ClockInAt = status == AttendanceStatus.Absent ? null : date.ToDateTime(new TimeOnly(8, rnd.Next(0, 30)), DateTimeKind.Utc),
                ClockOutAt = status == AttendanceStatus.Absent ? null : date.ToDateTime(new TimeOnly(16, rnd.Next(45, 60) % 60), DateTimeKind.Utc),
                Status = status,
                Notes = "Auto-filled.",
                ModifiedDate = now,
            });
            rows++;
        }

        // Leave requests — a couple of new pending requests.
        for (var i = 0; i < Math.Max(1, count / 3); i++)
        {
            var empId = employeeIds[rnd.Next(employeeIds.Count)];
            var startOffset = rnd.Next(7, 60);
            var duration = rnd.Next(1, 5);
            var leave = new LeaveRequest
            {
                BusinessEntityId = empId,
                LeaveType = (LeaveType)(byte)rnd.Next(1, Enum.GetValues<LeaveType>().Length + 1),
                StartDate = DateOnly.FromDateTime(now.AddDays(startOffset)),
                EndDate = DateOnly.FromDateTime(now.AddDays(startOffset + duration)),
                Status = LeaveStatus.Pending,
                Reason = "Auto-filled leave request.",
                RequestedByUserId = "demo-fill",
                RequestedAt = now.AddMinutes(-rnd.Next(0, 1440)),
                ModifiedDate = now,
            };
            db.LeaveRequests.Add(leave);
            rows++;
        }
        await db.SaveChangesAsync(ct);
        // Audit logs need IDs assigned, so capture after save.
        // For simplicity, audit logs are added per-row inline above where idiomatic; here we
        // skip individual leave audits to keep the filler fast — they're transactional fillers.

        // Handover notes — 1 per call.
        for (var i = 0; i < Math.Max(1, count / 5); i++)
        {
            db.ShiftHandoverNotes.Add(new ShiftHandoverNote
            {
                StationId = stationIds[rnd.Next(stationIds.Count)],
                ShiftDate = DateOnly.FromDateTime(now.AddDays(-rnd.Next(0, 7))),
                Note = HandoverNotes[rnd.Next(HandoverNotes.Length)],
                AuthoredByUserId = "demo-fill",
                AuthoredAt = now.AddHours(-rnd.Next(1, 12)),
                RequiresAcknowledgment = rnd.Next(3) == 0,
                ModifiedDate = now,
            });
            rows++;
        }

        // Announcement — at most 1 per call.
        if (rnd.Next(3) == 0)
        {
            var sev = (AnnouncementSeverity)(byte)rnd.Next(1, 4);
            db.Announcements.Add(new Announcement
            {
                Title = AnnouncementTitles[rnd.Next(AnnouncementTitles.Length)],
                Body = "Auto-filled announcement body. Please disregard for production communications.",
                Severity = sev,
                PublishedAt = now,
                ExpiresAt = now.AddDays(rnd.Next(3, 14)),
                AuthoredByUserId = "demo-fill",
                IsActive = true,
                ModifiedDate = now,
            });
            rows++;
        }

        await db.SaveChangesAsync(ct);
        return rows;
    }

    private static readonly string[] HandoverNotes =
    [
        "Spindle running smoothly — no anomalies.",
        "Tool change due within 2 hours, replacement is in the cabinet.",
        "Coolant level was low at start of shift — topped off.",
        "Operator from previous shift reported intermittent vibration. Monitoring.",
        "All quality checks passed today. Shift ended on schedule.",
        "Slight drift in the X axis caliper reading. Recalibrate at next break.",
    ];

    private static readonly string[] AnnouncementTitles =
    [
        "Shift schedule update for next week",
        "New training requirement for Forklift operators",
        "Holiday hours posted on the bulletin board",
        "Reminder: PPE walkthrough Friday morning",
        "Cafeteria menu change starting Monday",
        "Quarterly safety report now available",
    ];

    // ────────────────────────────────────────────────────────────────
    //  ENGINEERING — ECOs, documents, deviations
    // ────────────────────────────────────────────────────────────────

    private async Task<int> FillEngineeringAsync(
        ApplicationDbContext db, int count, List<int> productIds, Random rnd, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = 0;
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        // ECO — 1 per call.
        var eco = new EngineeringChangeOrder
        {
            Code = $"ECO-FILL-{suffix}",
            Title = "Auto-filled change request",
            Description = "Routine engineering change request added by the demo filler.",
            Status = EcoStatus.Draft,
            RaisedByUserId = "demo-fill",
            RaisedAt = now,
            ModifiedDate = now,
        };
        db.EngineeringChangeOrders.Add(eco);
        await db.SaveChangesAsync(ct);
        rows++;

        // Engineering documents — count/2 per call.
        for (var i = 0; i < Math.Max(1, count / 2); i++)
        {
            var docSuffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var doc = new EngineeringDocument
            {
                Code = $"DOC-FILL-{docSuffix}",
                Title = DocumentTitles[rnd.Next(DocumentTitles.Length)] + " " + docSuffix,
                Kind = (EngineeringDocumentKind)(byte)rnd.Next(1, 6),
                ProductId = rnd.Next(2) == 0 ? productIds[rnd.Next(productIds.Count)] : null,
                RevisionNumber = rnd.Next(1, 5),
                Url = "https://example.com/docs/" + docSuffix.ToLowerInvariant(),
                IsActive = true,
                ModifiedDate = now,
            };
            db.EngineeringDocuments.Add(doc);
            await db.SaveChangesAsync(ct);
            rows++;
        }

        // Deviation — at most 1 per call.
        if (rnd.Next(2) == 0)
        {
            var dev = new DeviationRequest
            {
                Code = $"DEV-FILL-{suffix}",
                ProductId = productIds[rnd.Next(productIds.Count)],
                Reason = "Auto-filled deviation: " + DeviationReasons[rnd.Next(DeviationReasons.Length)],
                ProposedDisposition = "Use as-is pending review.",
                AuthorizedQuantity = rnd.Next(5, 50),
                UnitMeasureCode = "EA",
                ValidFrom = DateOnly.FromDateTime(now),
                ValidTo = DateOnly.FromDateTime(now.AddDays(rnd.Next(7, 30))),
                Status = DeviationStatus.Pending,
                RaisedByUserId = "demo-fill",
                RaisedAt = now,
                ModifiedDate = now,
            };
            db.DeviationRequests.Add(dev);
            await db.SaveChangesAsync(ct);
            rows++;
        }

        await db.SaveChangesAsync(ct);
        return rows;
    }

    private static readonly string[] DocumentTitles =
    [
        "Component specification",
        "Assembly instructions",
        "Inspection procedure",
        "Cad model revision",
        "Material certificate",
        "Test method",
    ];

    private static readonly string[] DeviationReasons =
    [
        "minor cosmetic variance, customer accepted",
        "trueness slightly out of nominal but within tolerance band",
        "supplier shipped alternate part — engineering review pending",
        "labelling typo, batch already shipped",
    ];

    // ────────────────────────────────────────────────────────────────
    //  MAINTENANCE — work orders, meter readings, log entries
    // ────────────────────────────────────────────────────────────────

    private async Task<int> FillMaintenanceAsync(
        ApplicationDbContext db, int count, List<int> assetIds, Random rnd, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = 0;

        // Corrective work orders — count/2 per call.
        for (var i = 0; i < Math.Max(1, count / 2); i++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var wo = new MaintenanceWorkOrder
            {
                WorkOrderNumber = $"WO-FILL-{suffix}",
                Title = WoTitles[rnd.Next(WoTitles.Length)],
                AssetId = assetIds[rnd.Next(assetIds.Count)],
                Type = WorkOrderType.Corrective,
                Status = WorkOrderStatus.Draft,
                Priority = (WorkOrderPriority)(byte)rnd.Next(1, 5),
                RaisedByUserId = "demo-fill",
                RaisedAt = now,
                ModifiedDate = now,
            };
            db.MaintenanceWorkOrders.Add(wo);
            await db.SaveChangesAsync(ct);
            rows++;
        }

        // Meter readings — count per call, advancing the value for each asset.
        foreach (var assetId in assetIds.Take(Math.Min(assetIds.Count, count)))
        {
            // Pick up where we left off for this asset (RuntimeHours kind).
            var latest = await db.MeterReadings.AsNoTracking()
                .Where(m => m.AssetId == assetId && m.Kind == MeterKind.RuntimeHours)
                .OrderByDescending(m => m.RecordedAt)
                .Select(m => (decimal?)m.Value).FirstOrDefaultAsync(ct);
            var nextValue = (latest ?? 1000m) + rnd.Next(8, 20);
            db.MeterReadings.Add(new MeterReading
            {
                AssetId = assetId,
                Kind = MeterKind.RuntimeHours,
                Value = nextValue,
                RecordedAt = now,
                RecordedByUserId = "demo-fill",
                Notes = "Auto-filled progression.",
                ModifiedDate = now,
            });
            rows++;
        }

        // Maintenance log entries — count/3 per call.
        for (var i = 0; i < Math.Max(1, count / 3); i++)
        {
            db.MaintenanceLogs.Add(new MaintenanceLog
            {
                AssetId = assetIds[rnd.Next(assetIds.Count)],
                Kind = (MaintenanceLogKind)(byte)rnd.Next(1, 5),
                Note = LogNotes[rnd.Next(LogNotes.Length)],
                AuthoredByUserId = "demo-fill",
                AuthoredAt = now.AddMinutes(-rnd.Next(0, 1440)),
                ModifiedDate = now,
            });
            rows++;
        }

        await db.SaveChangesAsync(ct);
        return rows;
    }

    private static readonly string[] WoTitles =
    [
        "Investigate intermittent stop",
        "Replace worn coupling",
        "Recalibrate axis encoders",
        "Address coolant leak",
        "Inspect drive belt tension",
        "Replace consumables kit",
    ];

    private static readonly string[] LogNotes =
    [
        "Operator notes minor noise during cycle start.",
        "Visual inspection clean — no oil leaks observed.",
        "Filter restriction sensor at threshold; replace at next PM.",
        "Coolant pH slightly low; topped up additive.",
        "Cycle counter advanced normally during shift.",
    ];

    // ────────────────────────────────────────────────────────────────
    //  PERFORMANCE — extend OEE + production-metric series by one day
    // ────────────────────────────────────────────────────────────────

    private async Task<int> FillPerformanceAsync(
        ApplicationDbContext db, int count, List<int> stationIds, Random rnd, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = 0;
        var stationId = stationIds[0];

        // Find the latest seeded OEE day for this station and add the next day.
        var latestOee = await db.OeeSnapshots.AsNoTracking()
            .Where(s => s.StationId == stationId && s.PeriodKind == PerformancePeriodKind.Day)
            .OrderByDescending(s => s.PeriodStart)
            .Select(s => (DateTime?)s.PeriodStart).FirstOrDefaultAsync(ct);

        var nextDay = (latestOee ?? DateTime.SpecifyKind(now.Date.AddDays(-1), DateTimeKind.Utc)).AddDays(1);

        if (!await db.OeeSnapshots.AnyAsync(s => s.StationId == stationId && s.PeriodKind == PerformancePeriodKind.Day && s.PeriodStart == nextDay, ct))
        {
            var avail = 0.78m + (decimal)rnd.NextDouble() * 0.18m;
            var perf = 0.70m + (decimal)rnd.NextDouble() * 0.25m;
            var qual = 0.88m + (decimal)rnd.NextDouble() * 0.11m;
            db.OeeSnapshots.Add(new OeeSnapshot
            {
                StationId = stationId,
                PeriodKind = PerformancePeriodKind.Day,
                PeriodStart = nextDay,
                PeriodEnd = nextDay.AddDays(1),
                PlannedRuntimeMinutes = 480m,
                ActualRuntimeMinutes = 480m * avail,
                DowntimeMinutes = 480m * (1m - avail),
                UnitsProduced = 200m + rnd.Next(-30, 40),
                UnitsScrapped = rnd.Next(0, 10),
                IdealCycleSeconds = 60m,
                Availability = Math.Round(avail, 4),
                Performance = Math.Round(perf, 4),
                Quality = Math.Round(qual, 4),
                Oee = Math.Round(avail * perf * qual, 4),
                ComputedAt = now,
                ModifiedDate = now,
            });
            rows++;
        }

        // Same for production-daily metric.
        var nextDate = DateOnly.FromDateTime(nextDay);
        if (!await db.ProductionDailyMetrics.AnyAsync(m => m.StationId == stationId && m.Date == nextDate, ct))
        {
            db.ProductionDailyMetrics.Add(new ProductionDailyMetric
            {
                StationId = stationId,
                Date = nextDate,
                UnitsProduced = 200m + rnd.Next(-30, 40),
                UnitsScrapped = rnd.Next(0, 10),
                AverageCycleSeconds = 60m + (decimal)(rnd.NextDouble() * 10 - 5),
                YieldFraction = Math.Round(0.93m + (decimal)rnd.NextDouble() * 0.06m, 4),
                RunCount = rnd.Next(2, 5),
                ComputedAt = now,
                ModifiedDate = now,
            });
            rows++;
        }

        await db.SaveChangesAsync(ct);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  ENTERPRISE — additional stations + assets
    // ────────────────────────────────────────────────────────────────

    private async Task<int> FillEnterpriseAsync(
        ApplicationDbContext db, int count, List<int> orgIds, List<int> orgUnitIds, Random rnd, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = 0;
        var orgId = orgIds[0];
        var orgUnitId = orgUnitIds[0];
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        // 1 station per call.
        var stationKind = (StationKind)(byte)rnd.Next(1, Enum.GetValues<StationKind>().Length + 1);
        db.Stations.Add(new Station
        {
            OrgUnitId = orgUnitId,
            Code = $"ST-FILL-{suffix}",
            Name = $"{stationKind} Station {suffix}",
            StationKind = stationKind,
            IsActive = true,
            ModifiedDate = now,
        });
        rows++;

        // 1 asset per call (when count >= 2).
        if (count >= 2)
        {
            var assetType = (AssetType)(byte)(rnd.Next(1, 7));
            var assetSuffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            db.Assets.Add(new Asset
            {
                OrganizationId = orgId,
                OrgUnitId = orgUnitId,
                AssetTag = $"AST-FILL-{assetSuffix}",
                Name = $"Filler {assetType} {assetSuffix}",
                AssetType = assetType,
                Status = AssetStatus.Active,
                ModifiedDate = now,
            });
            rows++;
        }

        await db.SaveChangesAsync(ct);
        return rows;
    }
}

public sealed class DemoFillResult
{
    public bool Skipped { get; set; }
    public int Workforce { get; set; }
    public int Engineering { get; set; }
    public int Maintenance { get; set; }
    public int Performance { get; set; }
    public int Enterprise { get; set; }
    public int Total => Workforce + Engineering + Maintenance + Performance + Enterprise;
}
