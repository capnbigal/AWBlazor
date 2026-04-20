using AWBlazorApp.Features.Quality.Capa.Application.Services;
using AWBlazorApp.Features.Quality.Inspections.Application.Services;
using AWBlazorApp.Features.Quality.Ncrs.Application.Services;
using AWBlazorApp.Shared.Services;

namespace AWBlazorApp.Features.Quality;

public static class QualityServiceRegistration
{
    public static IServiceCollection AddQualityServices(this IServiceCollection services)
    {
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<INonConformanceService, NonConformanceService>();
        services.AddScoped<ICapaService, CapaService>();

        // Quality's trigger hook. Registered ahead of Workforce's so the existing resolution
        // order of IEnumerable<IPostingTriggerHook> is preserved.
        services.AddScoped<IPostingTriggerHook, InspectionTriggerHook>();
        return services;
    }
}
