using AWBlazorApp.Features.Workforce.Domain;

namespace AWBlazorApp.Features.Workforce.Services;

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
