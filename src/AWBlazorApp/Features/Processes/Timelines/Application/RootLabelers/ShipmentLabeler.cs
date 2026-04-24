using AWBlazorApp.Features.Logistics.Shipments.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.RootLabelers;

public class ShipmentLabeler : IRootEntityLabeler
{
    public string EntityType => "Shipment";

    public async Task<string?> GetLabelAsync(ApplicationDbContext db, string rootEntityId, CancellationToken ct)
    {
        if (!int.TryParse(rootEntityId, out var id)) return null;
        var exists = await db.Set<Shipment>().AsNoTracking()
            .AnyAsync(s => s.Id == id, ct);
        return exists ? $"Shipment #{id}" : null;
    }
}
