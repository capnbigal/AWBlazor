using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
namespace AWBlazorApp.Features.Scheduling.Services;

public interface ISchedulingDispatcher
{
    Task OnSalesOrderCreatedAsync(SalesOrderHeader soh, short locationId, ApplicationDbContext db, CancellationToken ct);
    bool IsDispatching { get; }
}
