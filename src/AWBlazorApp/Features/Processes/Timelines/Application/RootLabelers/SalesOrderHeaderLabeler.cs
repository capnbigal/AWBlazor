using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.RootLabelers;

public class SalesOrderHeaderLabeler : IRootEntityLabeler
{
    public string EntityType => "SalesOrderHeader";

    public async Task<string?> GetLabelAsync(ApplicationDbContext db, string rootEntityId, CancellationToken ct)
    {
        if (!int.TryParse(rootEntityId, out var id)) return null;
        var exists = await db.Set<SalesOrderHeader>().AsNoTracking()
            .AnyAsync(s => s.Id == id, ct);
        return exists ? $"SO #{id}" : null;
    }
}
