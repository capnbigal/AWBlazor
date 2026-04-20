using AWBlazorApp.Features.Maintenance.PmSchedules.Application.Services;
using AWBlazorApp.Features.Maintenance.WorkOrders.Application.Services;

namespace AWBlazorApp.Features.Maintenance;

public static class MaintenanceServiceRegistration
{
    public static IServiceCollection AddMaintenanceServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IPmScheduleService, PmScheduleService>();
        return services;
    }
}
