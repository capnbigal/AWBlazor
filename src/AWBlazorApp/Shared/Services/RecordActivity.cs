using AWBlazorApp.Shared.UI.Components;
using MudBlazor;

namespace AWBlazorApp.Shared.Services;

/// <summary>One synthesized activity-timeline entry for an AdventureWorks record.</summary>
/// <param name="Label">What happened (e.g. "Shipped").</param>
/// <param name="At">When, or null for a planned/not-yet-reached milestone.</param>
/// <param name="Icon">Material icon for the milestone.</param>
/// <param name="Color">MudBlazor color (Success/Info/Warning/Default…).</param>
/// <param name="Detail">Optional secondary text.</param>
public sealed record ActivityEvent(string Label, DateTime? At, string Icon, Color Color, string? Detail = null);

/// <summary>
/// Synthesizes a chronological activity timeline for AdventureWorks records from their temporal
/// columns (OrderDate / DueDate / ShipDate / StartDate / EndDate / status). These tables predate the
/// app's AuditLog, so most rows have no audit history — but their date columns still tell the story
/// of the record's lifecycle. Pure functions: callers pass the already-loaded values, so there is no
/// DB access here and the logic is trivially unit-testable. Events are returned in chronological
/// order with planned/unknown (null-dated) milestones sinking to the end.
/// </summary>
public static class RecordActivity
{
    public static IReadOnlyList<ActivityEvent> ForSalesOrder(DateTime orderDate, DateTime dueDate, DateTime? shipDate, byte status)
    {
        var (statusLabel, statusColor) = StatusHelper.SalesOrderStatus(status);
        return Ordered(
            new ActivityEvent("Order placed", orderDate, Icons.Material.Filled.ShoppingCart, Color.Primary),
            new ActivityEvent("Due", dueDate, Icons.Material.Filled.Event, Color.Info),
            shipDate is null
                ? new ActivityEvent("Not yet shipped", null, Icons.Material.Filled.LocalShipping, Color.Default)
                : new ActivityEvent("Shipped", shipDate, Icons.Material.Filled.LocalShipping, Color.Success),
            new ActivityEvent(statusLabel, null, Icons.Material.Filled.Flag, statusColor, "Current status"));
    }

    public static IReadOnlyList<ActivityEvent> ForPurchaseOrder(DateTime orderDate, DateTime? shipDate, byte status)
    {
        var (statusLabel, statusColor) = StatusHelper.PurchaseOrderStatus(status);
        return Ordered(
            new ActivityEvent("Ordered", orderDate, Icons.Material.Filled.ShoppingBag, Color.Primary),
            shipDate is null
                ? new ActivityEvent("Not yet shipped", null, Icons.Material.Filled.LocalShipping, Color.Default)
                : new ActivityEvent("Shipped", shipDate, Icons.Material.Filled.LocalShipping, Color.Success),
            new ActivityEvent(statusLabel, null, Icons.Material.Filled.Flag, statusColor, "Current status"));
    }

    public static IReadOnlyList<ActivityEvent> ForWorkOrder(DateTime startDate, DateTime dueDate, DateTime? endDate) =>
        Ordered(
            new ActivityEvent("Started", startDate, Icons.Material.Filled.PlayArrow, Color.Primary),
            new ActivityEvent("Due", dueDate, Icons.Material.Filled.Event, Color.Info),
            endDate is null
                ? new ActivityEvent("In progress", null, Icons.Material.Filled.HourglassTop, Color.Warning)
                : new ActivityEvent("Completed", endDate, Icons.Material.Filled.CheckCircle, Color.Success));

    // Chronological ascending; null-dated milestones (planned / not-yet-reached) sort to the end in
    // their declared order (OrderBy is a stable sort).
    private static IReadOnlyList<ActivityEvent> Ordered(params ActivityEvent[] events) =>
        events.OrderBy(e => e.At ?? DateTime.MaxValue).ToList();
}
