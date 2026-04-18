using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.Domain;

/// <summary>
/// Alert raised when an operator clocks in to a station and is missing or has an expired
/// required qualification. Per the user's preference: clock-ins are NOT blocked — the event
/// posts normally and an alert is queued for review by the operator's manager (whoever owns
/// the alert inbox UI).
/// </summary>
[Table("QualificationAlert", Schema = "wf")]
public class QualificationAlert
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c> (the operator).</summary>
    public int BusinessEntityId { get; set; }

    public int StationId { get; set; }
    public int QualificationId { get; set; }

    public long? OperatorClockEventId { get; set; }

    public QualificationAlertReason Reason { get; set; }

    public QualificationAlertStatus Status { get; set; } = QualificationAlertStatus.Open;

    public DateTime RaisedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    [MaxLength(450)] public string? AcknowledgedByUserId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum QualificationAlertReason : byte
{
    Missing = 1,
    Expired = 2,
}

public enum QualificationAlertStatus : byte
{
    Open = 1,
    Acknowledged = 2,
    Resolved = 3,
    Dismissed = 4,
}
