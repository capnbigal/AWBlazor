using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

[Table("WeeklyPlan", Schema = "Scheduling")]
public class WeeklyPlan
{
    [Key, Column("WeeklyPlanID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("Version")] public int Version { get; set; }
    [Column("PublishedAt")] public DateTime PublishedAt { get; set; }
    [Column("PublishedBy"), MaxLength(256)] public string PublishedBy { get; set; } = "";
    [Column("BaselineDiverged")] public bool BaselineDiverged { get; set; }
    [Column("GenerationOptionsJson")] public string? GenerationOptionsJson { get; set; }
    public List<WeeklyPlanItem> Items { get; set; } = new();
}
