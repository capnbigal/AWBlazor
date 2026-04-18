using AWBlazorApp.Features.Engineering.Audit;
using AWBlazorApp.Features.Engineering.Domain;
using AWBlazorApp.Features.Maintenance.Audit;
using AWBlazorApp.Features.Maintenance.Domain;
using AWBlazorApp.Features.Performance.Audit;
using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Domain;
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
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var employeeIds = await db.Employees.AsNoTracking().OrderBy(e => e.Id).Take(12).Select(e => e.Id).ToListAsync(cancellationToken);
        var stationIds = await db.Stations.AsNoTracking().OrderBy(s => s.Id).Take(6).Select(s => s.Id).ToListAsync(cancellationToken);
        var assetIds = await db.Assets.AsNoTracking().OrderBy(a => a.Id).Take(6).Select(a => a.Id).ToListAsync(cancellationToken);
        var productIds = await db.Database.SqlQueryRaw<int>("SELECT TOP 6 ProductID AS Value FROM Production.Product WHERE MakeFlag = 1 ORDER BY ProductID").ToListAsync(cancellationToken);

        if (employeeIds.Count == 0 || stationIds.Count == 0 || assetIds.Count == 0 || productIds.Count == 0)
        {
            _logger.LogWarning("Demo seed skipped — upstream FK targets missing. Employees={E} Stations={S} Assets={A} Products={P}",
                employeeIds.Count, stationIds.Count, assetIds.Count, productIds.Count);
            result.Skipped = true;
            return result;
        }

        result.Workforce = await SeedWorkforceAsync(employeeIds, stationIds, cancellationToken);
        result.Engineering = await SeedEngineeringAsync(productIds, stationIds, cancellationToken);
        result.Maintenance = await SeedMaintenanceAsync(assetIds, productIds, cancellationToken);
        result.Performance = await SeedPerformanceAsync(stationIds, assetIds, cancellationToken);

        _logger.LogInformation("Demo seed complete: wf={Wf} eng={Eng} maint={Maint} perf={Perf}",
            result.Workforce, result.Engineering, result.Maintenance, result.Performance);
        return result;
    }

    // ────────────────────────────────────────────────────────────────
    //  WORKFORCE
    // ────────────────────────────────────────────────────────────────

    public async Task<int> SeedWorkforceAsync(List<int> employeeIds, List<int> stationIds, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // Marker: DEMO-FORKLIFT training course.
        if (await db.TrainingCourses.AnyAsync(c => c.Code == "DEMO-FORKLIFT", ct))
        {
            _logger.LogInformation("Workforce demo seed: already present, skipping.");
            return 0;
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
        foreach (var c in courses) db.TrainingCourseAuditLogs.Add(TrainingCourseAuditService.RecordCreate(c, "demo-seed"));
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
        foreach (var q in quals) db.QualificationAuditLogs.Add(QualificationAuditService.RecordCreate(q, "demo-seed"));
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
        foreach (var l in leaves) db.LeaveRequestAuditLogs.Add(LeaveRequestAuditService.RecordCreate(l, "demo-seed"));
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
        foreach (var a in announcements) db.AnnouncementAuditLogs.Add(AnnouncementAuditService.RecordCreate(a, "demo-seed"));
        rows += announcements.Length;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} workforce demo rows.", rows);
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
        foreach (var r in routings) db.ManufacturingRoutingAuditLogs.Add(ManufacturingRoutingAuditService.RecordCreate(r, "demo-seed"));
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
        foreach (var b in boms) db.BomHeaderAuditLogs.Add(BomHeaderAuditService.RecordCreate(b, "demo-seed"));
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
        db.EngineeringChangeOrderAuditLogs.Add(EngineeringChangeOrderAuditService.RecordCreate(draftEco, "demo-seed"));
        db.EngineeringChangeOrderAuditLogs.Add(EngineeringChangeOrderAuditService.RecordCreate(reviewEco, "demo-seed"));
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
        foreach (var d in docs) db.EngineeringDocumentAuditLogs.Add(EngineeringDocumentAuditService.RecordCreate(d, "demo-seed"));
        rows += docs.Length;

        // Deviations.
        var deviations = new[]
        {
            new DeviationRequest { Code = "DEMO-DEV-001", ProductId = productIds[0], Reason = "Minor paint chip on 3 frames — cosmetic only.", ProposedDisposition = "Ship with touch-up. Customer accepted.", AuthorizedQuantity = 3m, UnitMeasureCode = "EA", ValidFrom = DateOnly.FromDateTime(now), ValidTo = DateOnly.FromDateTime(now.AddDays(7)), Status = DeviationStatus.Pending, RaisedByUserId = "demo-seed", RaisedAt = now.AddHours(-4), ModifiedDate = now },
            new DeviationRequest { Code = "DEMO-DEV-002", ProductId = productIds.ElementAtOrDefault(1), Reason = "Wheel trueness 0.2mm over spec.", ProposedDisposition = "Rework in-line.", AuthorizedQuantity = 12m, UnitMeasureCode = "EA", ValidFrom = DateOnly.FromDateTime(now.AddDays(-2)), ValidTo = DateOnly.FromDateTime(now.AddDays(14)), Status = DeviationStatus.Approved, RaisedByUserId = "demo-seed", RaisedAt = now.AddDays(-2), DecidedByUserId = "demo-seed", DecidedAt = now.AddDays(-1), DecisionNotes = "Within customer tolerance.", ModifiedDate = now },
        };
        db.DeviationRequests.AddRange(deviations);
        await db.SaveChangesAsync(ct);
        foreach (var d in deviations) db.DeviationRequestAuditLogs.Add(DeviationRequestAuditService.RecordCreate(d, "demo-seed"));
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
        foreach (var s in schedules) db.PmScheduleAuditLogs.Add(PmScheduleAuditService.RecordCreate(s, "demo-seed"));
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
        foreach (var w in workOrders) db.MaintenanceWorkOrderAuditLogs.Add(MaintenanceWorkOrderAuditService.RecordCreate(w, "demo-seed"));
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
        foreach (var p in parts) db.SparePartAuditLogs.Add(SparePartAuditService.RecordCreate(p, "demo-seed"));
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
        if (await db.KpiDefinitions.AnyAsync(k => k.Code == "DEMO-KPI-OEE", ct))
        {
            _logger.LogInformation("Performance demo seed: already present, skipping.");
            return 0;
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var rows = 0;

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

        // KPIs.
        var kpis = new[]
        {
            new KpiDefinition { Code = "DEMO-KPI-OEE", Name = "Plant OEE", Unit = "%", Source = KpiSource.OeeOverall, Aggregation = KpiAggregation.Average, TargetValue = 0.85m, WarningThreshold = 0.75m, CriticalThreshold = 0.60m, Direction = KpiDirection.HigherIsBetter, IsActive = true, ModifiedDate = now },
            new KpiDefinition { Code = "DEMO-KPI-AVAIL", Name = "Availability", Unit = "%", Source = KpiSource.OeeAvailability, Aggregation = KpiAggregation.Average, TargetValue = 0.90m, WarningThreshold = 0.80m, CriticalThreshold = 0.70m, Direction = KpiDirection.HigherIsBetter, IsActive = true, ModifiedDate = now },
            new KpiDefinition { Code = "DEMO-KPI-YIELD", Name = "First-pass yield", Unit = "%", Source = KpiSource.ProductionYield, Aggregation = KpiAggregation.Average, TargetValue = 0.98m, WarningThreshold = 0.95m, CriticalThreshold = 0.90m, Direction = KpiDirection.HigherIsBetter, IsActive = true, ModifiedDate = now },
            new KpiDefinition { Code = "DEMO-KPI-THRU", Name = "Daily throughput", Unit = "units", Source = KpiSource.ProductionUnits, Aggregation = KpiAggregation.Sum, TargetValue = 1500m, WarningThreshold = 1200m, CriticalThreshold = 900m, Direction = KpiDirection.HigherIsBetter, IsActive = true, ModifiedDate = now },
            new KpiDefinition { Code = "DEMO-KPI-MTBF", Name = "Mean time between failures", Unit = "hrs", Source = KpiSource.MaintenanceMtbf, Aggregation = KpiAggregation.Average, TargetValue = 200m, WarningThreshold = 150m, CriticalThreshold = 100m, Direction = KpiDirection.HigherIsBetter, IsActive = true, ModifiedDate = now },
            new KpiDefinition { Code = "DEMO-KPI-PMCOMP", Name = "PM compliance", Unit = "%", Source = KpiSource.MaintenancePmCompliance, Aggregation = KpiAggregation.Average, TargetValue = 0.95m, WarningThreshold = 0.85m, CriticalThreshold = 0.70m, Direction = KpiDirection.HigherIsBetter, IsActive = true, ModifiedDate = now },
        };
        db.KpiDefinitions.AddRange(kpis);
        await db.SaveChangesAsync(ct);
        foreach (var k in kpis) db.KpiDefinitionAuditLogs.Add(KpiDefinitionAuditService.RecordCreate(k, "demo-seed"));
        rows += kpis.Length;

        // KPI values — one latest value per KPI for the last week.
        var weekStart = DateTime.SpecifyKind(today.AddDays(-7), DateTimeKind.Utc);
        var weekEnd = DateTime.SpecifyKind(today, DateTimeKind.Utc);
        foreach (var kpi in kpis)
        {
            decimal sampleValue = kpi.Code switch
            {
                "DEMO-KPI-OEE" => 0.72m,
                "DEMO-KPI-AVAIL" => 0.88m,
                "DEMO-KPI-YIELD" => 0.97m,
                "DEMO-KPI-THRU" => 1320m,
                "DEMO-KPI-MTBF" => 175m,
                "DEMO-KPI-PMCOMP" => 0.92m,
                _ => 0m,
            };
            KpiStatus status = (kpi.Direction == KpiDirection.HigherIsBetter)
                ? (sampleValue < kpi.CriticalThreshold ? KpiStatus.Critical
                    : sampleValue < kpi.WarningThreshold ? KpiStatus.Warning
                    : KpiStatus.OnTarget)
                : (sampleValue > kpi.CriticalThreshold ? KpiStatus.Critical
                    : sampleValue > kpi.WarningThreshold ? KpiStatus.Warning
                    : KpiStatus.OnTarget);
            db.KpiValues.Add(new KpiValue
            {
                KpiDefinitionId = kpi.Id,
                PeriodKind = PerformancePeriodKind.Week,
                PeriodStart = weekStart,
                PeriodEnd = weekEnd,
                Value = sampleValue,
                TargetAtPeriod = kpi.TargetValue,
                Status = status,
                ComputedAt = now,
                ModifiedDate = now,
            });
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Scorecards.
        var pmScorecard = new ScorecardDefinition { Code = "DEMO-SC-PLANT", Name = "Plant Manager Dashboard", Description = "Top-level plant performance at a glance.", OwnerUserId = "demo-seed", IsActive = true, ModifiedDate = now };
        var maintScorecard = new ScorecardDefinition { Code = "DEMO-SC-MAINT", Name = "Maintenance Lead Dashboard", Description = "Reliability + PM compliance.", OwnerUserId = "demo-seed", IsActive = true, ModifiedDate = now };
        db.ScorecardDefinitions.AddRange(pmScorecard, maintScorecard);
        await db.SaveChangesAsync(ct);
        db.ScorecardDefinitionAuditLogs.Add(ScorecardDefinitionAuditService.RecordCreate(pmScorecard, "demo-seed"));
        db.ScorecardDefinitionAuditLogs.Add(ScorecardDefinitionAuditService.RecordCreate(maintScorecard, "demo-seed"));
        rows += 2;

        var plantKpiCodes = new[] { "DEMO-KPI-OEE", "DEMO-KPI-AVAIL", "DEMO-KPI-YIELD", "DEMO-KPI-THRU" };
        var maintKpiCodes = new[] { "DEMO-KPI-MTBF", "DEMO-KPI-PMCOMP", "DEMO-KPI-AVAIL" };
        for (var i = 0; i < plantKpiCodes.Length; i++)
        {
            var kpi = kpis.First(k => k.Code == plantKpiCodes[i]);
            db.ScorecardKpis.Add(new ScorecardKpi { ScorecardDefinitionId = pmScorecard.Id, KpiDefinitionId = kpi.Id, DisplayOrder = (i + 1) * 10, Visual = ScorecardKpiVisual.KpiCard, ModifiedDate = now });
            rows++;
        }
        for (var i = 0; i < maintKpiCodes.Length; i++)
        {
            var kpi = kpis.First(k => k.Code == maintKpiCodes[i]);
            db.ScorecardKpis.Add(new ScorecardKpi { ScorecardDefinitionId = maintScorecard.Id, KpiDefinitionId = kpi.Id, DisplayOrder = (i + 1) * 10, Visual = ScorecardKpiVisual.KpiCard, ModifiedDate = now });
            rows++;
        }
        await db.SaveChangesAsync(ct);

        // Reports.
        var reports = new[]
        {
            new PerformanceReport { Code = "DEMO-RPT-OEE-WEEK", Name = "Weekly OEE by station", Description = "OEE for all stations for the last 7 days.", Kind = PerformanceReportKind.OeeSummary, DefinitionJson = "{\"range\":\"last7\"}", IsActive = true, ModifiedDate = now },
            new PerformanceReport { Code = "DEMO-RPT-MAINT-SC", Name = "Monthly maintenance scorecard", Description = "MTBF / MTTR / availability per asset for the month.", Kind = PerformanceReportKind.MaintenanceScorecard, DefinitionJson = "{\"range\":\"lastMonth\"}", IsActive = true, ModifiedDate = now },
        };
        db.PerformanceReports.AddRange(reports);
        await db.SaveChangesAsync(ct);
        foreach (var r in reports) db.PerformanceReportAuditLogs.Add(PerformanceReportAuditService.RecordCreate(r, "demo-seed"));
        rows += reports.Length;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Rows} performance demo rows.", rows);
        return rows;
    }
}

public sealed class DemoSeedResult
{
    public bool Skipped { get; set; }
    public int Workforce { get; set; }
    public int Engineering { get; set; }
    public int Maintenance { get; set; }
    public int Performance { get; set; }
    public int Total => Workforce + Engineering + Maintenance + Performance;
}
