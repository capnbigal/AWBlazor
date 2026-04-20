using AWBlazorApp.Features.Admin.Services;

namespace AWBlazorApp.Features.Admin;

public static class AdminServiceRegistration
{
    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<DemoDataSeeder>();
        services.AddScoped<DemoDataFiller>();
        services.AddScoped<AdventureWorksDateShifter>();
        services.AddScoped<IPermissionService, PermissionService>();
        return services;
    }
}
