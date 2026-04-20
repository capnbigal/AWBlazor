using AWBlazorApp.Features.Engineering.Deviations.Application.Services;
using AWBlazorApp.Features.Engineering.Ecos.Application.Services;

namespace AWBlazorApp.Features.Engineering;

public static class EngineeringServiceRegistration
{
    public static IServiceCollection AddEngineeringServices(this IServiceCollection services)
    {
        services.AddScoped<IEcoService, EcoService>();
        services.AddScoped<IDeviationService, DeviationService>();
        return services;
    }
}
