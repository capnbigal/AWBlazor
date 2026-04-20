using AWBlazorApp.Features.UserGuide.Services;

namespace AWBlazorApp.Features.UserGuide;

public static class UserGuideServiceRegistration
{
    public static IServiceCollection AddUserGuideServices(this IServiceCollection services)
    {
        services.AddSingleton<UserGuideService>();
        return services;
    }
}
