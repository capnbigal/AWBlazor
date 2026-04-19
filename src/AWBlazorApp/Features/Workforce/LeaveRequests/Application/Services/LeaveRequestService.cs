using AWBlazorApp.Features.Workforce.Audit;
using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Workforce.LeaveRequests.Application.Services;

/// <inheritdoc />
public sealed class LeaveRequestService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<LeaveRequestService> logger) : ILeaveRequestService
{
    public Task ApproveAsync(int id, string? notes, string? userId, CancellationToken ct)
        => TransitionAsync(id, LeaveStatus.Approved, notes, userId, ct);

    public Task RejectAsync(int id, string? notes, string? userId, CancellationToken ct)
        => TransitionAsync(id, LeaveStatus.Rejected, notes, userId, ct);

    public Task CancelAsync(int id, string? userId, CancellationToken ct)
        => TransitionAsync(id, LeaveStatus.Cancelled, "Cancelled by requester.", userId, ct);

    private async Task TransitionAsync(int id, LeaveStatus to, string? notes, string? userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var entity = await db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new InvalidOperationException($"LeaveRequest {id} not found.");
        if (entity.Status != LeaveStatus.Pending)
            throw new InvalidOperationException($"Leave request is {entity.Status}; only Pending can transition.");

        var before = LeaveRequestAuditService.CaptureSnapshot(entity);
        entity.Status = to;
        entity.ReviewedByUserId = userId;
        entity.ReviewedAt = DateTime.UtcNow;
        entity.ReviewNotes = notes?.Trim();
        entity.ModifiedDate = DateTime.UtcNow;
        db.LeaveRequestAuditLogs.Add(LeaveRequestAuditService.RecordUpdate(before, entity, userId));
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request {Id} → {Status} by {User}", id, to, userId);
    }
}
