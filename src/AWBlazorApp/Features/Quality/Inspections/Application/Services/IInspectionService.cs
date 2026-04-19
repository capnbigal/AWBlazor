using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 

namespace AWBlazorApp.Features.Quality.Inspections.Application.Services;

/// <summary>
/// Owns the inspection lifecycle. Start moves Pending → InProgress and stamps the inspector;
/// RecordResult appends one result row and computes Pass/Fail against the plan's tolerances;
/// Complete rolls up the recorded results into an overall Pass / Fail status. A Fail
/// auto-opens a <see cref="NonConformance"/> referencing the inspection.
/// </summary>
public interface IInspectionService
{
    Task<int> CreateAsync(CreateInspectionInput input, string? userId, CancellationToken cancellationToken);
    Task<InspectionStatus> StartAsync(int inspectionId, int? inspectorBusinessEntityId, string? userId, CancellationToken cancellationToken);
    Task<InspectionResult> RecordResultAsync(RecordResultInput input, int? recordedByBusinessEntityId, CancellationToken cancellationToken);
    Task<CompleteResult> CompleteAsync(int inspectionId, string? userId, CancellationToken cancellationToken);
}

public sealed record CreateInspectionInput(
    int InspectionPlanId, InspectionSourceKind SourceKind, int SourceId,
    int? InventoryItemId, int? LotId, decimal Quantity, string? UnitMeasureCode, string? Notes);

public sealed record RecordResultInput(
    int InspectionId, int InspectionPlanCharacteristicId,
    decimal? NumericResult, string? AttributeResult, string? Notes);

public sealed record CompleteResult(int InspectionId, InspectionStatus FinalStatus, int? AutoNcrId);
