using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.Rules.Domain;

[Table("SchedulingRule", Schema = "Scheduling")]
public class SchedulingRule
{
    [Key, Column("SchedulingRuleID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("EventType")] public SchedulingEventType EventType { get; set; }
    [Column("InFrozenWindow")] public bool InFrozenWindow { get; set; }
    [Column("Action")] public RecalcActionType Action { get; set; }
    [Column("ParametersJson")] public string? ParametersJson { get; set; }
    [Column("Priority")] public int Priority { get; set; }
    [Column("IsActive")] public bool IsActive { get; set; } = true;
}
