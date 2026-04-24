using AWBlazorApp.Features.Logistics.Shipments.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.HopQueries;

public class ShipmentLineFromShipment : IChainHopQuery
{
    public string ParentEntity => "Shipment";
    public string ChildEntity => "ShipmentLine";
    public string ForeignKey => "ShipmentId";

    public async Task<IReadOnlyList<string>> GetChildIdsAsync(
        ApplicationDbContext db, IReadOnlyList<string> parentIds, CancellationToken ct)
    {
        if (parentIds.Count == 0) return Array.Empty<string>();
        var ints = parentIds.Select(int.Parse).ToArray();
        return await db.Set<ShipmentLine>().AsNoTracking()
            .Where(l => ints.Contains(l.ShipmentId))
            .Select(l => l.Id.ToString())
            .ToListAsync(ct);
    }

    public async Task<string?> GetParentIdAsync(
        ApplicationDbContext db, string childId, CancellationToken ct)
    {
        var id = int.Parse(childId);
        var parentId = await db.Set<ShipmentLine>().AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => (int?)l.ShipmentId)
            .SingleOrDefaultAsync(ct);
        return parentId?.ToString();
    }
}
