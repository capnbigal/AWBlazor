using AWBlazorApp.Features.Performance.Domain;

namespace AWBlazorApp.Features.Performance.Services;

/// <summary>
/// Computes OEE (Availability × Performance × Quality) from upstream MES + Quality tables.
/// Writes / upserts <see cref="OeeSnapshot"/> rows for cheap lookup from dashboards.
/// Availability = ActualRuntime / (ActualRuntime + Downtime).
/// Performance   = (UnitsProduced × IdealCycle) / ActualRuntime.
/// Quality       = (UnitsProduced − UnitsScrapped) / UnitsProduced.
/// Missing denominators yield 0; caller decides whether that's "no data" or "zero".
/// </summary>
public interface IOeeService
{
    /// <summary>Computes and persists an OEE snapshot for the given station / period. Idempotent (upsert).</summary>
    Task<OeeSnapshot> ComputeAsync(
        int stationId,
        PerformancePeriodKind periodKind,
        DateTime periodStart,
        DateTime periodEnd,
        decimal idealCycleSeconds,
        CancellationToken cancellationToken);
}
