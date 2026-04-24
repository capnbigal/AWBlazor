using AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Dtos;

public sealed record WeeklyPlanDto(
    int Id, int WeekId, short LocationId, int Version,
    DateTime PublishedAt, string PublishedBy, bool BaselineDiverged,
    string? GenerationOptionsJson);

public sealed record WeeklyPlanItemDto(
    int Id, int WeeklyPlanId, int SalesOrderId, int SalesOrderDetailId, int ProductId,
    int PlannedSequence, DateTime PlannedStart, DateTime PlannedEnd, short PlannedQty,
    bool OverCapacity);

public sealed record GenerateWeeklyPlanRequest
{
    public int WeekId { get; set; }
    public short LocationId { get; set; }
    public bool DryRun { get; set; } = false;
    public bool StrictCapacity { get; set; } = false;
}

public static class WeeklyPlanMappings
{
    public static WeeklyPlanDto ToDto(this WeeklyPlan e) =>
        new(e.Id, e.WeekId, e.LocationId, e.Version, e.PublishedAt, e.PublishedBy,
            e.BaselineDiverged, e.GenerationOptionsJson);

    public static WeeklyPlanItemDto ToDto(this WeeklyPlanItem e) =>
        new(e.Id, e.WeeklyPlanId, e.SalesOrderId, e.SalesOrderDetailId, e.ProductId,
            e.PlannedSequence, e.PlannedStart, e.PlannedEnd, e.PlannedQty, e.OverCapacity);
}
