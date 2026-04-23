using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.Shifts.Dtos;

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

    }
