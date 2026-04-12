using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record ScrapReasonDto(short Id, string Name, DateTime ModifiedDate);

public sealed record CreateScrapReasonRequest
{
    public string? Name { get; set; }
}

public sealed record UpdateScrapReasonRequest
{
    public string? Name { get; set; }
}

public sealed record ScrapReasonAuditLogDto(
    int Id, short ScrapReasonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Name, DateTime SourceModifiedDate);

public static class ScrapReasonMappings
{
    public static ScrapReasonDto ToDto(this ScrapReason e) => new(e.Id, e.Name, e.ModifiedDate);

    public static ScrapReason ToEntity(this CreateScrapReasonRequest r) => new()
    {
        Name = (r.Name ?? string.Empty).Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateScrapReasonRequest r, ScrapReason e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ScrapReasonAuditLogDto ToDto(this ScrapReasonAuditLog a) => new(
        a.Id, a.ScrapReasonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Name, a.SourceModifiedDate);
}
