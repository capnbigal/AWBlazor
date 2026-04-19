using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Engineering.Domain;

public class DeviationRequestAuditLog : AdventureWorksAuditLogBase
{
    public int DeviationRequestId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    public int ProductId { get; set; }
    [MaxLength(2000)] public string? Reason { get; set; }
    [MaxLength(2000)] public string? ProposedDisposition { get; set; }
    public decimal? AuthorizedQuantity { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public DeviationStatus Status { get; set; }
    [MaxLength(450)] public string? RaisedByUserId { get; set; }
    public DateTime RaisedAt { get; set; }
    [MaxLength(450)] public string? DecidedByUserId { get; set; }
    public DateTime? DecidedAt { get; set; }
    [MaxLength(2000)] public string? DecisionNotes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
