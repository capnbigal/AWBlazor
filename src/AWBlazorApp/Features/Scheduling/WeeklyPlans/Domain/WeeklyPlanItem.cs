using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.WeeklyPlans.Domain;

[Table("WeeklyPlanItem", Schema = "Scheduling")]
public class WeeklyPlanItem
{
    [Key, Column("WeeklyPlanItemID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("WeeklyPlanID")] public int WeeklyPlanId { get; set; }
    [Column("SalesOrderID")] public int SalesOrderId { get; set; }
    [Column("SalesOrderDetailID")] public int SalesOrderDetailId { get; set; }
    [Column("ProductID")] public int ProductId { get; set; }
    [Column("PlannedSequence")] public int PlannedSequence { get; set; }
    [Column("PlannedStart")] public DateTime PlannedStart { get; set; }
    [Column("PlannedEnd")] public DateTime PlannedEnd { get; set; }
    [Column("PlannedQty")] public short PlannedQty { get; set; }
    [Column("OverCapacity")] public bool OverCapacity { get; set; }
}
