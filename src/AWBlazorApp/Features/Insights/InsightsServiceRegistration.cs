using AWBlazorApp.Features.Insights.Services;

namespace AWBlazorApp.Features.Insights;

public static class InsightsServiceRegistration
{
    public static IServiceCollection AddInsightsServices(this IServiceCollection services)
    {
        services.AddSingleton<NotificationService>();
        services.AddScoped<NotificationRuleEvaluator>();
        services.AddScoped<SavedQueryRunner>();
        services.AddScoped<KpiSnapshotJob>();
        services.AddScoped<ReportDispatcher>();
        services.AddScoped<ReportScheduleRegistry>();
        return services;
    }
}
