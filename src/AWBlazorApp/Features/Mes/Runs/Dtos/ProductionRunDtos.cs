using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 

namespace AWBlazorApp.Features.Mes.Runs.Dtos;

public sealed record ProductionRunDto(
    int Id, string RunNumber, ProductionRunKind Kind, int? WorkOrderId, int? StationId, int? AssetId,
    ProductionRunStatus Status, DateTime? PlannedStartAt, DateTime? ActualStartAt, DateTime? ActualEndAt,
    decimal QuantityPlanned, decimal QuantityProduced, decimal QuantityScrapped,
    string? Notes, string? PostedByUserId, DateTime ModifiedDate);

public sealed record CreateProductionRunRequest
{
    public ProductionRunKind Kind { get; set; } = ProductionRunKind.Production;
    public int? WorkOrderId { get; set; }
    public int? StationId { get; set; }
    public int? AssetId { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public decimal QuantityPlanned { get; set; }
    public string? Notes { get; set; }
}

public sealed record UpdateProductionRunRequest
{
    public ProductionRunKind? Kind { get; set; }
    public int? WorkOrderId { get; set; }
    public int? StationId { get; set; }
    public int? AssetId { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public decimal? QuantityPlanned { get; set; }
    public string? Notes { get; set; }
}

public sealed record ProductionRunOperationDto(
    int Id, int ProductionRunId, short? OperationSequence, int SequenceNumber, string OperationDescription,
    DateTime? StartAt, DateTime? EndAt, decimal ActualHours, DateTime ModifiedDate);

public sealed record CreateProductionRunOperationRequest
{
    public int ProductionRunId { get; set; }
    public short? OperationSequence { get; set; }
    public int SequenceNumber { get; set; }
    public string? OperationDescription { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public decimal ActualHours { get; set; }
}

public sealed record UpdateProductionRunOperationRequest
{
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public decimal? ActualHours { get; set; }
    public string? OperationDescription { get; set; }
}

public sealed record CompleteProductionRunRequest
{
    public decimal QuantityProduced { get; set; }
    public decimal QuantityScrapped { get; set; }
    public int? MaterialIssueInventoryItemId { get; set; }
    public decimal? MaterialIssueQuantity { get; set; }
    public string? MaterialIssueUnitMeasureCode { get; set; }
    public int? MaterialIssueFromLocationId { get; set; }
    public int? MaterialIssueLotId { get; set; }
}

public static class ProductionRunMappings
{
    public static ProductionRunDto ToDto(this ProductionRun e) => new(
        e.Id, e.RunNumber, e.Kind, e.WorkOrderId, e.StationId, e.AssetId,
        e.Status, e.PlannedStartAt, e.ActualStartAt, e.ActualEndAt,
        e.QuantityPlanned, e.QuantityProduced, e.QuantityScrapped,
        e.Notes, e.PostedByUserId, e.ModifiedDate);

    public static ProductionRun ToEntity(this CreateProductionRunRequest r)
    {
        var now = DateTime.UtcNow;
        return new ProductionRun
        {
            RunNumber = $"RUN-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            Kind = r.Kind,
            WorkOrderId = r.WorkOrderId,
            StationId = r.StationId,
            AssetId = r.AssetId,
            Status = ProductionRunStatus.Draft,
            PlannedStartAt = r.PlannedStartAt,
            QuantityPlanned = r.QuantityPlanned,
            Notes = r.Notes?.Trim(),
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateProductionRunRequest r, ProductionRun e)
    {
        if (r.Kind is not null) e.Kind = r.Kind.Value;
        if (r.WorkOrderId is not null) e.WorkOrderId = r.WorkOrderId;
        if (r.StationId is not null) e.StationId = r.StationId;
        if (r.AssetId is not null) e.AssetId = r.AssetId;
        if (r.PlannedStartAt is not null) e.PlannedStartAt = r.PlannedStartAt;
        if (r.QuantityPlanned is not null) e.QuantityPlanned = r.QuantityPlanned.Value;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ProductionRunOperationDto ToDto(this ProductionRunOperation e) => new(
        e.Id, e.ProductionRunId, e.OperationSequence, e.SequenceNumber, e.OperationDescription,
        e.StartAt, e.EndAt, e.ActualHours, e.ModifiedDate);

    public static ProductionRunOperation ToEntity(this CreateProductionRunOperationRequest r) => new()
    {
        ProductionRunId = r.ProductionRunId,
        OperationSequence = r.OperationSequence,
        SequenceNumber = r.SequenceNumber,
        OperationDescription = (r.OperationDescription ?? string.Empty).Trim(),
        StartAt = r.StartAt,
        EndAt = r.EndAt,
        ActualHours = r.ActualHours,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateProductionRunOperationRequest r, ProductionRunOperation e)
    {
        if (r.StartAt is not null) e.StartAt = r.StartAt;
        if (r.EndAt is not null) e.EndAt = r.EndAt;
        if (r.ActualHours is not null) e.ActualHours = r.ActualHours.Value;
        if (r.OperationDescription is not null) e.OperationDescription = r.OperationDescription.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }
}
