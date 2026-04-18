using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.Domain;

/// <summary>
/// Free-text note left by an outgoing crew for the incoming crew at a station. The narrow
/// "communication" channel for shop-floor handoffs — distinct from the broader
/// <see cref="Announcement"/> table which spans an OrgUnit or the whole org.
/// </summary>
[Table("ShiftHandoverNote", Schema = "wf")]
public class ShiftHandoverNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int StationId { get; set; }

    public DateOnly ShiftDate { get; set; }

    /// <summary>FK → <c>HumanResources.Shift.ShiftID</c>. Going off-shift.</summary>
    public int? FromShiftId { get; set; }
    /// <summary>FK → <c>HumanResources.Shift.ShiftID</c>. Coming on-shift.</summary>
    public int? ToShiftId { get; set; }

    [MaxLength(2000)] public string Note { get; set; } = string.Empty;

    [MaxLength(450)] public string? AuthoredByUserId { get; set; }
    public DateTime AuthoredAt { get; set; }

    public bool RequiresAcknowledgment { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    [MaxLength(450)] public string? AcknowledgedByUserId { get; set; }

    public DateTime ModifiedDate { get; set; }
}
