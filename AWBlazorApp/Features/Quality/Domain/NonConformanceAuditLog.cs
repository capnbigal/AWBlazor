using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Quality.Domain;

public class NonConformanceAuditLog : AdventureWorksAuditLogBase
{
    public int NonConformanceId { get; set; }

    [MaxLength(32)] public string? NcrNumber { get; set; }
    public int? InspectionId { get; set; }
    public int InventoryItemId { get; set; }
    public int? LotId { get; set; }
    public int? LocationId { get; set; }
    public decimal Quantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public NonConformanceStatus Status { get; set; }
    public NonConformanceDisposition? Disposition { get; set; }
    [MaxLength(450)] public string? DispositionedByUserId { get; set; }
    public DateTime? DispositionedAt { get; set; }
    [MaxLength(2000)] public string? DispositionNotes { get; set; }
    public long? PostedTransactionId { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
