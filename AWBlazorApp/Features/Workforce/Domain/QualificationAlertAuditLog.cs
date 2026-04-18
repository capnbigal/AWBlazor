using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Workforce.Domain;

public class QualificationAlertAuditLog : AdventureWorksAuditLogBase
{
    public long QualificationAlertId { get; set; }

    public int BusinessEntityId { get; set; }
    public int StationId { get; set; }
    public int QualificationId { get; set; }
    public long? OperatorClockEventId { get; set; }
    public QualificationAlertReason Reason { get; set; }
    public QualificationAlertStatus Status { get; set; }
    public DateTime RaisedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    [MaxLength(450)] public string? AcknowledgedByUserId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
