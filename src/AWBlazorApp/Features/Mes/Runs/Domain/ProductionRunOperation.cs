using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Runs.Domain;

/// <summary>
/// One operation executed as part of a <see cref="ProductionRun"/>. Maps loosely to a
/// <c>Production.WorkOrderRouting</c> step, but captured here as a soft reference because
/// WorkOrderRouting has a composite PK (WorkOrderID + ProductID + OperationSequence) that
/// isn't worth modeling as a hard FK at this layer.
/// </summary>
[Table("ProductionRunOperation", Schema = "mes")]
public class ProductionRunOperation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ProductionRunId { get; set; }

    /// <summary>Matches <c>WorkOrderRouting.OperationSequence</c> when the run is tied to a WO.</summary>
    public short? OperationSequence { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(200)] public string OperationDescription { get; set; } = string.Empty;

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal ActualHours { get; set; }

    public DateTime ModifiedDate { get; set; }
}
