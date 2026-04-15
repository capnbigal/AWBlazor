using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Models;

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

public sealed record EmployeeDepartmentHistoryAuditLogDto(
    int Id, int BusinessEntityId, short DepartmentId, byte ShiftId, DateTime StartDate,
    string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    DateTime? EndDate, DateTime SourceModifiedDate);

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

    public static EmployeeDepartmentHistoryAuditLogDto ToDto(this EmployeeDepartmentHistoryAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.DepartmentId, a.ShiftId, a.StartDate,
        a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.EndDate, a.SourceModifiedDate);
}
