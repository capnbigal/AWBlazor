using AWBlazorApp.Features.Scheduling.Alerts.Api;
using AWBlazorApp.Features.Scheduling.DeliverySchedules.Api;
using AWBlazorApp.Features.Scheduling.LineConfigurations.Api;
using AWBlazorApp.Features.Scheduling.LineProductAssignments.Api;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Api;

namespace AWBlazorApp.Features.Scheduling;

public static class SchedulingEndpoints
{
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapLineConfigurationEndpoints();
        app.MapLineProductAssignmentEndpoints();
        app.MapWeeklyPlanEndpoints();
        app.MapDeliveryScheduleEndpoints();
        app.MapSchedulingAlertEndpoints();
        return app;
    }
}
