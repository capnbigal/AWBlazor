using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.Departments.Dtos;

public sealed record DepartmentDto(short Id, string Name, string GroupName, DateTime ModifiedDate);

public sealed record CreateDepartmentRequest
{
    public string? Name { get; set; }
    public string? GroupName { get; set; }
}

public sealed record UpdateDepartmentRequest
{
    public string? Name { get; set; }
    public string? GroupName { get; set; }
}

public static class DepartmentMappings
{
    public static DepartmentDto ToDto(this Department e) => new(e.Id, e.Name, e.GroupName, e.ModifiedDate);

    public static Department ToEntity(this CreateDepartmentRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        GroupName = (r.GroupName ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateDepartmentRequest r, Department e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.GroupName is not null) e.GroupName = r.GroupName.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
