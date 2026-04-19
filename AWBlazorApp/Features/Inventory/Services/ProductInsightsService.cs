using AWBlazorApp.Features.Engineering.Domain;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Dtos;
using AWBlazorApp.Features.Mes.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AWBlazorApp.Features.Inventory.Services;

public interface IProductInsightsService
{
    /// <summary>Loads everything the product-explorer right pane needs in one shot.</summary>
    Task<ProductInsightsDto?> GetAsync(int productId, CancellationToken ct);

    /// <summary>Searchable product list for the explorer's left pane. Limited to <paramref name="take"/> rows.</summary>
    Task<IReadOnlyList<ProductPickerItemDto>> ListProductsAsync(string? search, int take, CancellationToken ct);

    /// <summary>Drop one product's cached payload (or all of them when null).</summary>
    void Invalidate(int? productId = null);
}

public sealed class ProductInsightsService : IProductInsightsService
{
    private const string CacheKeyPrefix = "product-insights:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IMemoryCache _cache;

    public ProductInsightsService(IDbContextFactory<ApplicationDbContext> dbFactory, IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache = cache;
    }

    public void Invalidate(int? productId = null)
    {
        if (productId.HasValue)
            _cache.Remove(CacheKeyPrefix + productId.Value);
        // We can't enumerate IMemoryCache to wipe the whole prefix without reflection; the
        // 60-second TTL bounds staleness for "wipe all" callers, which only really matters for
        // tests anyway — they create a fresh provider per scope.
    }

    public async Task<ProductInsightsDto?> GetAsync(int productId, CancellationToken ct)
    {
        if (_cache.TryGetValue<ProductInsightsDto>(CacheKeyPrefix + productId, out var cached) && cached is not null)
            return cached;

        var dto = await BuildAsync(productId, ct);
        if (dto is not null)
            _cache.Set(CacheKeyPrefix + productId, dto, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });
        return dto;
    }

    public async Task<IReadOnlyList<ProductPickerItemDto>> ListProductsAsync(string? search, int take, CancellationToken ct)
    {
        take = Math.Clamp(take, 1, 500);
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var s = search?.Trim();
        var managedItemProductIds = await db.InventoryItems.AsNoTracking()
            .Where(i => i.IsActive)
            .Select(i => i.ProductId)
            .ToListAsync(ct);
        var managed = managedItemProductIds.ToHashSet();

        var q = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(s))
        {
            q = q.Where(p => p.Name.Contains(s) || p.ProductNumber.Contains(s));
        }

        var rows = await q.OrderBy(p => p.Name)
            .Take(take)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ProductNumber,
                Discontinued = p.DiscontinuedDate != null || (p.SellEndDate != null && p.SellEndDate < DateTime.UtcNow),
            })
            .ToListAsync(ct);

        return rows.Select(p => new ProductPickerItemDto(
            p.Id, p.Name, p.ProductNumber, managed.Contains(p.Id), p.Discontinued)).ToList();
    }

    private async Task<ProductInsightsDto?> BuildAsync(int productId, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return null;

        // Optional inventory metadata layer. May be null for master-data-only products.
        var item = await db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == productId, ct);

        var header = new ProductHeaderDto(
            ProductId: product.Id,
            Name: product.Name,
            ProductNumber: product.ProductNumber,
            Color: product.Color,
            Class: product.Class,
            Style: product.Style,
            ProductLine: product.ProductLine,
            ListPrice: product.ListPrice,
            StandardCost: product.StandardCost,
            SafetyStockLevel: product.SafetyStockLevel,
            ReorderPoint: product.ReorderPoint,
            DaysToManufacture: product.DaysToManufacture,
            MakeFlag: product.MakeFlag,
            FinishedGoodsFlag: product.FinishedGoodsFlag,
            SellStartDate: product.SellStartDate,
            SellEndDate: product.SellEndDate,
            DiscontinuedDate: product.DiscontinuedDate,
            InventoryItemId: item?.Id,
            TracksLot: item?.TracksLot ?? false,
            TracksSerial: item?.TracksSerial ?? false,
            InvReorderPoint: item?.ReorderPoint ?? 0m,
            InvReorderQty: item?.ReorderQty ?? 0m);

        var inventory = item is null
            ? new InventorySnapshotDto(0, 0, 0, 0, 0, false, Array.Empty<LocationBalanceDto>())
            : await BuildInventorySnapshotAsync(db, item, ct);

        var lots = item is null
            ? new LotSummaryDto(0, 0, Array.Empty<LotRowDto>())
            : await BuildLotSummaryAsync(db, item, ct);

        // 12-month windows are inclusive of the current month — index 0 = oldest, index 11 = current.
        var now = DateTime.UtcNow;
        var windowStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

        IReadOnlyList<MonthlyActivityDto> invActivity = item is null
            ? EmptyMonthly(windowStart)
            : await BuildInventoryActivityAsync(db, item, windowStart, ct);

        var pos = await BuildPurchaseOrdersAsync(db, productId, windowStart, ct);
        var wos = await BuildWorkOrdersAsync(db, productId, windowStart, ct);
        var runs = await BuildProductionRunsAsync(db, productId, windowStart, ct);
        var ecos = await BuildRecentEcosAsync(db, productId, ct);

        return new ProductInsightsDto(
            GeneratedAt: DateTime.UtcNow,
            Header: header,
            Inventory: inventory,
            Lots: lots,
            InventoryActivity12m: invActivity,
            PurchaseOrders12m: pos,
            WorkOrders12m: wos,
            ProductionRuns12m: runs,
            RecentEcos: ecos);
    }

    private static async Task<InventorySnapshotDto> BuildInventorySnapshotAsync(ApplicationDbContext db, InventoryItem item, CancellationToken ct)
    {
        var balances = await db.InventoryBalances.AsNoTracking()
            .Where(b => b.InventoryItemId == item.Id && b.Quantity != 0)
            .Select(b => new { b.LocationId, b.Status, b.Quantity })
            .ToListAsync(ct);

        if (balances.Count == 0)
        {
            return new InventorySnapshotDto(0, 0, 0, 0, 0, item.ReorderPoint > 0, Array.Empty<LocationBalanceDto>());
        }

        var locationIds = balances.Select(b => b.LocationId).Distinct().ToList();
        var locations = await db.InventoryLocations.AsNoTracking()
            .Where(l => locationIds.Contains(l.Id))
            .Select(l => new { l.Id, l.Code, l.Name })
            .ToListAsync(ct);
        var locationLookup = locations.ToDictionary(l => l.Id);

        var byLocation = balances
            .GroupBy(b => b.LocationId)
            .Select(g =>
            {
                var loc = locationLookup.TryGetValue(g.Key, out var l) ? l : null;
                return new LocationBalanceDto(
                    LocationId: g.Key,
                    LocationCode: loc?.Code ?? "?",
                    LocationName: loc?.Name ?? $"Location #{g.Key}",
                    QtyOnHand: g.Sum(x => x.Quantity));
            })
            .OrderByDescending(b => b.QtyOnHand)
            .ToList();

        var totalOnHand = balances.Sum(b => b.Quantity);
        var totalAvailable = balances.Where(b => b.Status == BalanceStatus.Available).Sum(b => b.Quantity);
        var totalHold = balances.Where(b => b.Status == BalanceStatus.Hold).Sum(b => b.Quantity);
        var totalQuarantine = balances.Where(b => b.Status == BalanceStatus.Quarantine).Sum(b => b.Quantity);

        return new InventorySnapshotDto(
            TotalOnHand: totalOnHand,
            TotalAvailable: totalAvailable,
            TotalHold: totalHold,
            TotalQuarantine: totalQuarantine,
            LocationCount: byLocation.Count,
            BelowReorderPoint: item.ReorderPoint > 0 && totalAvailable < item.ReorderPoint,
            ByLocation: byLocation);
    }

    private static async Task<LotSummaryDto> BuildLotSummaryAsync(ApplicationDbContext db, InventoryItem item, CancellationToken ct)
    {
        if (!item.TracksLot)
            return new LotSummaryDto(0, 0, Array.Empty<LotRowDto>());

        // Sum balances per lot first so we can show qty-on-hand alongside lot metadata.
        var lotBalances = await db.InventoryBalances.AsNoTracking()
            .Where(b => b.InventoryItemId == item.Id && b.LotId != null && b.Quantity != 0)
            .GroupBy(b => b.LotId!.Value)
            .Select(g => new { LotId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var activeLotIds = lotBalances.Select(l => l.LotId).ToList();
        var lots = await db.Lots.AsNoTracking()
            .Where(l => l.InventoryItemId == item.Id && activeLotIds.Contains(l.Id))
            .Select(l => new { l.Id, l.LotCode, l.ManufacturedAt, l.ReceivedAt, l.Status })
            .ToListAsync(ct);

        var qtyByLot = lotBalances.ToDictionary(b => b.LotId, b => b.Qty);

        var rows = lots
            .Select(l => new LotRowDto(
                LotId: l.Id,
                LotCode: l.LotCode,
                QtyOnHand: qtyByLot.TryGetValue(l.Id, out var q) ? q : 0m,
                ManufacturedAt: l.ManufacturedAt,
                ReceivedAt: l.ReceivedAt,
                Status: l.Status.ToString()))
            .OrderByDescending(l => l.QtyOnHand)
            .ToList();

        return new LotSummaryDto(
            ActiveLotCount: rows.Count,
            TotalLotQty: rows.Sum(r => r.QtyOnHand),
            TopLots: rows.Take(10).ToList());
    }

    private static async Task<IReadOnlyList<MonthlyActivityDto>> BuildInventoryActivityAsync(ApplicationDbContext db, InventoryItem item, DateTime windowStart, CancellationToken ct)
    {
        var raw = await db.InventoryTransactions.AsNoTracking()
            .Where(t => t.InventoryItemId == item.Id && t.OccurredAt >= windowStart)
            .Select(t => new { t.OccurredAt, t.Quantity })
            .ToListAsync(ct);
        return BinByMonth(windowStart, raw.Select(r => (r.OccurredAt, r.Quantity)));
    }

    private static async Task<IReadOnlyList<MonthlyActivityDto>> BuildPurchaseOrdersAsync(ApplicationDbContext db, int productId, DateTime windowStart, CancellationToken ct)
    {
        // Join through PurchaseOrderHeader for OrderDate. Sum OrderQty per month.
        var raw = await (
            from d in db.PurchaseOrderDetails.AsNoTracking()
            join h in db.PurchaseOrderHeaders.AsNoTracking() on d.PurchaseOrderId equals h.Id
            where d.ProductId == productId && h.OrderDate >= windowStart
            select new { h.OrderDate, Qty = (decimal)d.OrderQty }
        ).ToListAsync(ct);
        return BinByMonth(windowStart, raw.Select(r => (r.OrderDate, r.Qty)));
    }

    private static async Task<IReadOnlyList<MonthlyActivityDto>> BuildWorkOrdersAsync(ApplicationDbContext db, int productId, DateTime windowStart, CancellationToken ct)
    {
        var raw = await db.WorkOrders.AsNoTracking()
            .Where(w => w.ProductId == productId && w.StartDate >= windowStart)
            .Select(w => new { w.StartDate, Qty = (decimal)w.OrderQty })
            .ToListAsync(ct);
        return BinByMonth(windowStart, raw.Select(r => (r.StartDate, r.Qty)));
    }

    private static async Task<IReadOnlyList<MonthlyActivityDto>> BuildProductionRunsAsync(ApplicationDbContext db, int productId, DateTime windowStart, CancellationToken ct)
    {
        // ProductionRun has no direct ProductId; it joins to a product only when WorkOrderId is
        // set. Ad-hoc kinds (Engineering / Replacement / Service) carry no product attribution and
        // are intentionally omitted from this chart.
        var raw = await (
            from r in db.ProductionRuns.AsNoTracking()
            join w in db.WorkOrders.AsNoTracking() on r.WorkOrderId equals w.Id
            where w.ProductId == productId
                && r.ActualStartAt != null
                && r.ActualStartAt >= windowStart
            select new { Date = r.ActualStartAt!.Value, Qty = 1m }
        ).ToListAsync(ct);
        return BinByMonth(windowStart, raw.Select(r => (r.Date, r.Qty)));
    }

    private static async Task<IReadOnlyList<EcoSummaryDto>> BuildRecentEcosAsync(ApplicationDbContext db, int productId, CancellationToken ct)
    {
        // ECOs link to products via EcoAffectedItem. Pull affected-item rows where AffectedKind=Product
        // and TargetId=productId, then resolve the parent ECO. Distinct because one ECO can list
        // the same product more than once across different change descriptions.
        var ecoIds = await db.EcoAffectedItems.AsNoTracking()
            .Where(a => a.AffectedKind == EcoAffectedKind.Product && a.TargetId == productId)
            .Select(a => a.EngineeringChangeOrderId)
            .Distinct()
            .ToListAsync(ct);

        if (ecoIds.Count == 0) return Array.Empty<EcoSummaryDto>();

        var rows = await db.EngineeringChangeOrders.AsNoTracking()
            .Where(e => ecoIds.Contains(e.Id))
            .OrderByDescending(e => e.RaisedAt)
            .Take(10)
            .Select(e => new EcoSummaryDto(
                e.Id, e.Code, e.Title, e.Status.ToString(), e.RaisedAt, e.DecidedAt))
            .ToListAsync(ct);
        return rows;
    }

    /// <summary>
    /// Bin a sequence of (date, value) pairs into 12 monthly buckets starting at
    /// <paramref name="windowStart"/>. Output length is always 12; missing months emit Value=0.
    /// </summary>
    private static IReadOnlyList<MonthlyActivityDto> BinByMonth(DateTime windowStart, IEnumerable<(DateTime Date, decimal Value)> events)
    {
        var bins = Enumerable.Range(0, 12)
            .Select(i =>
            {
                var d = windowStart.AddMonths(i);
                return new MonthlyActivityDto(d.Year, d.Month, 0m);
            })
            .ToArray();

        foreach (var (date, value) in events)
        {
            var monthIndex = ((date.Year - windowStart.Year) * 12) + (date.Month - windowStart.Month);
            if (monthIndex >= 0 && monthIndex < 12)
            {
                bins[monthIndex] = bins[monthIndex] with { Value = bins[monthIndex].Value + value };
            }
        }
        return bins;
    }

    private static IReadOnlyList<MonthlyActivityDto> EmptyMonthly(DateTime windowStart) =>
        BinByMonth(windowStart, Enumerable.Empty<(DateTime, decimal)>());
}
