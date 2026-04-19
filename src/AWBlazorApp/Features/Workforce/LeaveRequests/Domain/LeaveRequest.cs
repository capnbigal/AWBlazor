using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Workforce.LeaveRequests.Domain;

/// <summary>
/// Time-off request. Simple workflow per the user's preference: Pending → Approved or
/// Rejected by a single approver. No multi-stage HR confirmation.
/// </summary>
[Table("LeaveRequest", Schema = "wf")]
public class LeaveRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int BusinessEntityId { get; set; }

    public LeaveType LeaveType { get; set; } = LeaveType.Vacation;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    [MaxLength(2000)] public string? Reason { get; set; }

    [MaxLength(450)] public string? RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }

    [MaxLength(450)] public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    [MaxLength(500)] public string? ReviewNotes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum LeaveType : byte
{
    Vacation = 1,
    Sick = 2,
    Personal = 3,
    Bereavement = 4,
    Jury = 5,
    Unpaid = 6,
    Other = 7,
}

public enum LeaveStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
}
