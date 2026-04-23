using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 

namespace AWBlazorApp.Features.HumanResources.EmployeePayHistories.Dtos;

public sealed record EmployeePayHistoryDto(
    int BusinessEntityId, DateTime RateChangeDate, decimal Rate, byte PayFrequency, DateTime ModifiedDate);

public sealed record CreateEmployeePayHistoryRequest
{
    public int BusinessEntityId { get; set; }
    public DateTime RateChangeDate { get; set; }
    public decimal Rate { get; set; }
    public byte PayFrequency { get; set; }
}

public sealed record UpdateEmployeePayHistoryRequest
{
    public decimal? Rate { get; set; }
    public byte? PayFrequency { get; set; }
}

public static class EmployeePayHistoryMappings
{
    public static EmployeePayHistoryDto ToDto(this EmployeePayHistory e) => new(
        e.BusinessEntityId, e.RateChangeDate, e.Rate, e.PayFrequency, e.ModifiedDate);

    public static EmployeePayHistory ToEntity(this CreateEmployeePayHistoryRequest r) => new()
    {
        BusinessEntityId = r.BusinessEntityId,
        RateChangeDate = r.RateChangeDate,
        Rate = r.Rate,
        PayFrequency = r.PayFrequency,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateEmployeePayHistoryRequest r, EmployeePayHistory e)
    {
        if (r.Rate.HasValue) e.Rate = r.Rate.Value;
        if (r.PayFrequency.HasValue) e.PayFrequency = r.PayFrequency.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    }
