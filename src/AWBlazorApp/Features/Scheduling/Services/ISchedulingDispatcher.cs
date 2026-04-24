using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
namespace AWBlazorApp.Features.Scheduling.Services;

public interface ISchedulingDispatcher
{
    Task OnSalesOrderCreatedAsync(SalesOrderHeader soh, short locationId, CancellationToken ct);
    bool IsDispatching { get; }
}
