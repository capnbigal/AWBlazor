using AWBlazorApp.Shared.Services;
using AWBlazorApp.Shared.UI.Components;
using AWBlazorApp.Shared.Validation;
using FluentValidation;

namespace AWBlazorApp.Shared;

/// <summary>
/// Registers Shared-owned cross-cutting services consumed by multiple features:
/// lookup cache, analytics cache, distinct-values provider, and FluentValidation infrastructure.
/// </summary>
public static class SharedServiceRegistration
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<AnalyticsCacheService>();
        services.AddSingleton<LookupService>();
        services.AddScoped<IDistinctValuesProvider, DistinctValuesProvider>();

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddTransient(typeof(MudFormValidator<>));

        return services;
    }
}
