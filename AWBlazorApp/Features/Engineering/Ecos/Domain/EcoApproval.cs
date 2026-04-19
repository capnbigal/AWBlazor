using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Engineering.Ecos.Domain;

/// <summary>
/// Records a single approval decision against an ECO. For the simple single-approver flow
/// there's typically one row per ECO, but the table supports multiple rows if a future
/// multi-stage approval policy is added.
/// </summary>
[Table("EcoApproval", Schema = "eng")]
public class EcoApproval
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int EngineeringChangeOrderId { get; set; }

    [MaxLength(450)] public string? ApproverUserId { get; set; }

    public EcoApprovalDecision Decision { get; set; }

    public DateTime DecidedAt { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum EcoApprovalDecision : byte
{
    Approved = 1,
    Rejected = 2,
}
