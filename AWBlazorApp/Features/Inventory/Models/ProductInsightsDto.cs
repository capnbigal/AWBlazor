namespace AWBlazorApp.Features.Inventory.Models;

/// <summary>
/// Cross-module snapshot for the product-centric explorer at /inventory. One service call
/// loads everything the right pane needs for a single product so the UI binds in one shot.
/// Cached per-product 60s by the service.
/// </summary>
public sealed record ProductInsightsDto(
    DateTime GeneratedAt,
    ProductHeaderDto Header,
    InventorySnapshotDto Inventory,
    LotSummaryDto Lots,
    IReadOnlyList<MonthlyActivityDto> InventoryActivity12m,
    IReadOnlyList<MonthlyActivityDto> PurchaseOrders12m,
    IReadOnlyList<MonthlyActivityDto> WorkOrders12m,
    IReadOnlyList<MonthlyActivityDto> ProductionRuns12m,
    IReadOnlyList<EcoSummaryDto> RecentEcos);

/// <summary>Per-product master data + the linked InventoryItem flags (when managed).</summary>
public sealed record ProductHeaderDto(
    int ProductId,
    string Name,
    string ProductNumber,
    string? Color,
    string? Class,
    string? Style,
    string? ProductLine,
    decimal ListPrice,
    decimal StandardCost,
    short SafetyStockLevel,
    short ReorderPoint,
    int DaysToManufacture,
    bool MakeFlag,
    bool FinishedGoodsFlag,
    DateTime SellStartDate,
    DateTime? SellEndDate,
    DateTime? DiscontinuedDate,
    int? InventoryItemId,
    bool TracksLot,
    bool TracksSerial,
    decimal InvReorderPoint,
    decimal InvReorderQty);

/// <summary>Aggregated on-hand position for a managed product.</summary>
public sealed record InventorySnapshotDto(
    decimal TotalOnHand,
    decimal TotalAvailable,
    decimal TotalHold,
    decimal TotalQuarantine,
    int LocationCount,
    bool BelowReorderPoint,
    IReadOnlyList<LocationBalanceDto> ByLocation);

public sealed record LocationBalanceDto(
    int LocationId,
    string LocationCode,
    string LocationName,
    decimal QtyOnHand);

public sealed record LotSummaryDto(
    int ActiveLotCount,
    decimal TotalLotQty,
    IReadOnlyList<LotRowDto> TopLots);

public sealed record LotRowDto(
    int LotId,
    string LotCode,
    decimal QtyOnHand,
    DateTime? ManufacturedAt,
    DateTime? ReceivedAt,
    string Status);

/// <summary>One bin in a 12-month time series. Always returned in chronological order; missing months emit Value=0.</summary>
public sealed record MonthlyActivityDto(int Year, int Month, decimal Value);

public sealed record EcoSummaryDto(
    int Id,
    string Code,
    string Title,
    string Status,
    DateTime RaisedAt,
    DateTime? DecidedAt);

/// <summary>Row in the left-pane product picker. IsManaged flags whether an InventoryItem row exists.</summary>
public sealed record ProductPickerItemDto(
    int ProductId,
    string Name,
    string ProductNumber,
    bool IsManaged,
    bool DiscontinuedOrEnded);
