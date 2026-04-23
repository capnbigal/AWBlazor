using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Logistics.Receipts.Domain; using AWBlazorApp.Features.Logistics.Shipments.Domain; using AWBlazorApp.Features.Logistics.Transfers.Domain; 
using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Admin.Services;

/// <summary>
/// Seeds representative demo data across the M6 / M7 / M8 / M9 modules so the droplet has
/// something to show on a fresh deploy. Each module's seed is idempotent — checks for a
/// marker row (seeded Code starting with <c>DEMO-</c>) and no-ops if present. Callers can
/// re-run the whole seed or any individual module without duplicating data.
/// </summary>
public sealed class DemoDataSeeder
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<DemoDataSeeder> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<DemoSeedResult> SeedAllAsync(CancellationToken cancellationToken)
    {
        var result = new DemoSeedResult();

        // The wf/eng/maint/perf seeds all FK back to org.Station / org.Asset and to AW
        // Production.Product. AW restores ship Product populated, but the org schema starts
        // empty on a fresh deploy (Phase A creates the tables, doesn't seed them). Bootstrap
        // a minimum baseline if any of those upstream rows are missing — counts get rolled
        // into result.Baseline so the caller can see what was created.
        result.Baseline = await EnsureBaselineAsync(cancellationToken);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var employeeIds = await db.Employees.AsNoTracking().OrderBy(e => e.Id).Take(12).Select(e => e.Id).ToListAsync(cancellationToken);
        var stationIds = await db.Stations.AsNoTracking().OrderBy(s => s.Id).Take(6).Select(s => s.Id).ToListAsync(cancellationToken);
        var assetIds = await db.Assets.AsNoTracking().OrderBy(a => a.Id).Take(6).Select(a => a.Id).ToListAsync(cancellationToken);
        var productIds = await db.Database.SqlQueryRaw<int>("SELECT TOP 6 ProductID AS Value FROM Production.Product WHERE MakeFlag = 1 ORDER BY ProductID").ToListAsync(cancellationToken);

        if (employeeIds.Count == 0 || stationIds.Count == 0 || assetIds.Count == 0 || productIds.Count == 0)
        {
            _logger.LogWarning("Demo seed skipped — upstream FK targets missing even after baseline. Employees={E} Stations={S} Assets={A} Products={P}",
                employeeIds.Count, stationIds.Count, assetIds.Count, productIds.Count);
            result.Skipped = true;
            return result;
        }

        result.Workforce = await SeedWorkforceAsync(employeeIds, stationIds, cancellationToken);
        result.Engineering = await SeedEngineeringAsync(productIds, stationIds, cancellationToken);
        result.Maintenance = await SeedMaintenanceAsync(assetIds, productIds, cancellationToken);
        result.Performance = await SeedPerformanceAsync(stationIds, assetIds, cancellationToken);
        result.Inventory = await SeedInventoryAsync(productIds, cancellationToken);
        result.Logistics = await SeedLogisticsAsync(cancellationToken);
        result.Quality = await SeedQualityAsync(productIds, employeeIds, cancellationToken);
        result.Mes = await SeedMesAsync(stationIds, cancellationToken);

        _logger.LogInformation("Demo seed complete: baseline={Base} wf={Wf} eng={Eng} maint={Maint} perf={Perf} inv={Inv} lgx={Lgx} qa={Qa} mes={Mes}",
            result.Baseline, result.Workforce, result.Engineering, result.Maintenance, result.Performance, result.Inventory, result.Logistics, result.Quality, result.Mes);
        return result;
    }

    // ────────────────────────────────────────────────────────────────
    //  BASELINE — seed minimum org/orgunit/station/asset if empty
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a minimum org hierarchy + 3 stations + 5 assets when those tables are empty,
    /// so the wf/eng/maint/perf seeds have FK targets. Returns the count of rows created.
    /// Idempotent — checks for the DEMO-PLANT marker organization and skips if present.
    /// </summary>
    private async Task<int> EnsureBaselineAsync(CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var rows = 0;
        var now = DateTime.UtcNow;

        // Already bootstrapped?
        if (await db.Organizations.AnyAsync(o => o.Code == "DEMO-PLANT", ct))
        {
            _logger.LogInformation("Baseline org/station/asset already present, skipping bootstrap.");
            return 0;
        }

        // 1. Organization (only if no organizations exist at all — preserve any real one).
        var org = await db.Organizations.FirstOrDefaultAsync(o => o.IsPrimary, ct);
        if (org is null)
        {
            org = new Organization
            {
                Code = "DEMO-PLANT",
                Name = "Demo Manufacturing Plant",
                IsPrimary = !await db.Organizations.AnyAsync(o => o.IsPrimary, ct),
                IsActive = true,
                ModifiedDate = now,
            };
            db.Organizations.Add(org);
            await db.SaveChangesAsync(ct);
            rows++;
            _logger.LogInformation("Bootstrapped Organization {Code} (Id={Id}).", org.Code, org.Id);
        }

        // 2. OrgUnit (Plant -> Area). Reuse if any exist for this org.
        var orgUnit = await db.OrgUnits.FirstOrDefaultAsync(u => u.OrganizationId == org.Id, ct);
        if (orgUnit is null)
        {
            var plant = new OrgUnit
            {
                OrganizationId = org.Id,
                Kind = OrgUnitKind.Plant,
                Code = "DEMO-PLANT-A",
                Name = "Plant A",
                Path = "DEMO-PLANT-A",
                Depth = 0,
                IsActive = true,
                ModifiedDate = now,
            };
            db.OrgUnits.Add(plant);
            await db.SaveChangesAsync(ct);
            rows++;

            orgUnit = new OrgUnit
            {
                OrganizationId = org.Id,
                ParentOrgUnitId = plant.Id,
                Kind = OrgUnitKind.Area,
                Code = "DEMO-AREA-MAIN",
                Name = "Main Production Area",
                Path = "DEMO-PLANT-A/DEMO-AREA-MAIN",
                Depth = 1,
                IsActive = true,
                ModifiedDate = now,
            };
            db.OrgUnits.Add(orgUnit);
            await db.SaveChangesAsync(ct);
            rows++;
            _logger.LogInformation("Bootstrapped 2 OrgUnits under Organization {Id}.", org.Id);
        }

        // 3. Stations — only seed if NONE exist anywhere (preserve real installations).
        if (!await db.Stations.AnyAsync(ct))
        {
            var stations = new[]
            {
                new Station { OrgUnitId = orgUnit.Id, Code = "DEMO-ST-01", Name = "Assembly Station 1", StationKind = StationKind.Assembly, IsActive = true, ModifiedDate = now },
                new Station { OrgUnitId = orgUnit.Id, Code = "DEMO-ST-02", Name = "Assembly Station 2", StationKind = StationKind.Assembly, IsActive = true, ModifiedDate = now },
                new Station { OrgUnitId = orgUnit.Id, Code = "DEMO-ST-03", Name = "Inspection Bench", StationKind = StationKind.Inspection, IsActive = true, ModifiedDate = now },
            };
            db.Stations.AddRange(stations);
            await db.SaveChangesAsync(ct);
            rows += stations.Length;
            _logger.LogInformation("Bootstrapped {Count} stations under OrgUnit {Id}.", stations.Length, orgUnit.Id);
        }

        // 4. Assets — only seed if NONE exist anywhere.
        if (!await db.Assets.AnyAsync(ct))
        {
            var assets = new[]
            {
                new Asset { OrganizationId = org.Id, OrgUnitId = orgUnit.Id, AssetTag = "DEMO-AST-CNC01", Name = "CNC Mill #1", Manufacturer = "Haas", Model = "VF-2", AssetType = AssetType.Machine, Status = AssetStatus.Active, ModifiedDate = now },
                new Asset { OrganizationId = org.Id, OrgUnitId = orgUnit.Id, AssetTag = "DEMO-AST-LATHE01", Name = "Lathe #1", Manufacturer = "Mazak", Model = "QT-200", AssetType = AssetType.Machine, Status = AssetStatus.Active, ModifiedDate = now },
                new Asset { OrganizationId = org.Id, OrgUnitId = orgUnit.Id, AssetTag = "DEMO-AST-PRESS01", Name = "Hydraulic Press", Manufacturer = "Schuler", AssetType = AssetType.Machine, Status = AssetStatus.Active, ModifiedDate = now },
                new Asset { OrganizationId = org.Id, OrgUnitId = orgUnit.Id, AssetTag = "DEMO-AST-FORK01", Name = "Forklift", Manufacturer = "Toyota", Model = "8FGU25", AssetType = AssetType.Vehicle, Status = AssetStatus.Active, ModifiedDate = now },
                new Asset { OrganizationId = org.Id, OrgUnitId = orgUnit.Id, AssetTag = "DEMO-AST-CMM01", Name = "Coordinate Measuring Machine", Manufacturer = "Zeiss", AssetType = AssetType.Instrument, Status = AssetStatus.Active, ModifiedDate = now },
            };
            db.Assets.AddRange(assets);
            await db.SaveChangesAsync(ct);
            rows += assets.Length;
            _logger.LogInformation("Bootstrapped {Count} assets under OrgUnit {Id}.", assets.Length, orgUnit.Id);
        }

        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  WORKFORCE
    // ────────────────────────────────────────────────────────────────

    public async Task<int> SeedWorkforceAsync(List<int> employeeIds, List<int> stationIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var seededRows = 0;

        // Marker: DEMO-FORKLIFT training course. The heavy block below is idempotent at the
        // module-marker level — but training records are seeded by the per-call EnsureDemo...
        // helper at the end so existing dev DBs (where the marker already exists) still get
        // training records backfilled into the wf.TrainingRecord table.
        if (await db.TrainingCourses.AnyAsync(c => c.Code == "DEMO-FORKLIFT", ct))
        {
            _logger.LogInformation("Workforce demo seed: heavy block already present, only backfilling training records.");
            seededRows += await EnsureDemoTrainingRecordsAsync(db, employeeIds, ct);
            return seededRows;
        }

        var now = DateTime.UtcNow;
        var rows = 0;

        var courses = new[]
        {
            new TrainingCourse { Code = "DEMO-FORKLIFT", Name = "Forklift Operator Refresher", Description = "Annual forklift safety + operations refresher.", DurationMinutes = 240, RecurrenceMonths = 12, IsActive = true, ModifiedDate = now },
            new TrainingCourse { Code = "DEMO-WELD-TIG", Name = "TIG Welding Certification", DurationMinutes = 480, RecurrenceMonths = 24, IsActive = true, ModifiedDate = now },
            new TrainingCourse { Code = "DEMO-SAFETY", Name = "Plant Safety Orientation", DurationMinutes = 120, IsActive = true, ModifiedDate = now },
            new TrainingCourse { Code = "DEMO-FIRSTAID", Name = "First Aid / CPR", DurationMinutes = 180, RecurrenceMonths = 24, IsActive = true, ModifiedDate = now },
            new TrainingCourse { Code = "DEMO-ISO9001", Name = "ISO 9001 Awareness", DurationMinutes = 90, IsActive = true, ModifiedDate = now },
        };
        db.TrainingCourses.AddRange(courses);
        await db.SaveChangesAsync(ct);
        rows += courses.Length;

        var quals = new[]
        {
            new Qualification { Code = "DEMO-Q-FORKLIFT", Name = "Forklift Operator", Category = QualificationCategory.Certification, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-TIG", Name = "TIG Welder", Category = QualificationCategory.Skill, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-CNC", Name = "CNC Operator", Category = QualificationCategory.Skill, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-QA", Name = "Quality Inspector", Category = QualificationCategory.Certification, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-ELEC", Name = "Licensed Electrician", Category = QualificationCategory.Compliance, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-SAFETY", Name = "Safety Officer", Category = QualificationCategory.Safety, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-LEAD", Name = "Team Lead", Category = QualificationCategory.Skill, IsActive = true, ModifiedDate = now },
            new Qualification { Code = "DEMO-Q-FIRSTRESP", Name = "First Responder", Category = QualificationCategory.Safety, IsActive = true, ModifiedDate = now },
        };
        db.Qualifications.AddRange(quals);
        await db.SaveChangesAsync(ct);
        rows += quals.Length;

        var rnd = new Random(42);
        for (var i = 0; i < Math.Min(employeeIds.Count, 12); i++)
        {
            var qual = quals[rnd.Next(quals.Length)];
            var earned = now.AddMonths(-rnd.Next(1, 30));
            DateTime? expires = rnd.Next(3) switch
            {
                0 => null,
                1 => now.AddDays(rnd.Next(10, 25)),   // soon-expiring
                _ => now.AddMonths(rnd.Next(3, 24)),  // current
            };
            var eq = new EmployeeQualification
            {
                BusinessEntityId = employeeIds[i],
                QualificationId = qual.Id,
                EarnedDate = earned,
                ExpiresOn = expires,
                EvidenceUrl = "https://example.com/cert/" + Guid.NewGuid().ToString("N")[..8],
                Notes = "Seeded demo qualification.",
                ModifiedDate = now,
            };
            db.EmployeeQualifications.Add(eq);
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Station requirements (5 per-station quals).
        foreach (var sId in stationIds.Take(3))
        {
            foreach (var q in quals.Where(q => q.Code is "DEMO-Q-CNC" or "DEMO-Q-SAFETY").Take(2))
            {
                db.StationQualifications.Add(new StationQualification
                {
                    StationId = sId,
                    QualificationId = q.Id,
                    IsRequired = true,
                    ModifiedDate = now,
                });
                rows++;
            }
        }
        await db.SaveChangesAsync(ct);

        // Open qualification alerts (2 Open, 1 Acknowledged) — use first station + first two employees.
        var missingQual = quals.First(q => q.Code == "DEMO-Q-FORKLIFT");
        foreach (var (empId, status) in new[]
        {
            (employeeIds[0], QualificationAlertStatus.Open),
            (employeeIds[1], QualificationAlertStatus.Open),
            (employeeIds[2], QualificationAlertStatus.Acknowledged),
        })
        {
            db.QualificationAlerts.Add(new QualificationAlert
            {
                BusinessEntityId = empId,
                StationId = stationIds[0],
                QualificationId = missingQual.Id,
                Reason = QualificationAlertReason.Missing,
                Status = status,
                RaisedAt = now.AddHours(-rnd.Next(2, 48)),
                AcknowledgedAt = status == QualificationAlertStatus.Acknowledged ? now.AddHours(-1) : null,
                ModifiedDate = now,
            });
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Attendance events — last 5 days, random statuses for first 4 employees.
        for (var d = 1; d <= 5; d++)
        {
            var date = DateOnly.FromDateTime(now.AddDays(-d));
            foreach (var empId in employeeIds.Take(4))
            {
                var status = rnd.Next(10) switch { 0 => AttendanceStatus.Late, 1 => AttendanceStatus.Absent, _ => AttendanceStatus.Present };
                db.AttendanceEvents.Add(new AttendanceEvent
                {
                    BusinessEntityId = empId,
                    ShiftDate = date,
                    ClockInAt = status == AttendanceStatus.Absent ? null : date.ToDateTime(new TimeOnly(8, rnd.Next(-5, 15)), DateTimeKind.Utc),
                    ClockOutAt = status == AttendanceStatus.Absent ? null : date.ToDateTime(new TimeOnly(16, rnd.Next(55, 70) % 60), DateTimeKind.Utc),
                    Status = status,
                    ModifiedDate = now,
                });
                rows++;
            }
        }
        await db.SaveChangesAsync(ct);

        // Leave requests.
        var leaves = new[]
        {
            new LeaveRequest { BusinessEntityId = employeeIds[0], LeaveType = LeaveType.Vacation, StartDate = DateOnly.FromDateTime(now.AddDays(14)), EndDate = DateOnly.FromDateTime(now.AddDays(18)), Status = LeaveStatus.Pending, Reason = "Family trip", RequestedByUserId = "demo-seed", RequestedAt = now.AddDays(-2), ModifiedDate = now },
            new LeaveRequest { BusinessEntityId = employeeIds[1], LeaveType = LeaveType.Sick, StartDate = DateOnly.FromDateTime(now.AddDays(-1)), EndDate = DateOnly.FromDateTime(now.AddDays(-1)), Status = LeaveStatus.Approved, RequestedByUserId = "demo-seed", RequestedAt = now.AddDays(-1), ReviewedByUserId = "demo-seed", ReviewedAt = now.AddHours(-12), ReviewNotes = "Approved.", ModifiedDate = now },
            new LeaveRequest { BusinessEntityId = employeeIds[2], LeaveType = LeaveType.Personal, StartDate = DateOnly.FromDateTime(now.AddDays(30)), EndDate = DateOnly.FromDateTime(now.AddDays(32)), Status = LeaveStatus.Pending, Reason = "Wedding", RequestedByUserId = "demo-seed", RequestedAt = now.AddDays(-3), ModifiedDate = now },
        };
        db.LeaveRequests.AddRange(leaves);
        await db.SaveChangesAsync(ct);
        rows += leaves.Length;

        // Shift handover notes.
        for (var i = 0; i < 3; i++)
        {
            db.ShiftHandoverNotes.Add(new ShiftHandoverNote
            {
                StationId = stationIds[i % stationIds.Count],
                ShiftDate = DateOnly.FromDateTime(now.AddDays(-i)),
                Note = i switch
                {
                    0 => "Coolant level low on spindle — topped off, flag for follow-up next shift.",
                    1 => "Tool #12 showing wear. Replaced at 14:30. Calibration OK.",
                    _ => "Shift handover smooth. No issues to report.",
                },
                AuthoredByUserId = "demo-seed",
                AuthoredAt = now.AddDays(-i).AddHours(-1),
                RequiresAcknowledgment = i == 1,
                ModifiedDate = now,
            });
            rows++;
        }

        // Announcements.
        var announcements = new[]
        {
            new Announcement { Title = "All-hands meeting this Friday", Body = "Join us at 2pm in the main conference room for the quarterly all-hands.", Severity = AnnouncementSeverity.Info, PublishedAt = now.AddDays(-1), ExpiresAt = now.AddDays(5), AuthoredByUserId = "demo-seed", IsActive = true, ModifiedDate = now },
            new Announcement { Title = "Safety walk-through Monday morning", Body = "Plant management will be conducting a safety walk-through. Please ensure PPE is worn at all times.", Severity = AnnouncementSeverity.Important, PublishedAt = now.AddHours(-6), ExpiresAt = now.AddDays(3), AuthoredByUserId = "demo-seed", IsActive = true, ModifiedDate = now },
            new Announcement { Title = "Emergency drill scheduled", Body = "Fire drill at 10:30 AM Thursday. Proceed to the designated assembly point.", Severity = AnnouncementSeverity.Critical, PublishedAt = now, ExpiresAt = now.AddDays(4), AuthoredByUserId = "demo-seed", IsActive = true, ModifiedDate = now },
        };
        db.Announcements.AddRange(announcements);
        await db.SaveChangesAsync(ct);
        rows += announcements.Length;

        await db.SaveChangesAsync(ct);

        // Backfill training records via the per-call helper (newly added in this PR — even on
        // a fresh-seed path it lands rows here so the count is reflected in the return value).
        rows += await EnsureDemoTrainingRecordsAsync(db, employeeIds, ct);

        _logger.LogInformation("Seeded {Rows} workforce demo rows.", rows);
        return rows;
    }

    /// <summary>
    /// Per-call helper that adds 8 training-record completions if none exist yet (keyed off
    /// <c>RecordedByUserId == "demo-seed"</c>). Lets existing dev DBs that were seeded before
    /// this loop existed get backfilled without re-running the heavy SeedWorkforceAsync block.
    /// </summary>
    private async Task<int> EnsureDemoTrainingRecordsAsync(ApplicationDbContext db, List<int> employeeIds, CancellationToken ct)
    {
        if (await db.TrainingRecords.AnyAsync(t => t.RecordedByUserId == "demo-seed", ct))
            return 0;

        // Pull the seeded courses we created earlier in this same module.
        var demoCourses = await db.TrainingCourses
            .Where(c => c.Code.StartsWith("DEMO-"))
            .Select(c => new { c.Id, c.RecurrenceMonths })
            .ToListAsync(ct);
        if (demoCourses.Count == 0 || employeeIds.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var rnd = new Random(99);
        var rows = 0;
        for (var i = 0; i < Math.Min(employeeIds.Count, 8); i++)
        {
            var course = demoCourses[rnd.Next(demoCourses.Count)];
            var completed = now.AddDays(-rnd.Next(30, 540));
            var expires = course.RecurrenceMonths.HasValue
                ? completed.AddMonths(course.RecurrenceMonths.Value)
                : (DateTime?)null;
            db.TrainingRecords.Add(new TrainingRecord
            {
                TrainingCourseId = course.Id,
                BusinessEntityId = employeeIds[i],
                CompletedAt = completed,
                ExpiresOn = expires,
                Score = rnd.Next(80, 101) + "%",
                EvidenceUrl = "https://example.com/training/" + Guid.NewGuid().ToString("N")[..8],
                Notes = "Seeded demo training completion.",
                RecordedByUserId = "demo-seed",
                ModifiedDate = now,
            });
            rows++;
        }
        await db.SaveChangesAsync(ct);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  ENGINEERING
    // ────────────────────────────────────────────────────────────────

    public async Task<int> SeedEngineeringAsync(List<int> productIds, List<int> stationIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.ManufacturingRoutings.AnyAsync(r => r.Code == "DEMO-RT-ROAD", ct))
        {
            _logger.LogInformation("Engineering demo seed: already present, skipping.");
            return 0;
        }

        var now = DateTime.UtcNow;
        var rows = 0;

        var routings = new[]
        {
            new ManufacturingRouting { Code = "DEMO-RT-ROAD", Name = "Road Bike Assembly v1", Description = "Standard road-bike assembly routing.", ProductId = productIds[0], RevisionNumber = 1, IsActive = true, ModifiedDate = now },
            new ManufacturingRouting { Code = "DEMO-RT-MTB", Name = "Mountain Bike Assembly v1", Description = "Mountain-bike assembly routing.", ProductId = productIds.ElementAtOrDefault(1), RevisionNumber = 1, IsActive = true, ModifiedDate = now },
        };
        db.ManufacturingRoutings.AddRange(routings);
        await db.SaveChangesAsync(ct);
        rows += routings.Length;

        var stationCount = stationIds.Count;
        foreach (var routing in routings)
        {
            var steps = new[]
            {
                new RoutingStep { ManufacturingRoutingId = routing.Id, SequenceNumber = 10, OperationName = "Frame inspection", StationId = stationIds[0 % stationCount], StandardMinutes = 5m, ModifiedDate = now },
                new RoutingStep { ManufacturingRoutingId = routing.Id, SequenceNumber = 20, OperationName = "Install headset + fork", StationId = stationIds[1 % stationCount], StandardMinutes = 12m, ModifiedDate = now },
                new RoutingStep { ManufacturingRoutingId = routing.Id, SequenceNumber = 30, OperationName = "Install drivetrain", StationId = stationIds[2 % stationCount], StandardMinutes = 20m, ModifiedDate = now },
                new RoutingStep { ManufacturingRoutingId = routing.Id, SequenceNumber = 40, OperationName = "Install wheels + brakes", StationId = stationIds[1 % stationCount], StandardMinutes = 15m, ModifiedDate = now },
                new RoutingStep { ManufacturingRoutingId = routing.Id, SequenceNumber = 50, OperationName = "Final QA + test ride", StationId = stationIds[0 % stationCount], StandardMinutes = 8m, ModifiedDate = now },
            };
            db.RoutingSteps.AddRange(steps);
            rows += steps.Length;
        }
        await db.SaveChangesAsync(ct);

        // BOMs.
        var boms = new[]
        {
            new BomHeader { Code = "DEMO-BOM-ROAD-V1", Name = "Road Bike BOM v1", ProductId = productIds[0], RevisionNumber = 1, IsActive = true, ModifiedDate = now },
            new BomHeader { Code = "DEMO-BOM-ROAD-V2", Name = "Road Bike BOM v2 (pending ECO)", ProductId = productIds[0], RevisionNumber = 2, IsActive = false, ModifiedDate = now },
        };
        db.BomHeaders.AddRange(boms);
        await db.SaveChangesAsync(ct);
        rows += boms.Length;

        foreach (var bom in boms)
        {
            for (var i = 0; i < Math.Min(5, productIds.Count); i++)
            {
                db.BomLines.Add(new BomLine
                {
                    BomHeaderId = bom.Id,
                    ComponentProductId = productIds[i],
                    Quantity = 1m + i,
                    UnitMeasureCode = "EA",
                    ScrapPercentage = 0.02m,
                    Notes = $"Component {i + 1}",
                    ModifiedDate = now,
                });
                rows++;
            }
        }
        await db.SaveChangesAsync(ct);

        // ECOs — one in each of the key workflow states.
        var draftEco = new EngineeringChangeOrder { Code = "DEMO-ECO-001", Title = "Upgrade saddle material", Description = "Switch from synthetic to leather.", Status = EcoStatus.Draft, RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-3), ModifiedDate = now };
        var reviewEco = new EngineeringChangeOrder { Code = "DEMO-ECO-002", Title = "Revise road bike BOM", Description = "New lighter crankset component.", Status = EcoStatus.UnderReview, RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-5), SubmittedAt = now.AddDays(-2), ModifiedDate = now };
        db.EngineeringChangeOrders.AddRange(draftEco, reviewEco);
        await db.SaveChangesAsync(ct);
        rows += 2;

        db.EcoAffectedItems.Add(new EcoAffectedItem
        {
            EngineeringChangeOrderId = reviewEco.Id,
            AffectedKind = EcoAffectedKind.Bom,
            TargetId = boms[1].Id,
            Notes = "Activate new BOM revision on approval.",
            ModifiedDate = now,
        });
        rows++;

        // Engineering docs.
        var docs = new[]
        {
            new EngineeringDocument { Code = "DEMO-DRW-ROAD-01", Title = "Road bike frame drawing", Kind = EngineeringDocumentKind.Drawing, ProductId = productIds[0], RevisionNumber = 2, Url = "https://example.com/drawings/road-01.pdf", IsActive = true, ModifiedDate = now },
            new EngineeringDocument { Code = "DEMO-SPEC-BRAKE", Title = "Brake pad specification", Kind = EngineeringDocumentKind.Specification, RevisionNumber = 1, Url = "https://example.com/specs/brake-pad.pdf", IsActive = true, ModifiedDate = now },
            new EngineeringDocument { Code = "DEMO-CAD-FORK", Title = "Front fork CAD model", Kind = EngineeringDocumentKind.CadModel, RevisionNumber = 3, Url = "https://example.com/cad/fork.step", IsActive = true, ModifiedDate = now },
            new EngineeringDocument { Code = "DEMO-PROC-QA", Title = "QA inspection procedure", Kind = EngineeringDocumentKind.Procedure, RevisionNumber = 1, Url = "https://example.com/procedures/qa-insp.pdf", IsActive = true, ModifiedDate = now },
            new EngineeringDocument { Code = "DEMO-DRW-MTB-01", Title = "Mountain bike frame drawing", Kind = EngineeringDocumentKind.Drawing, ProductId = productIds.ElementAtOrDefault(1), RevisionNumber = 1, Url = "https://example.com/drawings/mtb-01.pdf", IsActive = true, ModifiedDate = now },
        };
        db.EngineeringDocuments.AddRange(docs);
        await db.SaveChangesAsync(ct);
        rows += docs.Length;

        // Deviations.
        var deviations = new[]
        {
            new DeviationRequest { Code = "DEMO-DEV-001", ProductId = productIds[0], Reason = "Minor paint chip on 3 frames — cosmetic only.", ProposedDisposition = "Ship with touch-up. Customer accepted.", AuthorizedQuantity = 3m, UnitMeasureCode = "EA", ValidFrom = DateOnly.FromDateTime(now), ValidTo = DateOnly.FromDateTime(now.AddDays(7)), Status = DeviationStatus.Pending, RaisedByUserId = "demo-seed", RaisedAt = now.AddHours(-4), ModifiedDate = now },
            new DeviationRequest { Code = "DEMO-DEV-002", ProductId = productIds.ElementAtOrDefault(1), Reason = "Wheel trueness 0.2mm over spec.", ProposedDisposition = "Rework in-line.", AuthorizedQuantity = 12m, UnitMeasureCode = "EA", ValidFrom = DateOnly.FromDateTime(now.AddDays(-2)), ValidTo = DateOnly.FromDateTime(now.AddDays(14)), Status = DeviationStatus.Approved, RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-2), DecidedByUserId = "demo-seed", DecidedAt = now.AddDays(-1), DecisionNotes = "Within customer tolerance.", ModifiedDate = now },
        };
        db.DeviationRequests.AddRange(deviations);
        await db.SaveChangesAsync(ct);
        rows += deviations.Length;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} engineering demo rows.", rows);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  MAINTENANCE
    // ────────────────────────────────────────────────────────────────

    public async Task<int> SeedMaintenanceAsync(List<int> assetIds, List<int> productIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.SpareParts.AnyAsync(p => p.PartNumber == "DEMO-SP-OIL", ct))
        {
            _logger.LogInformation("Maintenance demo seed: already present, skipping.");
            return 0;
        }

        var now = DateTime.UtcNow;
        var rows = 0;

        // Asset maintenance profiles — one per asset, unique on AssetId.
        var criticalities = new[] { AssetCriticality.Critical, AssetCriticality.High, AssetCriticality.Medium, AssetCriticality.Low, AssetCriticality.Medium };
        for (var i = 0; i < Math.Min(assetIds.Count, 5); i++)
        {
            if (!await db.AssetMaintenanceProfiles.AnyAsync(p => p.AssetId == assetIds[i], ct))
            {
                var profile = new AssetMaintenanceProfile
                {
                    AssetId = assetIds[i],
                    Criticality = criticalities[i % criticalities.Length],
                    TargetMtbfHours = 200 + i * 50,
                    Notes = "Seeded demo profile.",
                    ModifiedDate = now,
                };
                db.AssetMaintenanceProfiles.Add(profile);
                rows++;
            }
        }
        await db.SaveChangesAsync(ct);

        // PM schedules.
        var schedules = new[]
        {
            new PmSchedule { Code = "DEMO-PM-LUBE", Name = "Daily lubrication", AssetId = assetIds[0], IntervalKind = PmIntervalKind.Days, IntervalValue = 7, DefaultPriority = WorkOrderPriority.Medium, EstimatedMinutes = 20, IsActive = true, ModifiedDate = now },
            new PmSchedule { Code = "DEMO-PM-CAL", Name = "Monthly calibration", AssetId = assetIds[1 % assetIds.Count], IntervalKind = PmIntervalKind.Days, IntervalValue = 30, DefaultPriority = WorkOrderPriority.High, EstimatedMinutes = 60, IsActive = true, ModifiedDate = now },
            new PmSchedule { Code = "DEMO-PM-CLEAN", Name = "Deep clean @ 500 hrs", AssetId = assetIds[2 % assetIds.Count], IntervalKind = PmIntervalKind.RuntimeHours, IntervalValue = 500, DefaultPriority = WorkOrderPriority.Medium, EstimatedMinutes = 120, IsActive = true, ModifiedDate = now },
        };
        db.PmSchedules.AddRange(schedules);
        await db.SaveChangesAsync(ct);
        rows += schedules.Length;

        foreach (var s in schedules)
        {
            var tasks = new[]
            {
                new PmScheduleTask { PmScheduleId = s.Id, SequenceNumber = 10, TaskName = "Inspect visually", EstimatedMinutes = 5, ModifiedDate = now },
                new PmScheduleTask { PmScheduleId = s.Id, SequenceNumber = 20, TaskName = "Perform task", EstimatedMinutes = 15, RequiresSignoff = true, ModifiedDate = now },
                new PmScheduleTask { PmScheduleId = s.Id, SequenceNumber = 30, TaskName = "Log completion + clean up", EstimatedMinutes = 5, ModifiedDate = now },
            };
            db.PmScheduleTasks.AddRange(tasks);
            rows += tasks.Length;
        }
        await db.SaveChangesAsync(ct);

        // Work orders across all states.
        var workOrders = new[]
        {
            new MaintenanceWorkOrder { WorkOrderNumber = "DEMO-WO-001", Title = "Spindle vibration investigation", AssetId = assetIds[0], Type = WorkOrderType.Corrective, Status = WorkOrderStatus.Draft, Priority = WorkOrderPriority.High, RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-1), ModifiedDate = now },
            new MaintenanceWorkOrder { WorkOrderNumber = "DEMO-WO-002", Title = "PM: Daily lubrication", AssetId = assetIds[0], Type = WorkOrderType.Preventive, Status = WorkOrderStatus.Scheduled, Priority = WorkOrderPriority.Medium, PmScheduleId = schedules[0].Id, ScheduledFor = now.AddHours(4), RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-1), ModifiedDate = now },
            new MaintenanceWorkOrder { WorkOrderNumber = "DEMO-WO-003", Title = "Coolant system check", AssetId = assetIds[1 % assetIds.Count], Type = WorkOrderType.Inspection, Status = WorkOrderStatus.InProgress, Priority = WorkOrderPriority.Medium, StartedAt = now.AddHours(-2), RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-1), ModifiedDate = now },
            new MaintenanceWorkOrder { WorkOrderNumber = "DEMO-WO-004", Title = "Replace drive belt", AssetId = assetIds[2 % assetIds.Count], Type = WorkOrderType.Corrective, Status = WorkOrderStatus.Completed, Priority = WorkOrderPriority.High, StartedAt = now.AddDays(-3).AddHours(10), CompletedAt = now.AddDays(-3).AddHours(12), CompletionNotes = "Belt replaced, tensioned, tested. Back in service.", CompletedMeterValue = 1250.5m, RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-3), ModifiedDate = now.AddDays(-3).AddHours(12) },
            new MaintenanceWorkOrder { WorkOrderNumber = "DEMO-WO-005", Title = "Safety audit findings follow-up", AssetId = assetIds[3 % assetIds.Count], Type = WorkOrderType.Safety, Status = WorkOrderStatus.Scheduled, Priority = WorkOrderPriority.Critical, ScheduledFor = now.AddDays(2), RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-4), ModifiedDate = now },
        };
        db.MaintenanceWorkOrders.AddRange(workOrders);
        await db.SaveChangesAsync(ct);
        rows += workOrders.Length;

        // Tasks on the In-Progress WO.
        var inProgressWo = workOrders[2];
        var inProgressTasks = new[]
        {
            new MaintenanceWorkOrderTask { MaintenanceWorkOrderId = inProgressWo.Id, SequenceNumber = 10, TaskName = "Visual inspection", EstimatedMinutes = 10, ActualMinutes = 8, IsComplete = true, CompletedAt = now.AddHours(-1), CompletedByUserId = "demo-seed", ModifiedDate = now },
            new MaintenanceWorkOrderTask { MaintenanceWorkOrderId = inProgressWo.Id, SequenceNumber = 20, TaskName = "Coolant sample test", EstimatedMinutes = 15, IsComplete = false, RequiresSignoff = true, ModifiedDate = now },
            new MaintenanceWorkOrderTask { MaintenanceWorkOrderId = inProgressWo.Id, SequenceNumber = 30, TaskName = "Clean filter + reassemble", EstimatedMinutes = 20, IsComplete = false, ModifiedDate = now },
        };
        db.MaintenanceWorkOrderTasks.AddRange(inProgressTasks);
        rows += inProgressTasks.Length;

        // Spare parts.
        var parts = new[]
        {
            new SparePart { PartNumber = "DEMO-SP-OIL", Name = "Spindle oil 1L", UnitMeasureCode = "L", StandardCost = 8.50m, ReorderPoint = 10, ReorderQuantity = 24, IsActive = true, ModifiedDate = now },
            new SparePart { PartNumber = "DEMO-SP-BELT", Name = "Drive belt 2GT-120", ProductId = productIds.ElementAtOrDefault(3), UnitMeasureCode = "EA", StandardCost = 22.00m, ReorderPoint = 4, ReorderQuantity = 10, IsActive = true, ModifiedDate = now },
            new SparePart { PartNumber = "DEMO-SP-FILTER", Name = "Coolant filter 25-micron", UnitMeasureCode = "EA", StandardCost = 14.25m, ReorderPoint = 5, ReorderQuantity = 12, IsActive = true, ModifiedDate = now },
            new SparePart { PartNumber = "DEMO-SP-BEARING", Name = "Precision bearing 6205-2RS", UnitMeasureCode = "EA", StandardCost = 18.75m, ReorderPoint = 8, ReorderQuantity = 20, IsActive = true, ModifiedDate = now },
            new SparePart { PartNumber = "DEMO-SP-GASKET", Name = "Head gasket kit", UnitMeasureCode = "KIT", StandardCost = 45.00m, ReorderPoint = 2, ReorderQuantity = 4, IsActive = true, ModifiedDate = now },
            new SparePart { PartNumber = "DEMO-SP-WIPE", Name = "Industrial wipes (box of 100)", UnitMeasureCode = "BOX", StandardCost = 12.00m, ReorderPoint = 6, ReorderQuantity = 20, IsActive = true, ModifiedDate = now },
        };
        db.SpareParts.AddRange(parts);
        await db.SaveChangesAsync(ct);
        rows += parts.Length;

        // Part usage on the completed WO.
        var completedWo = workOrders[3];
        db.WorkOrderPartUsages.Add(new WorkOrderPartUsage
        {
            MaintenanceWorkOrderId = completedWo.Id,
            SparePartId = parts[1].Id,
            Quantity = 1m,
            UnitCost = 22.00m,
            UsedAt = now.AddDays(-3).AddHours(11),
            UsedByUserId = "demo-seed",
            Notes = "Drive belt replaced.",
            ModifiedDate = now,
        });
        rows++;

        // Meter readings — last 20 days of runtime-hours progression for asset 0.
        var baseHours = 1000m;
        for (var d = 20; d >= 0; d--)
        {
            db.MeterReadings.Add(new MeterReading
            {
                AssetId = assetIds[0],
                Kind = MeterKind.RuntimeHours,
                Value = baseHours + (20 - d) * 12.5m,
                RecordedAt = now.AddDays(-d),
                RecordedByUserId = "demo-seed",
                ModifiedDate = now,
            });
            rows++;
        }

        // Maintenance log entries.
        var logs = new[]
        {
            new MaintenanceLog { AssetId = assetIds[0], Kind = MaintenanceLogKind.Observation, Note = "Slight hum at startup. Monitoring.", AuthoredByUserId = "demo-seed", AuthoredAt = now.AddHours(-3), ModifiedDate = now },
            new MaintenanceLog { AssetId = assetIds[1 % assetIds.Count], Kind = MaintenanceLogKind.Warning, Note = "Coolant pressure at upper limit. Possible blockage.", AuthoredByUserId = "demo-seed", AuthoredAt = now.AddHours(-8), ModifiedDate = now },
            new MaintenanceLog { AssetId = assetIds[2 % assetIds.Count], Kind = MaintenanceLogKind.Incident, Note = "Unexpected stop mid-cycle. Restarted OK. See WO-004 for diagnosis.", MaintenanceWorkOrderId = completedWo.Id, AuthoredByUserId = "demo-seed", AuthoredAt = now.AddDays(-3).AddHours(9), ModifiedDate = now },
            new MaintenanceLog { AssetId = assetIds[0], Kind = MaintenanceLogKind.NearMiss, Note = "Guard interlock tested — needed adjustment. Fixed on the spot.", AuthoredByUserId = "demo-seed", AuthoredAt = now.AddDays(-1), ModifiedDate = now },
            new MaintenanceLog { AssetId = assetIds[3 % assetIds.Count], Kind = MaintenanceLogKind.Observation, Note = "Operator reports smoother cycle after last PM.", AuthoredByUserId = "demo-seed", AuthoredAt = now.AddHours(-2), ModifiedDate = now },
        };
        db.MaintenanceLogs.AddRange(logs);
        rows += logs.Length;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} maintenance demo rows.", rows);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  PERFORMANCE
    // ────────────────────────────────────────────────────────────────

    public async Task<int> SeedPerformanceAsync(List<int> stationIds, List<int> assetIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // The heavy time-series data (OEE snapshots, daily metrics, monthly maintenance) is
        // idempotent only at the (date, station/asset) level, so we skip it when the first KPI
        // definition is already present. The KPI/Scorecard seeding further down runs every time
        // and is per-code idempotent, so adding a new DEMO-KPI-* below this line will backfill
        // it on existing dev databases without re-creating the time-series rows.
        var heavyDataAlreadySeeded = await db.KpiDefinitions.AnyAsync(k => k.Code == "DEMO-KPI-OEE", ct);

        var now = DateTime.UtcNow;
        var today = now.Date;
        var rows = 0;

        if (heavyDataAlreadySeeded)
        {
            _logger.LogInformation("Performance demo seed: heavy data already present, only backfilling KPIs/scorecards.");
            rows += await EnsureKpisAndScorecardsAsync(db, now, ct);
            return rows;
        }

        // OEE snapshots — last 7 days, station 0.
        var rnd = new Random(17);
        for (var d = 7; d >= 1; d--)
        {
            var start = DateTime.SpecifyKind(today.AddDays(-d), DateTimeKind.Utc);
            var end = start.AddDays(1);

            // Randomised plausible values that land in green/yellow/red mix.
            var avail = 0.80m + (decimal)rnd.NextDouble() * 0.15m;
            var perf = 0.75m + (decimal)rnd.NextDouble() * 0.20m;
            var qual = 0.90m + (decimal)rnd.NextDouble() * 0.09m;

            // Skip if a snapshot already exists for this (station, kind, start) to avoid unique-index collisions.
            if (await db.OeeSnapshots.AnyAsync(s => s.StationId == stationIds[0] && s.PeriodKind == PerformancePeriodKind.Day && s.PeriodStart == start, ct))
                continue;

            var snap = new OeeSnapshot
            {
                StationId = stationIds[0],
                PeriodKind = PerformancePeriodKind.Day,
                PeriodStart = start,
                PeriodEnd = end,
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
            };
            db.OeeSnapshots.Add(snap);
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Production daily metrics for the last 7 days, first station.
        for (var d = 7; d >= 1; d--)
        {
            var date = DateOnly.FromDateTime(today.AddDays(-d));
            if (await db.ProductionDailyMetrics.AnyAsync(m => m.StationId == stationIds[0] && m.Date == date, ct))
                continue;
            db.ProductionDailyMetrics.Add(new ProductionDailyMetric
            {
                StationId = stationIds[0],
                Date = date,
                UnitsProduced = 200m + rnd.Next(-30, 40),
                UnitsScrapped = rnd.Next(0, 10),
                AverageCycleSeconds = 60m + (decimal)(rnd.NextDouble() * 10 - 5),
                YieldFraction = Math.Round(0.95m + (decimal)rnd.NextDouble() * 0.04m, 4),
                RunCount = rnd.Next(2, 5),
                ComputedAt = now,
                ModifiedDate = now,
            });
            rows++;
        }

        // Maintenance monthly metrics — last 3 months for first asset.
        for (var m = 3; m >= 1; m--)
        {
            var mdate = now.AddMonths(-m);
            var year = mdate.Year;
            var month = mdate.Month;
            if (await db.MaintenanceMonthlyMetrics.AnyAsync(x => x.AssetId == assetIds[0] && x.Year == year && x.Month == month, ct))
                continue;
            db.MaintenanceMonthlyMetrics.Add(new MaintenanceMonthlyMetric
            {
                AssetId = assetIds[0],
                Year = year,
                Month = month,
                WorkOrderCount = rnd.Next(2, 8),
                BreakdownCount = rnd.Next(0, 3),
                PmWorkOrderCount = rnd.Next(1, 4),
                PmCompletedCount = rnd.Next(1, 4),
                MtbfHours = 180m + rnd.Next(-30, 30),
                MttrHours = 2.5m + (decimal)rnd.NextDouble() * 1.5m,
                AvailabilityFraction = Math.Round(0.94m + (decimal)rnd.NextDouble() * 0.05m, 4),
                PmComplianceFraction = Math.Round(0.90m + (decimal)rnd.NextDouble() * 0.09m, 4),
                ComputedAt = now,
                ModifiedDate = now,
            });
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // KPI definitions, sample values, scorecards, scorecard memberships — extracted into a
        // per-code-idempotent helper so adding a new DEMO-KPI-* below this line will backfill
        // it on existing dev databases, not just fresh installs.
        rows += await EnsureKpisAndScorecardsAsync(db, now, ct);

        // Reports.
        var reports = new[]
        {
            new PerformanceReport { Code = "DEMO-RPT-OEE-WEEK", Name = "Weekly OEE by station", Description = "OEE for all stations for the last 7 days.", Kind = PerformanceReportKind.OeeSummary, RangePreset = ReportRangePreset.Last7Days, IsActive = true, ModifiedDate = now },
            new PerformanceReport { Code = "DEMO-RPT-MAINT-SC", Name = "Monthly maintenance scorecard", Description = "MTBF / MTTR / availability per asset for the month.", Kind = PerformanceReportKind.MaintenanceScorecard, RangePreset = ReportRangePreset.LastMonth, IsActive = true, ModifiedDate = now },
        };
        db.PerformanceReports.AddRange(reports);
        await db.SaveChangesAsync(ct);
        rows += reports.Length;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} performance demo rows.", rows);
        return rows;
    }

    /// <summary>
    /// Seeds (or backfills) the demo KPI definitions, a sample week-of value per KPI, the two
    /// demo scorecards, and the scorecard→KPI memberships. Each step is per-code idempotent —
    /// calling repeatedly only adds rows that are missing. Add a new entry to <c>specs</c>
    /// below to extend the demo set; existing dev databases will pick it up on the next call.
    /// </summary>
    private async Task<int> EnsureKpisAndScorecardsAsync(ApplicationDbContext db, DateTime now, CancellationToken ct)
    {
        var rows = 0;
        var today = now.Date;

        // KPI definitions. (Code, Name, Unit, Source, Aggregation, Target, Warning, Critical, Direction).
        // Order matters only for ScorecardKpi.DisplayOrder, which is derived from the per-scorecard
        // arrays further down; the KPI list itself can be reordered freely.
        var specs = new (string Code, string Name, string Unit, KpiSource Source, KpiAggregation Agg,
                         decimal Target, decimal Warn, decimal Crit, KpiDirection Dir, decimal Sample)[]
        {
            ("DEMO-KPI-OEE",     "Plant OEE",                    "%",      KpiSource.OeeOverall,            KpiAggregation.Average, 0.85m, 0.75m, 0.60m, KpiDirection.HigherIsBetter, 0.72m),
            ("DEMO-KPI-AVAIL",   "Availability",                 "%",      KpiSource.OeeAvailability,       KpiAggregation.Average, 0.90m, 0.80m, 0.70m, KpiDirection.HigherIsBetter, 0.88m),
            ("DEMO-KPI-PERF",    "Performance",                  "%",      KpiSource.OeePerformance,        KpiAggregation.Average, 0.95m, 0.85m, 0.70m, KpiDirection.HigherIsBetter, 0.83m),
            ("DEMO-KPI-QUAL",    "Quality",                      "%",      KpiSource.OeeQuality,            KpiAggregation.Average, 0.99m, 0.97m, 0.93m, KpiDirection.HigherIsBetter, 0.96m),
            ("DEMO-KPI-YIELD",   "First-pass yield",             "%",      KpiSource.ProductionYield,       KpiAggregation.Average, 0.98m, 0.95m, 0.90m, KpiDirection.HigherIsBetter, 0.97m),
            ("DEMO-KPI-THRU",    "Daily throughput",             "units",  KpiSource.ProductionUnits,       KpiAggregation.Sum,     1500m, 1200m,  900m, KpiDirection.HigherIsBetter, 1320m),
            ("DEMO-KPI-PEAK",    "Peak hourly throughput",       "units",  KpiSource.ProductionUnits,       KpiAggregation.Maximum,  220m,  180m,  140m, KpiDirection.HigherIsBetter,  195m),
            ("DEMO-KPI-CYCLE",   "Average cycle time",           "s",      KpiSource.ProductionCycleSeconds, KpiAggregation.Average,  60m,   75m,   90m, KpiDirection.LowerIsBetter,   72m),
            ("DEMO-KPI-MTBF",    "Mean time between failures",   "hrs",    KpiSource.MaintenanceMtbf,       KpiAggregation.Average,  200m,  150m,  100m, KpiDirection.HigherIsBetter,  175m),
            ("DEMO-KPI-MTTR",    "Mean time to repair",          "hrs",    KpiSource.MaintenanceMttr,       KpiAggregation.Average,  2.0m,  4.0m,  8.0m, KpiDirection.LowerIsBetter,   3.2m),
            ("DEMO-KPI-MAVAIL",  "Asset availability",           "%",      KpiSource.MaintenanceAvailability, KpiAggregation.Average, 0.95m, 0.90m, 0.85m, KpiDirection.HigherIsBetter, 0.93m),
            ("DEMO-KPI-PMCOMP",  "PM compliance",                "%",      KpiSource.MaintenancePmCompliance, KpiAggregation.Average, 0.95m, 0.85m, 0.70m, KpiDirection.HigherIsBetter, 0.92m),
        };

        var existingCodes = await db.KpiDefinitions
            .Where(k => specs.Select(s => s.Code).Contains(k.Code))
            .Select(k => new { k.Code, k.Id })
            .ToListAsync(ct);
        var idByCode = existingCodes.ToDictionary(x => x.Code, x => x.Id);

        var kpisToInsert = specs.Where(s => !idByCode.ContainsKey(s.Code))
            .Select(s => new KpiDefinition
            {
                Code = s.Code, Name = s.Name, Unit = s.Unit,
                Source = s.Source, Aggregation = s.Agg,
                TargetValue = s.Target, WarningThreshold = s.Warn, CriticalThreshold = s.Crit,
                Direction = s.Dir, IsActive = true, ModifiedDate = now,
            })
            .ToList();

        if (kpisToInsert.Count > 0)
        {
            db.KpiDefinitions.AddRange(kpisToInsert);
            await db.SaveChangesAsync(ct);
            foreach (var k in kpisToInsert)
            {
                idByCode[k.Code] = k.Id;
            }
            await db.SaveChangesAsync(ct);
            rows += kpisToInsert.Count;
        }

        // KPI values — one sample per KPI for the last week, only insert if no value exists yet.
        var weekStart = DateTime.SpecifyKind(today.AddDays(-7), DateTimeKind.Utc);
        var weekEnd = DateTime.SpecifyKind(today, DateTimeKind.Utc);
        foreach (var s in specs)
        {
            if (!idByCode.TryGetValue(s.Code, out var kpiId)) continue;
            if (await db.KpiValues.AnyAsync(v => v.KpiDefinitionId == kpiId && v.PeriodStart == weekStart, ct)) continue;

            var status = (s.Dir == KpiDirection.HigherIsBetter)
                ? (s.Sample < s.Crit ? KpiStatus.Critical
                    : s.Sample < s.Warn ? KpiStatus.Warning
                    : KpiStatus.OnTarget)
                : (s.Sample > s.Crit ? KpiStatus.Critical
                    : s.Sample > s.Warn ? KpiStatus.Warning
                    : KpiStatus.OnTarget);
            db.KpiValues.Add(new KpiValue
            {
                KpiDefinitionId = kpiId,
                PeriodKind = PerformancePeriodKind.Week,
                PeriodStart = weekStart,
                PeriodEnd = weekEnd,
                Value = s.Sample,
                TargetAtPeriod = s.Target,
                Status = status,
                ComputedAt = now,
                ModifiedDate = now,
            });
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Scorecards.
        var pmScorecard = await db.ScorecardDefinitions.FirstOrDefaultAsync(s => s.Code == "DEMO-SC-PLANT", ct);
        if (pmScorecard is null)
        {
            pmScorecard = new ScorecardDefinition { Code = "DEMO-SC-PLANT", Name = "Plant Manager Dashboard", Description = "Top-level plant performance at a glance.", OwnerUserId = "demo-seed", IsActive = true, ModifiedDate = now };
            db.ScorecardDefinitions.Add(pmScorecard);
            await db.SaveChangesAsync(ct);
            rows++;
        }
        var maintScorecard = await db.ScorecardDefinitions.FirstOrDefaultAsync(s => s.Code == "DEMO-SC-MAINT", ct);
        if (maintScorecard is null)
        {
            maintScorecard = new ScorecardDefinition { Code = "DEMO-SC-MAINT", Name = "Maintenance Lead Dashboard", Description = "Reliability + PM compliance.", OwnerUserId = "demo-seed", IsActive = true, ModifiedDate = now };
            db.ScorecardDefinitions.Add(maintScorecard);
            await db.SaveChangesAsync(ct);
            rows++;
        }

        // Scorecard memberships — per-(scorecardId,kpiId) idempotent.
        var plantKpiCodes = new[] { "DEMO-KPI-OEE", "DEMO-KPI-AVAIL", "DEMO-KPI-PERF", "DEMO-KPI-QUAL", "DEMO-KPI-YIELD", "DEMO-KPI-THRU", "DEMO-KPI-PEAK", "DEMO-KPI-CYCLE" };
        var maintKpiCodes = new[] { "DEMO-KPI-MTBF", "DEMO-KPI-MTTR", "DEMO-KPI-MAVAIL", "DEMO-KPI-PMCOMP", "DEMO-KPI-AVAIL" };
        rows += await EnsureScorecardKpisAsync(db, pmScorecard.Id, plantKpiCodes, idByCode, now, ct);
        rows += await EnsureScorecardKpisAsync(db, maintScorecard.Id, maintKpiCodes, idByCode, now, ct);

        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  INVENTORY — seed locations + items + a couple of adjustments
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds representative inventory master data (3 locations, up to 5 InventoryItem rows
    /// bound to the first few makeflag products, and a couple of InventoryAdjustment rows).
    /// Idempotent — keyed off the "DEMO-LOC-WH1" warehouse code.
    /// </summary>
    public async Task<int> SeedInventoryAsync(List<int> productIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.InventoryLocations.AnyAsync(l => l.Code == "DEMO-LOC-WH1", ct))
        {
            _logger.LogInformation("Inventory demo seed: already present, skipping.");
            return 0;
        }

        var now = DateTime.UtcNow;
        var rows = 0;

        var org = await db.Organizations.OrderBy(o => o.Id).Select(o => o.Id).FirstOrDefaultAsync(ct);
        if (org == 0)
        {
            _logger.LogWarning("Inventory demo seed: no organization — skipping.");
            return 0;
        }

        // Locations: a warehouse with two child bins. The real app builds a deeper tree, but
        // three rows are enough to show the list page and anchor inventory items / adjustments.
        var warehouse = new InventoryLocation
        {
            OrganizationId = org, Code = "DEMO-LOC-WH1", Name = "Demo Warehouse 1",
            Kind = InventoryLocationKind.Warehouse, Path = "DEMO-LOC-WH1", Depth = 0,
            IsActive = true, ModifiedDate = now,
        };
        db.InventoryLocations.Add(warehouse);
        await db.SaveChangesAsync(ct);
        rows++;

        var binA = new InventoryLocation
        {
            OrganizationId = org, Code = "DEMO-LOC-BIN-A1", Name = "Bin A1",
            Kind = InventoryLocationKind.Bin, ParentLocationId = warehouse.Id,
            Path = $"{warehouse.Code}/DEMO-LOC-BIN-A1", Depth = 1,
            IsActive = true, ModifiedDate = now,
        };
        var binB = new InventoryLocation
        {
            OrganizationId = org, Code = "DEMO-LOC-BIN-B2", Name = "Bin B2",
            Kind = InventoryLocationKind.Bin, ParentLocationId = warehouse.Id,
            Path = $"{warehouse.Code}/DEMO-LOC-BIN-B2", Depth = 1,
            IsActive = true, ModifiedDate = now,
        };
        db.InventoryLocations.AddRange(binA, binB);
        await db.SaveChangesAsync(ct);
        rows += 2;

        // InventoryItems — bind the first handful of AW products into the inventory layer so
        // the /inventory/items page renders rows and the Product Explorer flags those products
        // as "inventory-managed".
        var itemProducts = productIds.Take(Math.Min(5, productIds.Count)).ToList();
        var rnd = new Random(17);
        var createdItems = new List<InventoryItem>();
        foreach (var pid in itemProducts)
        {
            if (await db.InventoryItems.AnyAsync(i => i.ProductId == pid, ct)) continue;
            var item = new InventoryItem
            {
                ProductId = pid,
                TracksLot = rnd.Next(2) == 0,  // roughly half the items track lots for variety
                TracksSerial = false,
                DefaultLocationId = binA.Id,
                MinQty = 10m, MaxQty = 500m, ReorderPoint = 50m, ReorderQty = 100m,
                IsActive = true, ModifiedDate = now,
            };
            db.InventoryItems.Add(item);
            createdItems.Add(item);
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Two adjustment rows — one Draft, one Posted — so the list has both open and closed
        // entries and the status chip filter on the page can be exercised.
        if (createdItems.Count > 0)
        {
            db.InventoryAdjustments.Add(new InventoryAdjustment
            {
                AdjustmentNumber = "DEMO-ADJ-001",
                InventoryItemId = createdItems[0].Id, LocationId = binA.Id,
                QuantityDelta = -3m, ReasonCode = AdjustmentReason.Damaged,
                Reason = "Forklift damage during unload.",
                Status = AdjustmentStatus.Draft,
                RequestedByUserId = "demo-seed", RequestedAt = now.AddDays(-2),
                ModifiedDate = now,
            });
            db.InventoryAdjustments.Add(new InventoryAdjustment
            {
                AdjustmentNumber = "DEMO-ADJ-002",
                InventoryItemId = createdItems[Math.Min(1, createdItems.Count - 1)].Id, LocationId = binB.Id,
                QuantityDelta = 5m, ReasonCode = AdjustmentReason.Found,
                Reason = "Count variance resolved — 5 extra found in Bin B2.",
                Status = AdjustmentStatus.Posted,
                RequestedByUserId = "demo-seed", RequestedAt = now.AddDays(-7),
                ApprovedByUserId = "demo-seed", ApprovedAt = now.AddDays(-6),
                ModifiedDate = now,
            });
            rows += 2;
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} inventory demo rows.", rows);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  LOGISTICS — seed a goods receipt, a shipment, a stock transfer
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds one of each logistics document so the three list pages render. Pulls PO/vendor
    /// FKs from AW data and location FKs from the inventory demo seed. Idempotent — keyed off
    /// the "DEMO-GR-001" receipt number.
    /// </summary>
    public async Task<int> SeedLogisticsAsync(CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.GoodsReceipts.AnyAsync(r => r.ReceiptNumber == "DEMO-GR-001", ct))
        {
            _logger.LogInformation("Logistics demo seed: already present, skipping.");
            return 0;
        }

        // All three logistics documents need at least one inventory location. If the inventory
        // seed hasn't populated any, skip — caller should run SeedInventoryAsync first.
        var anyLocation = await db.InventoryLocations.OrderBy(l => l.Id).Select(l => l.Id).FirstOrDefaultAsync(ct);
        if (anyLocation == 0)
        {
            _logger.LogWarning("Logistics demo seed: no inventory locations — run SeedInventoryAsync first. Skipping.");
            return 0;
        }
        var fromBin = await db.InventoryLocations.Where(l => l.Kind == InventoryLocationKind.Bin)
            .OrderBy(l => l.Id).Select(l => l.Id).FirstOrDefaultAsync(ct);
        var toBin = await db.InventoryLocations.Where(l => l.Kind == InventoryLocationKind.Bin)
            .OrderByDescending(l => l.Id).Select(l => l.Id).FirstOrDefaultAsync(ct);
        if (fromBin == 0 || toBin == 0 || fromBin == toBin)
        {
            // Fall back to the first two locations we have — the transfer still shows rows,
            // just with source == destination which the UI permits.
            fromBin = anyLocation;
            toBin = anyLocation;
        }

        // Grab any real PO/vendor/sales-order from AW so the FK targets are valid.
        var firstPo = await db.Database
            .SqlQueryRaw<int>("SELECT TOP 1 PurchaseOrderID AS Value FROM Purchasing.PurchaseOrderHeader ORDER BY PurchaseOrderID")
            .FirstOrDefaultAsync(ct);
        var firstVendor = await db.Database
            .SqlQueryRaw<int>("SELECT TOP 1 BusinessEntityID AS Value FROM Purchasing.Vendor ORDER BY BusinessEntityID")
            .FirstOrDefaultAsync(ct);
        var firstSo = await db.Database
            .SqlQueryRaw<int>("SELECT TOP 1 SalesOrderID AS Value FROM Sales.SalesOrderHeader ORDER BY SalesOrderID")
            .FirstOrDefaultAsync(ct);

        var now = DateTime.UtcNow;
        var rows = 0;

        db.GoodsReceipts.Add(new GoodsReceipt
        {
            ReceiptNumber = "DEMO-GR-001",
            PurchaseOrderId = firstPo == 0 ? null : firstPo,
            VendorBusinessEntityId = firstVendor == 0 ? null : firstVendor,
            ReceivedLocationId = anyLocation,
            Status = GoodsReceiptStatus.Posted,
            ReceivedAt = now.AddDays(-3),
            PostedAt = now.AddDays(-3).AddHours(2),
            PostedByUserId = "demo-seed",
            Notes = "Demo inbound receipt — PO fully received.",
            ModifiedDate = now,
        });
        db.GoodsReceipts.Add(new GoodsReceipt
        {
            ReceiptNumber = "DEMO-GR-002",
            PurchaseOrderId = firstPo == 0 ? null : firstPo,
            VendorBusinessEntityId = firstVendor == 0 ? null : firstVendor,
            ReceivedLocationId = anyLocation,
            Status = GoodsReceiptStatus.Draft,
            ReceivedAt = now.AddHours(-4),
            Notes = "Demo inbound receipt — awaiting count verification.",
            ModifiedDate = now,
        });
        rows += 2;

        db.Shipments.Add(new Shipment
        {
            ShipmentNumber = "DEMO-SH-001",
            SalesOrderId = firstSo == 0 ? null : firstSo,
            ShippedFromLocationId = anyLocation,
            Status = ShipmentStatus.Shipped,
            ShippedAt = now.AddDays(-1),
            PostedAt = now.AddDays(-1).AddMinutes(30),
            PostedByUserId = "demo-seed",
            TrackingNumber = "DEMO1Z999AA10123456784",
            Notes = "Demo outbound shipment.",
            ModifiedDate = now,
        });
        db.Shipments.Add(new Shipment
        {
            ShipmentNumber = "DEMO-SH-002",
            SalesOrderId = firstSo == 0 ? null : firstSo,
            ShippedFromLocationId = anyLocation,
            Status = ShipmentStatus.Packed,
            Notes = "Demo outbound shipment — packed, awaiting carrier pickup.",
            ModifiedDate = now,
        });
        rows += 2;

        db.StockTransfers.Add(new StockTransfer
        {
            TransferNumber = "DEMO-XF-001",
            FromLocationId = fromBin,
            ToLocationId = toBin,
            Status = StockTransferStatus.Completed,
            CorrelationId = Guid.NewGuid(),
            InitiatedAt = now.AddDays(-5),
            CompletedAt = now.AddDays(-5).AddHours(3),
            PostedByUserId = "demo-seed",
            Notes = "Demo internal transfer — bin rebalance.",
            ModifiedDate = now,
        });
        db.StockTransfers.Add(new StockTransfer
        {
            TransferNumber = "DEMO-XF-002",
            FromLocationId = fromBin,
            ToLocationId = toBin,
            Status = StockTransferStatus.InTransit,
            InitiatedAt = now.AddHours(-6),
            Notes = "Demo transfer — in transit.",
            ModifiedDate = now,
        });
        rows += 2;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} logistics demo rows.", rows);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  QUALITY — seed inspection plans + inspections + NCR + CAPA chain
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds the QA chain end-to-end: 2 InspectionPlans (Inbound + InProcess), 2 Inspections
    /// (Pending + Pass), 1 NonConformance (Open), 1 CapaCase (Open). Idempotent — keyed off
    /// the "DEMO-QA-PLAN-INB" plan code. The NCR pulls an InventoryItem if any exist (so seed
    /// the inventory module first); skips the NCR + CAPA chain when no items are present.
    /// </summary>
    public async Task<int> SeedQualityAsync(List<int> productIds, List<int> employeeIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.InspectionPlans.AnyAsync(p => p.PlanCode == "DEMO-QA-PLAN-INB", ct))
        {
            _logger.LogInformation("Quality demo seed: already present, skipping.");
            return 0;
        }

        var now = DateTime.UtcNow;
        var rows = 0;

        // Two reusable plans — one inbound (auto-trigger on receipt), one in-process.
        var inboundPlan = new InspectionPlan
        {
            PlanCode = "DEMO-QA-PLAN-INB",
            Name = "Inbound receipt sampling",
            Description = "Sample 5% of every inbound receipt; reject lots with > 2% defects.",
            Scope = InspectionScope.Inbound,
            ProductId = productIds.Count > 0 ? productIds[0] : null,
            SamplingRule = "5% per lot",
            AutoTriggerOnReceipt = true,
            IsActive = true, ModifiedDate = now,
        };
        var inProcessPlan = new InspectionPlan
        {
            PlanCode = "DEMO-QA-PLAN-IP",
            Name = "First-piece in-process check",
            Description = "First-piece inspection at start of every production run.",
            Scope = InspectionScope.InProcess,
            ProductId = productIds.Count > 1 ? productIds[1] : null,
            SamplingRule = "First piece",
            AutoTriggerOnProductionRun = true,
            IsActive = true, ModifiedDate = now,
        };
        db.InspectionPlans.AddRange(inboundPlan, inProcessPlan);
        await db.SaveChangesAsync(ct);
        rows += 2;

        // Two inspections — one Pending (newly raised), one Pass (closed cleanly).
        var firstItem = await db.InventoryItems.OrderBy(i => i.Id).Select(i => new { i.Id }).FirstOrDefaultAsync(ct);
        var pendingInspection = new Inspection
        {
            InspectionNumber = "DEMO-INSP-001",
            InspectionPlanId = inboundPlan.Id,
            Status = InspectionStatus.Pending,
            SourceKind = InspectionSourceKind.Manual,
            SourceId = 0,
            InspectorBusinessEntityId = employeeIds.Count > 0 ? employeeIds[0] : null,
            InventoryItemId = firstItem?.Id,
            Quantity = 100m,
            UnitMeasureCode = "EA",
            Notes = "Seeded demo inspection — awaiting inspector.",
            ModifiedDate = now,
        };
        var passedInspection = new Inspection
        {
            InspectionNumber = "DEMO-INSP-002",
            InspectionPlanId = inProcessPlan.Id,
            Status = InspectionStatus.Pass,
            SourceKind = InspectionSourceKind.Manual,
            SourceId = 0,
            InspectorBusinessEntityId = employeeIds.Count > 0 ? employeeIds[0] : null,
            InspectedAt = now.AddDays(-2),
            InventoryItemId = firstItem?.Id,
            Quantity = 50m,
            UnitMeasureCode = "EA",
            Notes = "Seeded demo inspection — first-piece check passed.",
            PostedByUserId = "demo-seed",
            ModifiedDate = now,
        };
        db.Inspections.AddRange(pendingInspection, passedInspection);
        await db.SaveChangesAsync(ct);
        rows += 2;

        // NCR + CAPA chain — only if we have an InventoryItem to anchor the NCR to.
        if (firstItem is not null)
        {
            var ncr = new NonConformance
            {
                NcrNumber = "DEMO-NCR-001",
                InspectionId = passedInspection.Id, // ties to a (passed) inspection — fine for demo
                InventoryItemId = firstItem.Id,
                Quantity = 5m,
                UnitMeasureCode = "EA",
                Description = "Demo NCR — surface defect found on small lot.",
                Status = NonConformanceStatus.Open,
                ModifiedDate = now,
            };
            db.NonConformances.Add(ncr);
            await db.SaveChangesAsync(ct);
            rows++;

            db.CapaCases.Add(new CapaCase
            {
                CaseNumber = "DEMO-CAPA-001",
                Title = "Investigate recurring surface defects",
                Status = CapaStatus.Investigation,
                RootCause = "Pending — under investigation.",
                OwnerBusinessEntityId = employeeIds.Count > 0 ? employeeIds[0] : null,
                OpenedAt = now.AddDays(-1),
                ModifiedDate = now,
            });
            rows++;
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} quality demo rows.", rows);
        return rows;
    }

    // ────────────────────────────────────────────────────────────────
    //  MES — seed production runs + downtime events + work instruction
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds a representative slice of MES activity: 2 ProductionRuns, 2 DowntimeEvents tied
    /// to a real DowntimeReason code, 1 WorkInstruction tied to a real AW WorkOrderRouting.
    /// Idempotent — keyed off the "DEMO-RUN-001" run number. Pulls AW WorkOrder + Routing IDs
    /// from the legacy schema; skips affected rows when those FK targets are missing.
    /// </summary>
    public async Task<int> SeedMesAsync(List<int> stationIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.ProductionRuns.AnyAsync(r => r.RunNumber == "DEMO-RUN-001", ct))
        {
            _logger.LogInformation("MES demo seed: already present, skipping.");
            return 0;
        }

        var now = DateTime.UtcNow;
        var rows = 0;

        // Pull a real AW WorkOrder for the production-run FK — seeded by AdventureWorks
        // restore. WorkInstruction.WorkOrderRoutingId is a soft int FK (no DB-level constraint
        // and no join in the list endpoint), so we point it at the demo-seeded engineering
        // ManufacturingRouting Id since that entity has a usable surrogate key — the legacy AW
        // Production.WorkOrderRouting has only a composite PK and no Id column to point at.
        var firstAwWorkOrderId = await db.Database
            .SqlQueryRaw<int>("SELECT TOP 1 WorkOrderID AS Value FROM Production.WorkOrder ORDER BY WorkOrderID")
            .FirstOrDefaultAsync(ct);
        var demoRoutingId = await db.ManufacturingRoutings
            .Where(r => r.Code == "DEMO-RT-ROAD")
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync(ct);

        var firstStation = stationIds.Count > 0 ? (int?)stationIds[0] : null;

        // 2 ProductionRuns — Completed + InProgress.
        var completedRun = new ProductionRun
        {
            RunNumber = "DEMO-RUN-001",
            Kind = ProductionRunKind.Production,
            WorkOrderId = firstAwWorkOrderId == 0 ? null : firstAwWorkOrderId,
            StationId = firstStation,
            Status = ProductionRunStatus.Completed,
            PlannedStartAt = now.AddDays(-2),
            ActualStartAt = now.AddDays(-2).AddMinutes(15),
            ActualEndAt = now.AddDays(-1).AddHours(-3),
            QuantityPlanned = 100m,
            QuantityProduced = 98m,
            QuantityScrapped = 2m,
            Notes = "Seeded demo run — completed cleanly.",
            PostedByUserId = "demo-seed",
            ModifiedDate = now,
        };
        var inProgressRun = new ProductionRun
        {
            RunNumber = "DEMO-RUN-002",
            Kind = ProductionRunKind.Production,
            WorkOrderId = firstAwWorkOrderId == 0 ? null : firstAwWorkOrderId,
            StationId = firstStation,
            Status = ProductionRunStatus.InProgress,
            PlannedStartAt = now.AddHours(-2),
            ActualStartAt = now.AddHours(-2),
            QuantityPlanned = 50m,
            QuantityProduced = 24m,
            QuantityScrapped = 0m,
            Notes = "Seeded demo run — currently running.",
            ModifiedDate = now,
        };
        db.ProductionRuns.AddRange(completedRun, inProgressRun);
        await db.SaveChangesAsync(ct);
        rows += 2;

        // 2 DowntimeEvents — only if we have at least one DowntimeReason (auto-seeded on boot)
        // and one Station. One closed (setup), one open (machine fault).
        var setupReasonId = await db.DowntimeReasons.Where(r => r.Code == DowntimeReasonCodes.Setup).Select(r => (int?)r.Id).FirstOrDefaultAsync(ct);
        var faultReasonId = await db.DowntimeReasons.Where(r => r.Code == DowntimeReasonCodes.MachineFault).Select(r => (int?)r.Id).FirstOrDefaultAsync(ct);
        if (firstStation.HasValue && setupReasonId.HasValue && faultReasonId.HasValue)
        {
            db.DowntimeEvents.Add(new DowntimeEvent
            {
                ProductionRunId = completedRun.Id,
                StationId = firstStation.Value,
                DowntimeReasonId = setupReasonId.Value,
                StartAt = now.AddDays(-2).AddMinutes(0),
                EndAt = now.AddDays(-2).AddMinutes(15),
                Notes = "Seeded demo downtime — startup setup before run.",
                ModifiedDate = now,
            });
            db.DowntimeEvents.Add(new DowntimeEvent
            {
                ProductionRunId = inProgressRun.Id,
                StationId = firstStation.Value,
                DowntimeReasonId = faultReasonId.Value,
                StartAt = now.AddMinutes(-30),
                EndAt = null, // still open
                Notes = "Seeded demo downtime — sensor fault under investigation.",
                ModifiedDate = now,
            });
            rows += 2;
        }

        // 1 WorkInstruction tied to the engineering demo routing. The routing must exist —
        // SeedEngineeringAsync runs before this method in SeedAllAsync, so it should always
        // be present unless the engineering seed itself was skipped.
        if (demoRoutingId.HasValue)
        {
            db.WorkInstructions.Add(new WorkInstruction
            {
                WorkOrderRoutingId = demoRoutingId.Value,
                Title = "Demo work instruction — Road Frame routing",
                IsActive = true,
                ModifiedDate = now,
            });
            rows++;
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} MES demo rows.", rows);
        return rows;
    }

    private static async Task<int> EnsureScorecardKpisAsync(
        ApplicationDbContext db, int scorecardId, string[] kpiCodes,
        Dictionary<string, int> idByCode, DateTime now, CancellationToken ct)
    {
        var existing = await db.ScorecardKpis.AsNoTracking()
            .Where(k => k.ScorecardDefinitionId == scorecardId)
            .Select(k => k.KpiDefinitionId)
            .ToListAsync(ct);
        var existingSet = existing.ToHashSet();
        var added = 0;
        for (var i = 0; i < kpiCodes.Length; i++)
        {
            if (!idByCode.TryGetValue(kpiCodes[i], out var kpiId)) continue;
            if (existingSet.Contains(kpiId)) continue;
            db.ScorecardKpis.Add(new ScorecardKpi
            {
                ScorecardDefinitionId = scorecardId,
                KpiDefinitionId = kpiId,
                DisplayOrder = (i + 1) * 10,
                Visual = ScorecardKpiVisual.KpiCard,
                ModifiedDate = now,
            });
            added++;
        }
        if (added > 0) await db.SaveChangesAsync(ct);
        return added;
    }
}

public sealed class DemoSeedResult
{
    public bool Skipped { get; set; }
    public int Baseline { get; set; }
    public int Workforce { get; set; }
    public int Engineering { get; set; }
    public int Maintenance { get; set; }
    public int Performance { get; set; }
    public int Inventory { get; set; }
    public int Logistics { get; set; }
    public int Quality { get; set; }
    public int Mes { get; set; }
    public int Total => Baseline + Workforce + Engineering + Maintenance + Performance + Inventory + Logistics + Quality + Mes;
}
