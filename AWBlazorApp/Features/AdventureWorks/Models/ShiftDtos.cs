using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

public sealed record ShiftDto(byte Id, string Name, TimeSpan StartTime, TimeSpan EndTime, DateTime ModifiedDate);

public sealed record CreateShiftRequest
{
    public string? Name { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public sealed record UpdateShiftRequest
{
    public string? Name { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public sealed record ShiftAuditLogDto(
    int Id, byte ShiftId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, TimeSpan StartTime, TimeSpan EndTime, DateTime SourceModifiedDate);

public static class ShiftMappings
{
    public static ShiftDto ToDto(this Shift e) => new(e.Id, e.Name, e.StartTime, e.EndTime, e.ModifiedDate);

    public static Shift ToEntity(this CreateShiftRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        StartTime = r.StartTime ?? TimeSpan.Zero,
        EndTime = r.EndTime ?? TimeSpan.Zero,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateShiftRequest r, Shift e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.StartTime.HasValue) e.StartTime = r.StartTime.Value;
        if (r.EndTime.HasValue) e.EndTime = r.EndTime.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ShiftAuditLogDto ToDto(this ShiftAuditLog a) => new(
        a.Id, a.ShiftId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.StartTime, a.EndTime, a.SourceModifiedDate);
}
