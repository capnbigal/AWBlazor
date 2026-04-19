using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 

namespace AWBlazorApp.Features.Quality.Inspections.Dtos;

public sealed record InspectionDto(
    int Id, string InspectionNumber, int InspectionPlanId, InspectionStatus Status,
    InspectionSourceKind SourceKind, int SourceId, int? InspectorBusinessEntityId,
    DateTime? InspectedAt, int? InventoryItemId, int? LotId,
    decimal Quantity, string UnitMeasureCode, string? Notes, string? PostedByUserId, DateTime ModifiedDate);

public sealed record CreateInspectionRequest
{
    public int InspectionPlanId { get; set; }
    public InspectionSourceKind SourceKind { get; set; } = InspectionSourceKind.Manual;
    public int SourceId { get; set; }
    public int? InventoryItemId { get; set; }
    public int? LotId { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public string? Notes { get; set; }
}

public sealed record StartInspectionRequest
{
    public int? InspectorBusinessEntityId { get; set; }
}

public sealed record InspectionAuditLogDto(
    int Id, int InspectionId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? InspectionNumber, int InspectionPlanId, InspectionStatus Status,
    InspectionSourceKind SourceKind, int SourceId, int? InspectorBusinessEntityId,
    DateTime? InspectedAt, int? InventoryItemId, int? LotId,
    decimal Quantity, string? UnitMeasureCode, string? Notes, string? PostedByUserId, DateTime SourceModifiedDate);

public sealed record InspectionResultDto(
    long Id, int InspectionId, int InspectionPlanCharacteristicId,
    decimal? NumericResult, string? AttributeResult, bool Passed, string? Notes,
    DateTime RecordedAt, int? RecordedByBusinessEntityId);

public sealed record RecordInspectionResultRequest
{
    public int InspectionPlanCharacteristicId { get; set; }
    public decimal? NumericResult { get; set; }
    public string? AttributeResult { get; set; }
    public string? Notes { get; set; }
    public int? RecordedByBusinessEntityId { get; set; }
}

public static class InspectionMappings
{
    public static InspectionDto ToDto(this Inspection e) => new(
        e.Id, e.InspectionNumber, e.InspectionPlanId, e.Status,
        e.SourceKind, e.SourceId, e.InspectorBusinessEntityId,
        e.InspectedAt, e.InventoryItemId, e.LotId,
        e.Quantity, e.UnitMeasureCode, e.Notes, e.PostedByUserId, e.ModifiedDate);

    public static InspectionAuditLogDto ToDto(this InspectionAuditLog a) => new(
        a.Id, a.InspectionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.InspectionNumber, a.InspectionPlanId, a.Status,
        a.SourceKind, a.SourceId, a.InspectorBusinessEntityId,
        a.InspectedAt, a.InventoryItemId, a.LotId,
        a.Quantity, a.UnitMeasureCode, a.Notes, a.PostedByUserId, a.SourceModifiedDate);

    public static InspectionResultDto ToDto(this InspectionResult e) => new(
        e.Id, e.InspectionId, e.InspectionPlanCharacteristicId,
        e.NumericResult, e.AttributeResult, e.Passed, e.Notes,
        e.RecordedAt, e.RecordedByBusinessEntityId);
}
