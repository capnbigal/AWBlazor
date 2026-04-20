using AWBlazorApp.Features.Logistics.Services;

namespace AWBlazorApp.Features.Logistics;

public static class LogisticsServiceRegistration
{
    public static IServiceCollection AddLogisticsServices(this IServiceCollection services)
    {
        services.AddScoped<ILogisticsPostingService, LogisticsPostingService>();
        return services;
    }
}
