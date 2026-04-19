using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Workforce.Domain;

public class LeaveRequestAuditLog : AdventureWorksAuditLogBase
{
    public int LeaveRequestId { get; set; }

    public int BusinessEntityId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    [MaxLength(2000)] public string? Reason { get; set; }
    [MaxLength(450)] public string? RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    [MaxLength(450)] public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    [MaxLength(500)] public string? ReviewNotes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
