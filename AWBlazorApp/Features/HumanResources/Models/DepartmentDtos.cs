using AWBlazorApp.Features.HumanResources.Domain;

namespace AWBlazorApp.Features.HumanResources.Models;

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

public sealed record DepartmentAuditLogDto(
    int Id, short DepartmentId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, string? GroupName, DateTime SourceModifiedDate);

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

    public static DepartmentAuditLogDto ToDto(this DepartmentAuditLog a) => new(
        a.Id, a.DepartmentId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.GroupName, a.SourceModifiedDate);
}
