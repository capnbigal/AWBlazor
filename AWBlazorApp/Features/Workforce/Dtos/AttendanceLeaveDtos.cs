using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 

namespace AWBlazorApp.Features.Workforce.Dtos;

public sealed record AttendanceEventDto(
    long Id, int BusinessEntityId, int? ShiftId, DateOnly ShiftDate,
    DateTime? ClockInAt, DateTime? ClockOutAt, AttendanceStatus Status, string? Notes, DateTime ModifiedDate);

public sealed record CreateAttendanceEventRequest
{
    public int BusinessEntityId { get; set; }
    public int? ShiftId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public DateTime? ClockInAt { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
}

public sealed record UpdateAttendanceEventRequest
{
    public DateTime? ClockInAt { get; set; }
    public DateTime? ClockOutAt { get; set; }
    public AttendanceStatus? Status { get; set; }
    public string? Notes { get; set; }
}

public sealed record LeaveRequestDto(
    int Id, int BusinessEntityId, LeaveType LeaveType,
    DateOnly StartDate, DateOnly EndDate, LeaveStatus Status,
    string? Reason, string? RequestedByUserId, DateTime RequestedAt,
    string? ReviewedByUserId, DateTime? ReviewedAt, string? ReviewNotes, DateTime ModifiedDate);

public sealed record CreateLeaveRequestRequest
{
    public int BusinessEntityId { get; set; }
    public LeaveType LeaveType { get; set; } = LeaveType.Vacation;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
}

public sealed record ReviewLeaveRequestRequest { public string? Notes { get; set; } }

public sealed record LeaveRequestAuditLogDto(
    int Id, int LeaveRequestId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int BusinessEntityId, LeaveType LeaveType, DateOnly StartDate, DateOnly EndDate, LeaveStatus Status,
    string? Reason, string? RequestedByUserId, DateTime RequestedAt,
    string? ReviewedByUserId, DateTime? ReviewedAt, string? ReviewNotes, DateTime SourceModifiedDate);

public static class AttendanceLeaveMappings
{
    public static AttendanceEventDto ToDto(this AttendanceEvent e) => new(
        e.Id, e.BusinessEntityId, e.ShiftId, e.ShiftDate,
        e.ClockInAt, e.ClockOutAt, e.Status, e.Notes, e.ModifiedDate);

    public static AttendanceEvent ToEntity(this CreateAttendanceEventRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        ShiftId = r.ShiftId,
        ShiftDate = r.ShiftDate,
        ClockInAt = r.ClockInAt ?? DateTime.UtcNow,
        Status = r.Status,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateAttendanceEventRequest r, AttendanceEvent e)
    {
        if (r.ClockInAt is not null) e.ClockInAt = r.ClockInAt;
        if (r.ClockOutAt is not null) e.ClockOutAt = r.ClockOutAt;
        if (r.Status is not null) e.Status = r.Status.Value;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static LeaveRequestDto ToDto(this LeaveRequest e) => new(
        e.Id, e.BusinessEntityId, e.LeaveType,
        e.StartDate, e.EndDate, e.Status,
        e.Reason, e.RequestedByUserId, e.RequestedAt,
        e.ReviewedByUserId, e.ReviewedAt, e.ReviewNotes, e.ModifiedDate);

    public static LeaveRequest ToEntity(this CreateLeaveRequestRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new LeaveRequest
        {
            BusinessEntityId = r.BusinessEntityId,
            LeaveType = r.LeaveType,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Status = LeaveStatus.Pending,
            Reason = r.Reason?.Trim(),
            RequestedByUserId = userId,
            RequestedAt = now,
            ModifiedDate = now,
        };
    }

    public static LeaveRequestAuditLogDto ToDto(this LeaveRequestAuditLog a) => new(
        a.Id, a.LeaveRequestId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.BusinessEntityId, a.LeaveType, a.StartDate, a.EndDate, a.Status,
        a.Reason, a.RequestedByUserId, a.RequestedAt,
        a.ReviewedByUserId, a.ReviewedAt, a.ReviewNotes, a.SourceModifiedDate);
}
