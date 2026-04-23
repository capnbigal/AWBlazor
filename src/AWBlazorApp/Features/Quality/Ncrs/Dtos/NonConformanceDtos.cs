using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 

namespace AWBlazorApp.Features.Quality.Ncrs.Dtos;

public sealed record NonConformanceDto(
    int Id, string NcrNumber, int? InspectionId, int InventoryItemId, int? LotId, int? LocationId,
    decimal Quantity, string UnitMeasureCode, string Description,
    NonConformanceStatus Status, NonConformanceDisposition? Disposition,
    string? DispositionedByUserId, DateTime? DispositionedAt, string? DispositionNotes,
    long? PostedTransactionId, DateTime ModifiedDate);

public sealed record CreateNonConformanceRequest
{
    public int? InspectionId { get; set; }
    public int InventoryItemId { get; set; }
    public int? LotId { get; set; }
    public int? LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public string? Description { get; set; }
}

public sealed record UpdateNonConformanceRequest
{
    public int? LotId { get; set; }
    public int? LocationId { get; set; }
    public decimal? Quantity { get; set; }
    public string? Description { get; set; }
    public NonConformanceStatus? Status { get; set; }
}

public sealed record DispositionNonConformanceRequest
{
    public NonConformanceDisposition Disposition { get; set; }
    public string? Notes { get; set; }
}

public sealed record NonConformanceActionDto(
    int Id, int NonConformanceId, string Action, string? PerformedByUserId,
    DateTime PerformedAt, string? Notes, DateTime ModifiedDate);

public sealed record CreateNonConformanceActionRequest
{
    public int NonConformanceId { get; set; }
    public string? Action { get; set; }
    public string? Notes { get; set; }
}

public static class NonConformanceMappings
{
    public static NonConformanceDto ToDto(this NonConformance e) => new(
        e.Id, e.NcrNumber, e.InspectionId, e.InventoryItemId, e.LotId, e.LocationId,
        e.Quantity, e.UnitMeasureCode, e.Description,
        e.Status, e.Disposition, e.DispositionedByUserId, e.DispositionedAt, e.DispositionNotes,
        e.PostedTransactionId, e.ModifiedDate);

    public static NonConformance ToEntity(this CreateNonConformanceRequest r)
    {
        var now = DateTime.UtcNow;
        return new NonConformance
        {
            NcrNumber = $"NCR-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            InspectionId = r.InspectionId,
            InventoryItemId = r.InventoryItemId,
            LotId = r.LotId,
            LocationId = r.LocationId,
            Quantity = r.Quantity,
            UnitMeasureCode = (r.UnitMeasureCode ?? "EA").Trim().ToUpperInvariant(),
            Description = (r.Description ?? string.Empty).Trim(),
            Status = NonConformanceStatus.Open,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateNonConformanceRequest r, NonConformance e)
    {
        if (r.LotId is not null) e.LotId = r.LotId;
        if (r.LocationId is not null) e.LocationId = r.LocationId;
        if (r.Quantity is not null) e.Quantity = r.Quantity.Value;
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.Status is not null) e.Status = r.Status.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static NonConformanceActionDto ToDto(this NonConformanceAction e) => new(
        e.Id, e.NonConformanceId, e.Action, e.PerformedByUserId,
        e.PerformedAt, e.Notes, e.ModifiedDate);

    public static NonConformanceAction ToEntity(this CreateNonConformanceActionRequest r, string? userId) => new()
    {
        NonConformanceId = r.NonConformanceId,
        Action = (r.Action ?? string.Empty).Trim(),
        PerformedByUserId = userId,
        PerformedAt = DateTime.UtcNow,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };
}
