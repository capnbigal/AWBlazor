using AWBlazorApp.Features.Logistics.Receipts.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Processes.Timelines.Application.HopQueries;

public class GoodsReceiptFromPurchaseOrderHeader : IChainHopQuery
{
    public string ParentEntity => "PurchaseOrderHeader";
    public string ChildEntity => "GoodsReceipt";
    public string ForeignKey => "PurchaseOrderId";

    public async Task<IReadOnlyList<string>> GetChildIdsAsync(
        ApplicationDbContext db, IReadOnlyList<string> parentIds, CancellationToken ct)
    {
        if (parentIds.Count == 0) return Array.Empty<string>();
        var ints = parentIds.Select(int.Parse).ToArray();
        return await db.Set<GoodsReceipt>().AsNoTracking()
            .Where(r => r.PurchaseOrderId != null && ints.Contains(r.PurchaseOrderId.Value))
            .Select(r => r.Id.ToString())
            .ToListAsync(ct);
    }

    public async Task<string?> GetParentIdAsync(
        ApplicationDbContext db, string childId, CancellationToken ct)
    {
        var id = int.Parse(childId);
        var parentId = await db.Set<GoodsReceipt>().AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => r.PurchaseOrderId)
            .SingleOrDefaultAsync(ct);
        return parentId?.ToString();
    }
}
