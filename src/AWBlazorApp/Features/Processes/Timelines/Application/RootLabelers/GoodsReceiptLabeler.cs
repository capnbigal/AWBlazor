using AWBlazorApp.Features.Logistics.Receipts.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.RootLabelers;

public class GoodsReceiptLabeler : IRootEntityLabeler
{
    public string EntityType => "GoodsReceipt";

    public async Task<string?> GetLabelAsync(ApplicationDbContext db, string rootEntityId, CancellationToken ct)
    {
        if (!int.TryParse(rootEntityId, out var id)) return null;
        var exists = await db.Set<GoodsReceipt>().AsNoTracking()
            .AnyAsync(r => r.Id == id, ct);
        return exists ? $"Receipt #{id}" : null;
    }
}
