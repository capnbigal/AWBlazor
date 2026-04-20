using AWBlazorApp.Features.Dashboard.Services;

namespace AWBlazorApp.Features.Dashboard;

public static class DashboardServiceRegistration
{
    public static IServiceCollection AddDashboardServices(this IServiceCollection services)
    {
        services.AddScoped<IPlantDashboardService, PlantDashboardService>();
        return services;
    }
}
