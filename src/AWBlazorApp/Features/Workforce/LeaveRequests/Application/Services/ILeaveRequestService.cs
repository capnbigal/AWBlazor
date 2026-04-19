using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 

namespace AWBlazorApp.Features.Workforce.LeaveRequests.Application.Services;

/// <summary>
/// Simple Pending → Approved/Rejected workflow per the user's preference. No multi-stage HR
/// confirmation. Cancel is also allowed for an unreviewed request.
/// </summary>
public interface ILeaveRequestService
{
    Task ApproveAsync(int leaveRequestId, string? reviewNotes, string? userId, CancellationToken cancellationToken);
    Task RejectAsync(int leaveRequestId, string? reviewNotes, string? userId, CancellationToken cancellationToken);
    Task CancelAsync(int leaveRequestId, string? userId, CancellationToken cancellationToken);
}
