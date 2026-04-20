using AWBlazorApp.Features.Workforce.LeaveRequests.Application.Services;
using AWBlazorApp.Features.Workforce.Qualifications.Application.Hooks;
using AWBlazorApp.Features.Workforce.Qualifications.Application.Services;
using AWBlazorApp.Shared.Services;

namespace AWBlazorApp.Features.Workforce;

public static class WorkforceServiceRegistration
{
    public static IServiceCollection AddWorkforceServices(this IServiceCollection services)
    {
        // Workforce's trigger hook. Must be registered after Quality's hook to preserve
        // the existing IEnumerable<IPostingTriggerHook> resolution order.
        services.AddScoped<IPostingTriggerHook, QualificationCheckHook>();

        services.AddScoped<IQualificationService, QualificationService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        return services;
    }
}
