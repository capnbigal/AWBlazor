using AWBlazorApp.Features.Logistics.Receipts.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.HopQueries;

public class GoodsReceiptLineFromGoodsReceipt : IChainHopQuery
{
    public string ParentEntity => "GoodsReceipt";
    public string ChildEntity => "GoodsReceiptLine";
    public string ForeignKey => "GoodsReceiptId";

    public async Task<IReadOnlyList<string>> GetChildIdsAsync(
        ApplicationDbContext db, IReadOnlyList<string> parentIds, CancellationToken ct)
    {
        if (parentIds.Count == 0) return Array.Empty<string>();
        var ints = parentIds.Select(int.Parse).ToArray();
        return await db.Set<GoodsReceiptLine>().AsNoTracking()
            .Where(l => ints.Contains(l.GoodsReceiptId))
            .Select(l => l.Id.ToString())
            .ToListAsync(ct);
    }

    public async Task<string?> GetParentIdAsync(
        ApplicationDbContext db, string childId, CancellationToken ct)
    {
        var id = int.Parse(childId);
        var parentId = await db.Set<GoodsReceiptLine>().AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => (int?)l.GoodsReceiptId)
            .SingleOrDefaultAsync(ct);
        return parentId?.ToString();
    }
}
