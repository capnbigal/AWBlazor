using AWBlazorApp.Features.Inventory.Services;

namespace AWBlazorApp.Features.Inventory;

public static class InventoryServiceRegistration
{
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.AddTransient<InventoryOutboxEmitterJob>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddSingleton<IInventoryOutboxPublisher, LoggingInventoryOutboxPublisher>();
        services.AddScoped<IProductInsightsService, ProductInsightsService>();
        return services;
    }
}
