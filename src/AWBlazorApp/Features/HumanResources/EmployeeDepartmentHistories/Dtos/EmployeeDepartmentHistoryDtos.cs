using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Dtos;

public sealed record EmployeeDepartmentHistoryDto(
    int BusinessEntityId, short DepartmentId, byte ShiftId,
    DateTime StartDate, DateTime? EndDate, DateTime ModifiedDate);

public sealed record CreateEmployeeDepartmentHistoryRequest
{
    public int BusinessEntityId { get; set; }
    public short DepartmentId { get; set; }
    public byte ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed record UpdateEmployeeDepartmentHistoryRequest
{
    public DateTime? EndDate { get; set; }
}

public static class EmployeeDepartmentHistoryMappings
{
    public static EmployeeDepartmentHistoryDto ToDto(this EmployeeDepartmentHistory e) => new(
        e.BusinessEntityId, e.DepartmentId, e.ShiftId, e.StartDate, e.EndDate, e.ModifiedDate);

    public static EmployeeDepartmentHistory ToEntity(this CreateEmployeeDepartmentHistoryRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        DepartmentId = r.DepartmentId,
        ShiftId = r.ShiftId,
        StartDate = r.StartDate.Date,
        EndDate = r.EndDate?.Date,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateEmployeeDepartmentHistoryRequest r, EmployeeDepartmentHistory e)
    {
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value.Date;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
