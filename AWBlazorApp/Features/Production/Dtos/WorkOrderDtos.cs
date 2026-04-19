using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Dtos;

public sealed record WorkOrderDto(
    int Id, int ProductId, int OrderQty, int StockedQty, short ScrappedQty,
    DateTime StartDate, DateTime? EndDate, DateTime DueDate,
    short? ScrapReasonId, DateTime ModifiedDate);

public sealed record CreateWorkOrderRequest
{
    public int ProductId { get; set; }
    public int OrderQty { get; set; }
    public short ScrappedQty { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime DueDate { get; set; }
    public short? ScrapReasonId { get; set; }
}

public sealed record UpdateWorkOrderRequest
{
    public int? ProductId { get; set; }
    public int? OrderQty { get; set; }
    public short? ScrappedQty { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DueDate { get; set; }
    public short? ScrapReasonId { get; set; }
}

public sealed record WorkOrderAuditLogDto(
    int Id, int WorkOrderId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int ProductId, int OrderQty, int StockedQty, short ScrappedQty,
    DateTime StartDate, DateTime? EndDate, DateTime DueDate, short? ScrapReasonId,
    DateTime SourceModifiedDate);

public static class WorkOrderMappings
{
    public static WorkOrderDto ToDto(this WorkOrder e) => new(
        e.Id, e.ProductId, e.OrderQty, e.StockedQty, e.ScrappedQty,
        e.StartDate, e.EndDate, e.DueDate, e.ScrapReasonId, e.ModifiedDate);

    public static WorkOrder ToEntity(this CreateWorkOrderRequest r) => new()
    {
        ProductId = r.ProductId,
        OrderQty = r.OrderQty,
        ScrappedQty = r.ScrappedQty,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        DueDate = r.DueDate,
        ScrapReasonId = r.ScrapReasonId,
        // StockedQty is computed by SQL Server (ISNULL(OrderQty - ScrappedQty, 0)) — leave default.
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateWorkOrderRequest r, WorkOrder e)
    {
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        if (r.OrderQty.HasValue) e.OrderQty = r.OrderQty.Value;
        if (r.ScrappedQty.HasValue) e.ScrappedQty = r.ScrappedQty.Value;
        if (r.StartDate.HasValue) e.StartDate = r.StartDate.Value;
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        if (r.DueDate.HasValue) e.DueDate = r.DueDate.Value;
        if (r.ScrapReasonId.HasValue) e.ScrapReasonId = r.ScrapReasonId.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static WorkOrderAuditLogDto ToDto(this WorkOrderAuditLog a) => new(
        a.Id, a.WorkOrderId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ProductId, a.OrderQty, a.StockedQty, a.ScrappedQty,
        a.StartDate, a.EndDate, a.DueDate, a.ScrapReasonId, a.SourceModifiedDate);
}
