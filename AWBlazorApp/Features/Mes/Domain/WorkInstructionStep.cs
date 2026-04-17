using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Domain;

/// <summary>
/// One step within a revision's instructional content. <see cref="Body"/> is free-form
/// Markdown; <see cref="AttachmentUrl"/> is an optional external link (image / PDF / video).
/// </summary>
[Table("WorkInstructionStep", Schema = "mes")]
public class WorkInstructionStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int WorkInstructionRevisionId { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string Body { get; set; } = string.Empty;

    [MaxLength(500)] public string? AttachmentUrl { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
