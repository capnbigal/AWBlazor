using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
namespace AWBlazorApp.Features.Scheduling.Services;

public interface IFrozenWindowEvaluator
{
    Task<bool> EvaluateAsync(SalesOrderHeader soh, DateTime nowUtc, short locationId, CancellationToken ct = default);
}
