namespace AWBlazorApp.Features.Engineering.Deviations.Application.Services;

/// <summary>
/// Deviation request workflow. Simple Pending → Approved / Rejected / Cancelled — matches the
/// <c>ILeaveRequestService</c> pattern. Expired is a derived state evaluated from
/// <c>ValidTo</c>, not a transition.
/// </summary>
public interface IDeviationService
{
    Task ApproveAsync(int deviationId, string? decisionNotes, string? userId, CancellationToken cancellationToken);
    Task RejectAsync(int deviationId, string? decisionNotes, string? userId, CancellationToken cancellationToken);
    Task CancelAsync(int deviationId, string? userId, CancellationToken cancellationToken);
}
