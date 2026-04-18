using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.Domain;

/// <summary>
/// A spare part consumed on a work order. One row per (WO, SparePart) consumption event —
/// the same part can appear multiple times if consumed in separate steps.
/// </summary>
[Table("WorkOrderPartUsage", Schema = "maint")]
public class WorkOrderPartUsage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int MaintenanceWorkOrderId { get; set; }

    public int SparePartId { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal? UnitCost { get; set; }

    public DateTime UsedAt { get; set; }

    [MaxLength(450)] public string? UsedByUserId { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
