using AWBlazorApp.Features.Processes.Timelines.Application;
using AWBlazorApp.Features.Processes.Timelines.Application.HopQueries;
using AWBlazorApp.Features.Processes.Timelines.Application.RootLabelers;

namespace AWBlazorApp.Features.Processes.Timelines;

public static class ProcessTimelineServiceRegistration
{
    public static IServiceCollection AddProcessTimelineServices(this IServiceCollection services)
    {
        services.AddSingleton<IProcessChainResolver, ProcessChainResolver>();
        services.AddSingleton<IProcessTimelineComposer, ProcessTimelineComposer>();

        services.AddSingleton<IChainHopQuery, ShipmentFromSalesOrderHeader>();
        services.AddSingleton<IChainHopQuery, ShipmentLineFromShipment>();
        services.AddSingleton<IChainHopQuery, GoodsReceiptFromPurchaseOrderHeader>();
        services.AddSingleton<IChainHopQuery, GoodsReceiptLineFromGoodsReceipt>();

        services.AddSingleton<IRootEntityLabeler, SalesOrderHeaderLabeler>();
        services.AddSingleton<IRootEntityLabeler, PurchaseOrderHeaderLabeler>();
        services.AddSingleton<IRootEntityLabeler, ShipmentLabeler>();
        services.AddSingleton<IRootEntityLabeler, GoodsReceiptLabeler>();

        return services;
    }
}
