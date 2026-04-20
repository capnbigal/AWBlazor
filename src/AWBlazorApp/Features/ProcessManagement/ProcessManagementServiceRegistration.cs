using AWBlazorApp.Features.ProcessManagement.Services;

namespace AWBlazorApp.Features.ProcessManagement;

public static class ProcessManagementServiceRegistration
{
    public static IServiceCollection AddProcessManagementServices(this IServiceCollection services)
    {
        services.AddTransient<ProcessSchedulerJob>();
        return services;
    }
}
