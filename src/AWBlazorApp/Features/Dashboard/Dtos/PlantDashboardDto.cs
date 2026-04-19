namespace AWBlazorApp.Features.Dashboard.Dtos;

/// <summary>
/// Single payload for the plant-wide dashboard at /dashboard/plant. Aggregates data from
/// every module schema (wf / eng / maint / perf / qa / mes / lgx / inv / org) so the
/// dashboard page only does one HTTP round-trip and one cache lookup. Fields are scalar /
/// list-of-record so the page can bind directly without re-querying.
/// </summary>
public sealed record PlantDashboardDto(
    DateTime GeneratedAt,
    PlantHeadlineKpisDto Headlines,
    IReadOnlyList<CriticalAlertDto> CriticalAlerts,
    IReadOnlyList<ModuleHealthDto> ModuleHealth,
    IReadOnlyList<ActivityItemDto> RecentActivity,
    IReadOnlyList<TrendPointDto> OeeTrend7d,
    IReadOnlyList<TrendPointDto> ProductionTrend7d,
    IReadOnlyList<WorkOrderTrendDto> WorkOrderTrend7d);

/// <summary>Top-level KPI tiles, one number each. Drives the hero strip at the top of the page.</summary>
public sealed record PlantHeadlineKpisDto(
    decimal? LatestOeeFraction,         // 0-1; null if no snapshots exist
    int ActiveProductionRuns,
    int OpenMaintenanceWorkOrders,
    int OpenQualificationAlerts,
    int OpenNonConformances,
    int EcosUnderReview,
    int PendingLeaveRequests,
    int ActiveAnnouncements);

/// <summary>
/// A single in-your-face alert that needs attention. Examples: critical announcement still
/// active, unresolved high-priority WO, KPI in critical band.
/// </summary>
public sealed record CriticalAlertDto(
    string Severity,    // "Info" / "Warning" / "Error"
    string Source,      // module name e.g. "Workforce", "Maintenance"
    string Title,
    string? Detail,
    string? LinkHref);

/// <summary>One per module — a stack of MiniStat tiles + a "Open" link.</summary>
public sealed record ModuleHealthDto(
    string Name,
    string Icon,        // MudBlazor SVG path string
    string LinkHref,
    string Subtitle,
    IReadOnlyList<MiniStatDto> Stats,
    IReadOnlyList<int> Trend30d,         // count per day for the trailing 30 days; primary metric per module
    string Trend30dLabel);                // e.g. "WOs raised / day"

/// <summary>Single label/value pair shown as a tiny stat inside a ModuleHealth card.</summary>
public sealed record MiniStatDto(
    string Label,
    string Value,
    string? Color,        // MudBlazor color name string ("Success" / "Warning" / "Error" / null = default)
    string? LinkHref = null);   // Optional drill-through to a pre-filtered list page

/// <summary>An entry in the recent-activity feed across all modules.</summary>
public sealed record ActivityItemDto(
    DateTime At,
    string Module,
    string Kind,        // e.g. "WorkOrder", "ECO", "Inspection", "LeaveRequest", "Announcement"
    string Title,
    string? Subtitle,
    string? LinkHref);

/// <summary>One point on a daily trend line.</summary>
public sealed record TrendPointDto(DateOnly Date, decimal Value);

/// <summary>Stacked-pair point: how many WOs raised vs completed on a given day.</summary>
public sealed record WorkOrderTrendDto(DateOnly Date, int Raised, int Completed);
