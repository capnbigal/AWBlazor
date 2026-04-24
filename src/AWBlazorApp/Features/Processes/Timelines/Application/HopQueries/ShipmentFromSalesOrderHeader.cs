using AWBlazorApp.Features.Logistics.Shipments.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.HopQueries;

public class ShipmentFromSalesOrderHeader : IChainHopQuery
{
    public string ParentEntity => "SalesOrderHeader";
    public string ChildEntity => "Shipment";
    public string ForeignKey => "SalesOrderId";

    public async Task<IReadOnlyList<string>> GetChildIdsAsync(
        ApplicationDbContext db, IReadOnlyList<string> parentIds, CancellationToken ct)
    {
        if (parentIds.Count == 0) return Array.Empty<string>();
        var ints = parentIds.Select(int.Parse).ToArray();
        return await db.Set<Shipment>().AsNoTracking()
            .Where(s => s.SalesOrderId != null && ints.Contains(s.SalesOrderId.Value))
            .Select(s => s.Id.ToString())
            .ToListAsync(ct);
    }

    public async Task<string?> GetParentIdAsync(
        ApplicationDbContext db, string childId, CancellationToken ct)
    {
        var id = int.Parse(childId);
        var parentId = await db.Set<Shipment>().AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => s.SalesOrderId)
            .SingleOrDefaultAsync(ct);
        return parentId?.ToString();
    }
}
