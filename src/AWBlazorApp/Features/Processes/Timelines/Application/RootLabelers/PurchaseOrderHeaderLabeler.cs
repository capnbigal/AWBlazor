using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.RootLabelers;

public class PurchaseOrderHeaderLabeler : IRootEntityLabeler
{
    public string EntityType => "PurchaseOrderHeader";

    public async Task<string?> GetLabelAsync(ApplicationDbContext db, string rootEntityId, CancellationToken ct)
    {
        if (!int.TryParse(rootEntityId, out var id)) return null;
        var exists = await db.Set<PurchaseOrderHeader>().AsNoTracking()
            .AnyAsync(p => p.Id == id, ct);
        return exists ? $"PO #{id}" : null;
    }
}
