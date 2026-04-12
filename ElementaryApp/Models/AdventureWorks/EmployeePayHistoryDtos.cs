using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

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

public sealed record EmployeePayHistoryAuditLogDto(
    int Id, int BusinessEntityId, DateTime RateChangeDate, string Action, string? ChangedBy,
    DateTime ChangedDate, string? ChangeSummary, decimal Rate, byte PayFrequency, DateTime SourceModifiedDate);

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

    public static EmployeePayHistoryAuditLogDto ToDto(this EmployeePayHistoryAuditLog a) => new(
        a.Id, a.BusinessEntityId, a.RateChangeDate, a.Action, a.ChangedBy,
        a.ChangedDate, a.ChangeSummary, a.Rate, a.PayFrequency, a.SourceModifiedDate);
}
