using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 

namespace AWBlazorApp.Features.Performance.Kpis.Application.Services;

/// <summary>
/// Evaluates a <see cref="KpiDefinition"/> against persisted metrics for a given period,
/// writes a <see cref="KpiValue"/> row (upsert), and derives its <see cref="KpiStatus"/>
/// from the KPI's thresholds.
/// </summary>
public interface IKpiEvaluationService
{
    Task<KpiValue> EvaluateAsync(
        int kpiDefinitionId,
        PerformancePeriodKind periodKind,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken);
}
