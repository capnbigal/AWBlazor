using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Mes.Domain;

public class ProductionRunOperationAuditLog : AdventureWorksAuditLogBase
{
    public int ProductionRunOperationId { get; set; }

    public int ProductionRunId { get; set; }
    public short? OperationSequence { get; set; }
    public int SequenceNumber { get; set; }
    [MaxLength(200)] public string? OperationDescription { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public decimal ActualHours { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
