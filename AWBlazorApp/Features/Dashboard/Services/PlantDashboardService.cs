using AWBlazorApp.Features.Dashboard.Dtos;
using AWBlazorApp.Features.Engineering.Domain;
using AWBlazorApp.Features.Maintenance.Domain;
using AWBlazorApp.Features.Mes.Domain;
using AWBlazorApp.Features.Performance.Domain;
using AWBlazorApp.Features.Quality.Domain;
using AWBlazorApp.Features.Workforce.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AWBlazorApp.Features.Dashboard.Services;

public sealed class PlantDashboardService : IPlantDashboardService
{
    private const string CacheKey = "plant-dashboard:v1";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private const int ActivityFeedSize = 20;

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PlantDashboardService> _logger;

    public PlantDashboardService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IMemoryCache cache,
        ILogger<PlantDashboardService> logger)
    {
        _dbFactory = dbFactory;
        _cache = cache;
        _logger = logger;
    }

    public void Invalidate() => _cache.Remove(CacheKey);

    public async Task<PlantDashboardDto> GetAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<PlantDashboardDto>(CacheKey, out var cached) && cached is not null)
            return cached;

        var dto = await BuildAsync(cancellationToken);

        _cache.Set(CacheKey, dto, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
        return dto;
    }

    private async Task<PlantDashboardDto> BuildAsync(CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var nowUtc = DateTime.UtcNow;
        var todayUtc = nowUtc.Date;
        var sevenDaysAgo = todayUtc.AddDays(-7);
        var todayDate = DateOnly.FromDateTime(todayUtc);

        // ── Headline KPIs ──────────────────────────────────────────────────
        var latestOee = await db.OeeSnapshots.AsNoTracking()
            .OrderByDescending(s => s.PeriodStart)
            .Select(s => (decimal?)s.Oee)
            .FirstOrDefaultAsync(ct);

        var activeRuns = await db.ProductionRuns.AsNoTracking()
            .CountAsync(r => r.Status == ProductionRunStatus.InProgress, ct);

        var openWoCount = await db.MaintenanceWorkOrders.AsNoTracking()
            .CountAsync(w => w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled, ct);

        var openQualAlerts = await db.QualificationAlerts.AsNoTracking()
            .CountAsync(a => a.Status == QualificationAlertStatus.Open, ct);

        var openNcrs = await db.NonConformances.AsNoTracking()
            .CountAsync(n => n.Status != NonConformanceStatus.Closed, ct);

        var ecosUnderReview = await db.EngineeringChangeOrders.AsNoTracking()
            .CountAsync(e => e.Status == EcoStatus.UnderReview, ct);

        var pendingLeaves = await db.LeaveRequests.AsNoTracking()
            .CountAsync(l => l.Status == LeaveStatus.Pending, ct);

        var activeAnnouncements = await db.Announcements.AsNoTracking()
            .CountAsync(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > nowUtc), ct);

        var headlines = new PlantHeadlineKpisDto(
            latestOee, activeRuns, openWoCount, openQualAlerts, openNcrs,
            ecosUnderReview, pendingLeaves, activeAnnouncements);

        // ── Critical alerts ────────────────────────────────────────────────
        var criticalAlerts = new List<CriticalAlertDto>();

        var criticalAnnouncements = await db.Announcements.AsNoTracking()
            .Where(a => a.IsActive && a.Severity == AnnouncementSeverity.Critical
                     && (a.ExpiresAt == null || a.ExpiresAt > nowUtc))
            .OrderByDescending(a => a.PublishedAt)
            .Select(a => new { a.Title, a.Body, a.Id })
            .Take(5)
            .ToListAsync(ct);
        foreach (var a in criticalAnnouncements)
            criticalAlerts.Add(new CriticalAlertDto("Error", "Workforce", a.Title,
                a.Body.Length > 120 ? a.Body[..120] + "…" : a.Body,
                "workforce/announcements"));

        var criticalWos = await db.MaintenanceWorkOrders.AsNoTracking()
            .Where(w => w.Priority == WorkOrderPriority.Critical
                     && w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled)
            .OrderByDescending(w => w.RaisedAt)
            .Select(w => new { w.Id, w.WorkOrderNumber, w.Title })
            .Take(5)
            .ToListAsync(ct);
        foreach (var w in criticalWos)
            criticalAlerts.Add(new CriticalAlertDto("Error", "Maintenance", $"Critical WO {w.WorkOrderNumber}",
                w.Title, $"maintenance/work-orders/{w.Id}"));

        var criticalKpis = await db.KpiValues.AsNoTracking()
            .Where(v => v.Status == KpiStatus.Critical)
            .OrderByDescending(v => v.PeriodStart)
            .Take(5)
            .Join(db.KpiDefinitions.AsNoTracking(), v => v.KpiDefinitionId, k => k.Id,
                (v, k) => new { k.Code, k.Name, v.Value })
            .ToListAsync(ct);
        foreach (var k in criticalKpis)
            criticalAlerts.Add(new CriticalAlertDto("Warning", "Performance", $"KPI {k.Code} critical",
                $"{k.Name} = {(k.Value?.ToString("F2") ?? "—")}", "performance/kpis"));

        // ── Module health cards ────────────────────────────────────────────
        var moduleHealth = await BuildModuleHealthAsync(db, nowUtc, todayDate, ct);

        // ── Recent activity (~last N events across modules) ───────────────
        var recentActivity = await BuildRecentActivityAsync(db, sevenDaysAgo, ct);

        // ── Trends (last 7 full days, including today's partial) ──────────
        // Pull raw rows to memory and group there — EF Core can't translate `.Date` on a
        // DateTime in a GroupBy/Select against SQL Server, and the row count for a 7-day
        // window is small (a few hundred rows at most).
        var sevenDaysAgoDate = DateOnly.FromDateTime(sevenDaysAgo);

        var oeeRaw = await db.OeeSnapshots.AsNoTracking()
            .Where(s => s.PeriodKind == PerformancePeriodKind.Day && s.PeriodStart >= sevenDaysAgo)
            .Select(s => new { s.PeriodStart, s.Oee })
            .ToListAsync(ct);
        var oeeTrend = oeeRaw
            .GroupBy(s => DateOnly.FromDateTime(s.PeriodStart))
            .Select(g => new TrendPointDto(g.Key, g.Average(x => x.Oee)))
            .OrderBy(p => p.Date)
            .ToList();

        var prodRaw = await db.ProductionDailyMetrics.AsNoTracking()
            .Where(m => m.Date >= sevenDaysAgoDate)
            .Select(m => new { m.Date, m.UnitsProduced })
            .ToListAsync(ct);
        var prodTrend = prodRaw
            .GroupBy(m => m.Date)
            .Select(g => new TrendPointDto(g.Key, g.Sum(x => x.UnitsProduced)))
            .OrderBy(p => p.Date)
            .ToList();

        var raisedRaw = await db.MaintenanceWorkOrders.AsNoTracking()
            .Where(w => w.RaisedAt >= sevenDaysAgo)
            .Select(w => w.RaisedAt)
            .ToListAsync(ct);
        var raisedByDay = raisedRaw
            .GroupBy(d => d.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToList();

        var completedRaw = await db.MaintenanceWorkOrders.AsNoTracking()
            .Where(w => w.CompletedAt != null && w.CompletedAt >= sevenDaysAgo)
            .Select(w => w.CompletedAt!.Value)
            .ToListAsync(ct);
        var completedByDay = completedRaw
            .GroupBy(d => d.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToList();

        var woTrend = Enumerable.Range(0, 8)
            .Select(offset =>
            {
                var d = todayUtc.AddDays(-7 + offset);
                var raised = raisedByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0;
                var completed = completedByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0;
                return new WorkOrderTrendDto(DateOnly.FromDateTime(d), raised, completed);
            })
            .ToList();

        sw.Stop();
        _logger.LogInformation("PlantDashboard built in {Ms}ms", sw.ElapsedMilliseconds);

        return new PlantDashboardDto(
            GeneratedAt: nowUtc,
            Headlines: headlines,
            CriticalAlerts: criticalAlerts,
            ModuleHealth: moduleHealth,
            RecentActivity: recentActivity,
            OeeTrend7d: oeeTrend,
            ProductionTrend7d: prodTrend,
            WorkOrderTrend7d: woTrend);
    }

    private async Task<List<ModuleHealthDto>> BuildModuleHealthAsync(
        ApplicationDbContext db, DateTime nowUtc, DateOnly today, CancellationToken ct)
    {
        // Use plain Material icon path constants — keep these in sync with NavMenu.
        const string iconWorkforce = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z\"/>";
        const string iconEngineering = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M5 4v3h5.5v12h3V7H19V4z\"/>";
        const string iconMaintenance = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M22.7 19l-9.1-9.1c.9-2.3.4-5-1.5-6.9-2-2-5-2.4-7.4-1.3L9 6 6 9 1.6 4.7C.4 7.1.9 10.1 2.9 12.1c1.9 1.9 4.6 2.4 6.9 1.5l9.1 9.1c.4.4 1 .4 1.4 0l2.3-2.3c.5-.4.5-1.1.1-1.4z\"/>";
        const string iconPerformance = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M3.5 18.49l6-6.01 4 4L22 6.92l-1.41-1.41-7.09 7.97-4-4L2 16.99z\"/>";
        const string iconQuality = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z\"/>";
        const string iconMes = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 14.5v-9l6 4.5-6 4.5z\"/>";
        const string iconLogistics = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M20 8h-3V4H3c-1.1 0-2 .9-2 2v11h2c0 1.66 1.34 3 3 3s3-1.34 3-3h6c0 1.66 1.34 3 3 3s3-1.34 3-3h2v-5l-3-4zM6 18.5c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zm12 0c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5z\"/>";
        const string iconInventory = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M20 2H4c-1 0-2 .9-2 2v3.01c0 .72.43 1.34 1 1.69V20c0 1.1 1.1 2 2 2h14c.9 0 2-.9 2-2V8.7c.57-.35 1-.97 1-1.69V4c0-1.1-1-2-2-2zm-5 12H9v-2h6v2zm5-7H4V4l16-.02V7z\"/>";
        const string iconEnterprise = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M12 7V3H2v18h20V7H12zM6 19H4v-2h2v2zm0-4H4v-2h2v2zm0-4H4V9h2v2zm0-4H4V5h2v2zm4 12H8v-2h2v2zm0-4H8v-2h2v2zm0-4H8V9h2v2zm0-4H8V5h2v2zm10 12h-8v-2h2v-2h-2v-2h2v-2h-2V9h8v10zm-2-8h-2v2h2v-2zm0 4h-2v2h2v-2z\"/>";

        // Workforce
        var attendanceTodayPresent = await db.AttendanceEvents.AsNoTracking()
            .Where(a => a.ShiftDate == today && a.Status == AttendanceStatus.Present)
            .CountAsync(ct);
        var workforceCriticality = await db.QualificationAlerts.AsNoTracking()
            .CountAsync(a => a.Status == QualificationAlertStatus.Open, ct);
        var workforcePendingLeaves = await db.LeaveRequests.AsNoTracking()
            .CountAsync(l => l.Status == LeaveStatus.Pending, ct);

        // Engineering
        var ecosDraft = await db.EngineeringChangeOrders.AsNoTracking()
            .CountAsync(e => e.Status == EcoStatus.Draft, ct);
        var ecosReview = await db.EngineeringChangeOrders.AsNoTracking()
            .CountAsync(e => e.Status == EcoStatus.UnderReview, ct);
        var activeBoms = await db.BomHeaders.AsNoTracking()
            .CountAsync(b => b.IsActive, ct);
        var pendingDeviations = await db.DeviationRequests.AsNoTracking()
            .CountAsync(d => d.Status == DeviationStatus.Pending, ct);

        // Maintenance
        var woInProgress = await db.MaintenanceWorkOrders.AsNoTracking()
            .CountAsync(w => w.Status == WorkOrderStatus.InProgress, ct);
        var woScheduled = await db.MaintenanceWorkOrders.AsNoTracking()
            .CountAsync(w => w.Status == WorkOrderStatus.Scheduled, ct);
        var pmsActive = await db.PmSchedules.AsNoTracking()
            .CountAsync(p => p.IsActive, ct);

        // Performance
        var kpisOnTarget = await db.KpiValues.AsNoTracking()
            .CountAsync(v => v.Status == KpiStatus.OnTarget, ct);
        var kpisWarning = await db.KpiValues.AsNoTracking()
            .CountAsync(v => v.Status == KpiStatus.Warning, ct);
        var kpisCritical = await db.KpiValues.AsNoTracking()
            .CountAsync(v => v.Status == KpiStatus.Critical, ct);

        // Quality
        var inspectionsPending = await db.Inspections.AsNoTracking()
            .CountAsync(i => i.Status == InspectionStatus.Pending, ct);
        var inspectionsInProgress = await db.Inspections.AsNoTracking()
            .CountAsync(i => i.Status == InspectionStatus.InProgress, ct);
        var ncrsOpen = await db.NonConformances.AsNoTracking()
            .CountAsync(n => n.Status != NonConformanceStatus.Closed, ct);
        var capaActive = await db.CapaCases.AsNoTracking()
            .CountAsync(c => c.Status != CapaStatus.Closed, ct);

        // MES
        var runsInProgress = await db.ProductionRuns.AsNoTracking()
            .CountAsync(r => r.Status == ProductionRunStatus.InProgress, ct);
        var runsCompletedToday = await db.ProductionRuns.AsNoTracking()
            .CountAsync(r => r.ActualEndAt != null && r.ActualEndAt >= nowUtc.Date, ct);
        var openDowntime = await db.DowntimeEvents.AsNoTracking()
            .CountAsync(d => d.EndAt == null, ct);

        // Logistics — last 24h
        var since24h = nowUtc.AddHours(-24);
        var receiptsToday = await db.GoodsReceipts.AsNoTracking()
            .CountAsync(r => r.ReceivedAt >= since24h, ct);
        var shipmentsToday = await db.Shipments.AsNoTracking()
            .CountAsync(s => s.ShippedAt != null && s.ShippedAt >= since24h, ct);

        // Inventory
        var inventoryItems = await db.InventoryItems.AsNoTracking()
            .CountAsync(i => i.IsActive, ct);
        var transactionsToday = await db.InventoryTransactions.AsNoTracking()
            .CountAsync(t => t.PostedAt >= since24h, ct);

        // Enterprise
        var orgUnits = await db.OrgUnits.AsNoTracking().CountAsync(ct);
        var stations = await db.Stations.AsNoTracking().CountAsync(s => s.IsActive, ct);
        var assets = await db.Assets.AsNoTracking()
            .CountAsync(a => a.Status == AWBlazorApp.Features.Enterprise.Domain.AssetStatus.Active, ct);

        // ── 30-day per-module trends ─────────────────────────────────────────
        // Pull only the date column from each table for the trailing 30 days, then bin in
        // memory. Cheap because the per-module windows are small and the dashboard is
        // cached 5 min — most page loads don't run these queries at all.
        var thirtyDaysAgo = nowUtc.AddDays(-30);
        var enterpriseEmpty = new int[30]; // Enterprise has no obvious time series; show flat.

        var attendance30 = BinByDay(await db.AttendanceEvents.AsNoTracking()
            .Where(a => a.ShiftDate >= DateOnly.FromDateTime(thirtyDaysAgo))
            .Select(a => a.ShiftDate.ToDateTime(TimeOnly.MinValue))
            .ToListAsync(ct), thirtyDaysAgo);

        var ecos30 = BinByDay(await db.EngineeringChangeOrders.AsNoTracking()
            .Where(e => e.RaisedAt >= thirtyDaysAgo).Select(e => e.RaisedAt).ToListAsync(ct), thirtyDaysAgo);

        var wos30 = BinByDay(await db.MaintenanceWorkOrders.AsNoTracking()
            .Where(w => w.RaisedAt >= thirtyDaysAgo).Select(w => w.RaisedAt).ToListAsync(ct), thirtyDaysAgo);

        var oeeSnap30 = BinByDay(await db.OeeSnapshots.AsNoTracking()
            .Where(s => s.PeriodKind == PerformancePeriodKind.Day && s.PeriodStart >= thirtyDaysAgo)
            .Select(s => s.PeriodStart).ToListAsync(ct), thirtyDaysAgo);

        var ncrs30 = BinByDay(await db.NonConformances.AsNoTracking()
            .Where(n => n.ModifiedDate >= thirtyDaysAgo).Select(n => n.ModifiedDate).ToListAsync(ct), thirtyDaysAgo);

        var runs30 = BinByDay(await db.ProductionRuns.AsNoTracking()
            .Where(r => r.ActualStartAt != null && r.ActualStartAt >= thirtyDaysAgo)
            .Select(r => r.ActualStartAt!.Value).ToListAsync(ct), thirtyDaysAgo);

        var receipts30 = BinByDay(await db.GoodsReceipts.AsNoTracking()
            .Where(r => r.ReceivedAt >= thirtyDaysAgo).Select(r => r.ReceivedAt).ToListAsync(ct), thirtyDaysAgo);

        var invTxns30 = BinByDay(await db.InventoryTransactions.AsNoTracking()
            .Where(t => t.PostedAt >= thirtyDaysAgo).Select(t => t.PostedAt).ToListAsync(ct), thirtyDaysAgo);

        return new List<ModuleHealthDto>
        {
            new("Workforce", iconWorkforce, "workforce", "Training, qualifications, attendance, comms",
                new[]
                {
                    new MiniStatDto("Present today", attendanceTodayPresent.ToString("N0"), "Success", "workforce/attendance"),
                    new MiniStatDto("Open alerts", workforceCriticality.ToString("N0"), workforceCriticality > 0 ? "Error" : null, "workforce/qualification-alerts"),
                    new MiniStatDto("Pending leave", workforcePendingLeaves.ToString("N0"), workforcePendingLeaves > 0 ? "Warning" : null, "workforce/leave-requests"),
                },
                attendance30, "Attendance / day"),

            new("Engineering", iconEngineering, "engineering", "ECOs, routings, BOMs, deviations",
                new[]
                {
                    new MiniStatDto("Draft ECOs", ecosDraft.ToString("N0"), null, "engineering/ecos?status=Draft"),
                    new MiniStatDto("Under review", ecosReview.ToString("N0"), ecosReview > 0 ? "Warning" : null, "engineering/ecos?status=UnderReview"),
                    new MiniStatDto("Active BOMs", activeBoms.ToString("N0"), null, "engineering/boms"),
                    new MiniStatDto("Pending deviations", pendingDeviations.ToString("N0"), pendingDeviations > 0 ? "Warning" : null, "engineering/deviations"),
                },
                ecos30, "ECOs raised / day"),

            new("Maintenance", iconMaintenance, "maintenance", "Work orders, PM schedules, spares",
                new[]
                {
                    new MiniStatDto("In progress", woInProgress.ToString("N0"), woInProgress > 0 ? "Warning" : null, "maintenance/work-orders?status=InProgress"),
                    new MiniStatDto("Scheduled", woScheduled.ToString("N0"), null, "maintenance/work-orders?status=Scheduled"),
                    new MiniStatDto("Active PMs", pmsActive.ToString("N0"), null, "maintenance/pm-schedules"),
                },
                wos30, "WOs raised / day"),

            new("Performance", iconPerformance, "performance", "OEE, KPIs, scorecards",
                new[]
                {
                    new MiniStatDto("KPIs on target", kpisOnTarget.ToString("N0"), "Success", "performance/kpis"),
                    new MiniStatDto("KPIs warning", kpisWarning.ToString("N0"), kpisWarning > 0 ? "Warning" : null, "performance/kpis"),
                    new MiniStatDto("KPIs critical", kpisCritical.ToString("N0"), kpisCritical > 0 ? "Error" : null, "performance/kpis"),
                },
                oeeSnap30, "OEE snapshots / day"),

            new("Quality", iconQuality, "quality", "Inspections, NCRs, CAPA",
                new[]
                {
                    new MiniStatDto("Pending insp", inspectionsPending.ToString("N0"), null, "quality/inspections?status=Pending"),
                    new MiniStatDto("In-progress insp", inspectionsInProgress.ToString("N0"), inspectionsInProgress > 0 ? "Info" : null, "quality/inspections?status=InProgress"),
                    new MiniStatDto("Open NCRs", ncrsOpen.ToString("N0"), ncrsOpen > 0 ? "Error" : null, "quality/ncrs"),
                    new MiniStatDto("Active CAPA", capaActive.ToString("N0"), capaActive > 0 ? "Warning" : null, "quality/capa"),
                },
                ncrs30, "NCR activity / day"),

            new("Production exec.", iconMes, "mes", "Production runs, downtime, OEE",
                new[]
                {
                    new MiniStatDto("Runs in progress", runsInProgress.ToString("N0"), runsInProgress > 0 ? "Info" : null, "mes/runs?status=InProgress"),
                    new MiniStatDto("Completed today", runsCompletedToday.ToString("N0"), null, "mes/runs?status=Completed"),
                    new MiniStatDto("Open downtime", openDowntime.ToString("N0"), openDowntime > 0 ? "Error" : null, "mes/downtime"),
                },
                runs30, "Runs started / day"),

            new("Logistics", iconLogistics, "logistics", "Receipts, shipments, transfers",
                new[]
                {
                    new MiniStatDto("Receipts (24h)", receiptsToday.ToString("N0"), null, "logistics/receipts"),
                    new MiniStatDto("Shipments (24h)", shipmentsToday.ToString("N0"), null, "logistics/shipments"),
                },
                receipts30, "Receipts / day"),

            new("Inventory", iconInventory, "inventory", "Items, balances, transactions",
                new[]
                {
                    new MiniStatDto("Active items", inventoryItems.ToString("N0"), null, "inventory/items"),
                    new MiniStatDto("Txns (24h)", transactionsToday.ToString("N0"), null, "inventory/transactions"),
                },
                invTxns30, "Inventory txns / day"),

            new("Enterprise", iconEnterprise, "enterprise/tree", "Org tree, stations, assets",
                new[]
                {
                    new MiniStatDto("Org units", orgUnits.ToString("N0"), null, "enterprise/tree"),
                    new MiniStatDto("Active stations", stations.ToString("N0"), null, "enterprise/stations"),
                    new MiniStatDto("Active assets", assets.ToString("N0"), null, "enterprise/assets"),
                },
                enterpriseEmpty, ""),
        };
    }

    /// <summary>
    /// Buckets a sequence of timestamps into 30 daily counts ending at "today" (UTC).
    /// Index 0 = oldest day (29 days ago), index 29 = today. Output length is always 30.
    /// </summary>
    private static int[] BinByDay(IReadOnlyCollection<DateTime> timestamps, DateTime windowStartUtc)
    {
        var bins = new int[30];
        var startDate = windowStartUtc.Date;
        foreach (var ts in timestamps)
        {
            var dayIndex = (ts.Date - startDate).Days;
            if (dayIndex >= 0 && dayIndex < 30) bins[dayIndex]++;
        }
        return bins;
    }

    private async Task<List<ActivityItemDto>> BuildRecentActivityAsync(
        ApplicationDbContext db, DateTime since, CancellationToken ct)
    {
        // Pull the most recent N from each module, then sort the union and trim. Keeps each
        // sub-query small + uses the existing per-table indexes.
        const int perModule = 6;

        var wos = await db.MaintenanceWorkOrders.AsNoTracking()
            .Where(w => w.RaisedAt >= since)
            .OrderByDescending(w => w.RaisedAt)
            .Select(w => new ActivityItemDto(w.RaisedAt, "Maintenance", "WorkOrder",
                w.WorkOrderNumber + " — " + w.Title, w.Status.ToString(),
                "maintenance/work-orders/" + w.Id))
            .Take(perModule)
            .ToListAsync(ct);

        var ecos = await db.EngineeringChangeOrders.AsNoTracking()
            .Where(e => e.RaisedAt >= since)
            .OrderByDescending(e => e.RaisedAt)
            .Select(e => new ActivityItemDto(e.RaisedAt, "Engineering", "ECO",
                e.Code + " — " + e.Title, e.Status.ToString(),
                "engineering/ecos/" + e.Id))
            .Take(perModule)
            .ToListAsync(ct);

        var inspections = await db.Inspections.AsNoTracking()
            .Where(i => i.InspectedAt != null && i.InspectedAt >= since)
            .OrderByDescending(i => i.InspectedAt)
            .Select(i => new ActivityItemDto(i.InspectedAt!.Value, "Quality", "Inspection",
                "Inspection " + i.InspectionNumber, i.Status.ToString(),
                "quality/inspections/" + i.Id))
            .Take(perModule)
            .ToListAsync(ct);

        var ncrs = await db.NonConformances.AsNoTracking()
            .Where(n => n.ModifiedDate >= since)
            .OrderByDescending(n => n.ModifiedDate)
            .Select(n => new ActivityItemDto(n.ModifiedDate, "Quality", "NCR",
                n.NcrNumber, n.Description.Length > 80 ? n.Description.Substring(0, 80) + "…" : n.Description,
                "quality/ncrs/" + n.Id))
            .Take(perModule)
            .ToListAsync(ct);

        var leaves = await db.LeaveRequests.AsNoTracking()
            .Where(l => l.RequestedAt >= since)
            .OrderByDescending(l => l.RequestedAt)
            .Select(l => new ActivityItemDto(l.RequestedAt, "Workforce", "LeaveRequest",
                "Leave for #" + l.BusinessEntityId, l.Status.ToString(),
                "workforce/leave-requests"))
            .Take(perModule)
            .ToListAsync(ct);

        var announcements = await db.Announcements.AsNoTracking()
            .Where(a => a.PublishedAt >= since)
            .OrderByDescending(a => a.PublishedAt)
            .Select(a => new ActivityItemDto(a.PublishedAt, "Workforce", "Announcement",
                a.Title, a.Severity.ToString(),
                "workforce/announcements"))
            .Take(perModule)
            .ToListAsync(ct);

        var runs = await db.ProductionRuns.AsNoTracking()
            .Where(r => r.ActualStartAt != null && r.ActualStartAt >= since)
            .OrderByDescending(r => r.ActualStartAt)
            .Select(r => new ActivityItemDto(r.ActualStartAt!.Value, "Production exec.", "ProductionRun",
                r.RunNumber, r.Status.ToString(),
                "mes/runs/" + r.Id))
            .Take(perModule)
            .ToListAsync(ct);

        return wos.Concat(ecos).Concat(inspections).Concat(ncrs)
                  .Concat(leaves).Concat(announcements).Concat(runs)
                  .OrderByDescending(a => a.At)
                  .Take(ActivityFeedSize)
                  .ToList();
    }
}
