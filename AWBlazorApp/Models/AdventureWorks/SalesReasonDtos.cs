using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record SalesReasonDto(int Id, string Name, string ReasonType, DateTime ModifiedDate);

public sealed record CreateSalesReasonRequest
{
    public string? Name { get; set; }
    public string? ReasonType { get; set; }
}

public sealed record UpdateSalesReasonRequest
{
    public string? Name { get; set; }
    public string? ReasonType { get; set; }
}

public sealed record SalesReasonAuditLogDto(
    int Id, int SalesReasonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, string? ReasonType, DateTime SourceModifiedDate);

public static class SalesReasonMappings
{
    public static SalesReasonDto ToDto(this SalesReason e)
        => new(e.Id, e.Name, e.ReasonType, e.ModifiedDate);

    public static SalesReason ToEntity(this CreateSalesReasonRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ReasonType = (r.ReasonType ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesReasonRequest r, SalesReason e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.ReasonType is not null) e.ReasonType = r.ReasonType.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesReasonAuditLogDto ToDto(this SalesReasonAuditLog a) => new(
        a.Id, a.SalesReasonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.ReasonType, a.SourceModifiedDate);
}
