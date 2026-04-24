using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;

[Table("SchedulingException", Schema = "Scheduling")]
public class SchedulingException
{
    [Key, Column("SchedulingExceptionID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("SalesOrderDetailID")] public int SalesOrderDetailId { get; set; }
    [Column("ExceptionType")] public ExceptionType ExceptionType { get; set; }
    [Column("PinnedSequence")] public int? PinnedSequence { get; set; }
    [Column("Reason"), MaxLength(500)] public string Reason { get; set; } = "";
    [Column("CreatedAt")] public DateTime CreatedAt { get; set; }
    [Column("CreatedBy"), MaxLength(256)] public string CreatedBy { get; set; } = "";
    [Column("ResolvedAt")] public DateTime? ResolvedAt { get; set; }
    [Column("ResolvedBy"), MaxLength(256)] public string? ResolvedBy { get; set; }
}
