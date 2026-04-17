using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Domain;

/// <summary>
/// One version of a <see cref="WorkInstruction"/>. Creating a new revision copies the steps
/// from the previous active revision into new <see cref="WorkInstructionStep"/> rows tied to
/// this one, so operators can edit freely without touching the published version. Publishing
/// flips this row's <see cref="Status"/> to Published, supersedes the prior active revision,
/// and updates the header's <c>ActiveRevisionId</c>.
/// </summary>
[Table("WorkInstructionRevision", Schema = "mes")]
public class WorkInstructionRevision
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int WorkInstructionId { get; set; }

    public int RevisionNumber { get; set; }

    public WorkInstructionRevisionStatus Status { get; set; } = WorkInstructionRevisionStatus.Draft;

    [MaxLength(450)] public string? CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedAt { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum WorkInstructionRevisionStatus : byte
{
    Draft = 1,
    Published = 2,
    Superseded = 3,
}
