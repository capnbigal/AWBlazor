using AWBlazorApp.Features.Performance.Jobs;
using AWBlazorApp.Features.Performance.Kpis.Application.Services;
using AWBlazorApp.Features.Performance.MaintenanceMetrics.Application.Services;
using AWBlazorApp.Features.Performance.Oee.Application.Services;
using AWBlazorApp.Features.Performance.ProductionMetrics.Application.Services;
using AWBlazorApp.Features.Performance.Reports.Application.Services;

namespace AWBlazorApp.Features.Performance;

public static class PerformanceServiceRegistration
{
    public static IServiceCollection AddPerformanceServices(this IServiceCollection services)
    {
        services.AddScoped<IOeeService, OeeService>();
        services.AddScoped<IProductionMetricsService, ProductionMetricsService>();
        services.AddScoped<IMaintenanceMetricsService, MaintenanceMetricsService>();
        services.AddScoped<IKpiEvaluationService, KpiEvaluationService>();
        services.AddScoped<IPerformanceReportRunner, PerformanceReportRunner>();
        services.AddScoped<MetricsRollupJob>();
        return services;
    }
}
