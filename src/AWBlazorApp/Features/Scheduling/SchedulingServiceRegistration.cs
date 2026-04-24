using AWBlazorApp.Features.Scheduling.Rules.Application;
using AWBlazorApp.Features.Scheduling.Services;
using AWBlazorApp.Features.Scheduling.WeeklyPlans.Application;

namespace AWBlazorApp.Features.Scheduling;

/// <summary>
/// Feature-level DI composition for Scheduling slice 1. Called from AddFeatureServices.
/// </summary>
/// <remarks>
/// All scheduling services are singletons — the dispatcher accepts the DbContext as a call
/// parameter (not a ctor dependency), the evaluator uses IDbContextFactory to mint its own,
/// and action implementations are stateless. See <see cref="SchedulingDispatchInterceptor"/>
/// for the DI-cycle rationale (IServiceProvider indirection for dispatcher resolution).
///
/// <see cref="SchedulingDispatchInterceptor"/> itself is registered in
/// <c>AddApplicationDatabase</c> because it must be on the DbContext options' interceptor
/// chain at factory-configuration time.
/// </remarks>
public static class SchedulingServiceRegistration
{
    public static IServiceCollection AddSchedulingServices(this IServiceCollection services)
    {
        services.AddSingleton<IFrozenWindowEvaluator, FrozenWindowEvaluator>();
        services.AddSingleton<ISchedulingRuleResolver, SchedulingRuleResolver>();
        services.AddSingleton<ISchedulingDispatcher, SchedulingDispatcher>();
        services.AddSingleton<IRecalcAction, SoftResortAction>();
        services.AddSingleton<IRecalcAction, AlertOnlyAction>();
        services.AddSingleton<IRecalcAction, HardReplanAction>();
        services.AddSingleton<IWeeklyPlanGenerator, WeeklyPlanGenerator>();
        return services;
    }
}
