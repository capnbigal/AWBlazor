using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
namespace AWBlazorApp.Features.Scheduling.Alerts.Domain;

[Table("SchedulingAlert", Schema = "Scheduling")]
public class SchedulingAlert
{
    [Key, Column("SchedulingAlertID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("CreatedAt")] public DateTime CreatedAt { get; set; }
    [Column("Severity")] public AlertSeverity Severity { get; set; }
    [Column("EventType")] public SchedulingEventType EventType { get; set; }
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("SalesOrderID")] public int? SalesOrderId { get; set; }
    [Column("Message"), MaxLength(1000)] public string Message { get; set; } = "";
    [Column("PayloadJson")] public string? PayloadJson { get; set; }
    [Column("AcknowledgedAt")] public DateTime? AcknowledgedAt { get; set; }
    [Column("AcknowledgedBy"), MaxLength(256)] public string? AcknowledgedBy { get; set; }
}
