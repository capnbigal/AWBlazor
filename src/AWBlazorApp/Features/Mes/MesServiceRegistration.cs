using AWBlazorApp.Features.Mes.Instructions.Application.Services;
using AWBlazorApp.Features.Mes.Runs.Application.Services;

namespace AWBlazorApp.Features.Mes;

public static class MesServiceRegistration
{
    public static IServiceCollection AddMesServices(this IServiceCollection services)
    {
        services.AddScoped<IProductionRunService, ProductionRunService>();
        services.AddScoped<IWorkInstructionRevisionService, WorkInstructionRevisionService>();
        return services;
    }
}
