using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Domain;

/// <summary>
/// Header record for a versioned set of instructions tied to a work-order routing step.
/// One row per <c>WorkOrderRoutingId</c>; the actual instructional content lives in
/// <see cref="WorkInstructionRevision"/> / <see cref="WorkInstructionStep"/> so edits bump
/// a revision number and older bodies remain accessible.
/// </summary>
[Table("WorkInstruction", Schema = "mes")]
public class WorkInstruction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK → <c>Production.WorkOrderRouting</c>'s surrogate key. Unique.</summary>
    public int WorkOrderRoutingId { get; set; }

    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    /// <summary>The currently-published revision. Null while the header exists but no
    /// revision has been published yet.</summary>
    public int? ActiveRevisionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
