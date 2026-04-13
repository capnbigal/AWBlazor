using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record EmployeeDto(
    int Id, string NationalIDNumber, string LoginID, string JobTitle,
    DateTime BirthDate, string MaritalStatus, string Gender, DateTime HireDate,
    bool SalariedFlag, bool CurrentFlag, short VacationHours, short SickLeaveHours,
    Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateEmployeeRequest
{
    /// <summary>PK / FK to Person.BusinessEntity. NOT identity — caller must supply.</summary>
    public int Id { get; set; }
    public string? NationalIDNumber { get; set; }
    public string? LoginID { get; set; }
    public string? JobTitle { get; set; }
    public DateTime BirthDate { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Gender { get; set; }
    public DateTime HireDate { get; set; }
    public bool SalariedFlag { get; set; } = true;
    public bool CurrentFlag { get; set; } = true;
    public short VacationHours { get; set; }
    public short SickLeaveHours { get; set; }
}

public sealed record UpdateEmployeeRequest
{
    public string? NationalIDNumber { get; set; }
    public string? LoginID { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Gender { get; set; }
    public DateTime? HireDate { get; set; }
    public bool? SalariedFlag { get; set; }
    public bool? CurrentFlag { get; set; }
    public short? VacationHours { get; set; }
    public short? SickLeaveHours { get; set; }
}

public sealed record EmployeeAuditLogDto(
    int Id, int EmployeeId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? NationalIDNumber, string? LoginID, string? JobTitle,
    DateTime BirthDate, string? MaritalStatus, string? Gender, DateTime HireDate,
    bool SalariedFlag, bool CurrentFlag, short VacationHours, short SickLeaveHours,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class EmployeeMappings
{
    public static EmployeeDto ToDto(this Employee e) => new(
        e.Id, e.NationalIDNumber, e.LoginID, e.JobTitle,
        e.BirthDate, e.MaritalStatus, e.Gender, e.HireDate,
        e.SalariedFlag, e.CurrentFlag, e.VacationHours, e.SickLeaveHours,
        e.RowGuid, e.ModifiedDate);

    public static Employee ToEntity(this CreateEmployeeRequest r) => new()
    {
        Id = r.Id,
        NationalIDNumber = (r.NationalIDNumber ?? string.Empty).Trim(),
        LoginID = (r.LoginID ?? string.Empty).Trim(),
        JobTitle = (r.JobTitle ?? string.Empty).Trim(),
        BirthDate = r.BirthDate,
        MaritalStatus = (r.MaritalStatus ?? string.Empty).Trim().ToUpperInvariant(),
        Gender = (r.Gender ?? string.Empty).Trim().ToUpperInvariant(),
        HireDate = r.HireDate,
        SalariedFlag = r.SalariedFlag,
        CurrentFlag = r.CurrentFlag,
        VacationHours = r.VacationHours,
        SickLeaveHours = r.SickLeaveHours,
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateEmployeeRequest r, Employee e)
    {
        if (r.NationalIDNumber is not null) e.NationalIDNumber = r.NationalIDNumber.Trim();
        if (r.LoginID is not null) e.LoginID = r.LoginID.Trim();
        if (r.JobTitle is not null) e.JobTitle = r.JobTitle.Trim();
        if (r.BirthDate.HasValue) e.BirthDate = r.BirthDate.Value;
        if (r.MaritalStatus is not null) e.MaritalStatus = r.MaritalStatus.Trim().ToUpperInvariant();
        if (r.Gender is not null) e.Gender = r.Gender.Trim().ToUpperInvariant();
        if (r.HireDate.HasValue) e.HireDate = r.HireDate.Value;
        if (r.SalariedFlag.HasValue) e.SalariedFlag = r.SalariedFlag.Value;
        if (r.CurrentFlag.HasValue) e.CurrentFlag = r.CurrentFlag.Value;
        if (r.VacationHours.HasValue) e.VacationHours = r.VacationHours.Value;
        if (r.SickLeaveHours.HasValue) e.SickLeaveHours = r.SickLeaveHours.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static EmployeeAuditLogDto ToDto(this EmployeeAuditLog a) => new(
        a.Id, a.EmployeeId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.NationalIDNumber, a.LoginID, a.JobTitle,
        a.BirthDate, a.MaritalStatus, a.Gender, a.HireDate,
        a.SalariedFlag, a.CurrentFlag, a.VacationHours, a.SickLeaveHours,
        a.RowGuid, a.SourceModifiedDate);
}
