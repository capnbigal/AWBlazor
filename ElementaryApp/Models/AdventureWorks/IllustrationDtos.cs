using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record IllustrationDto(int Id, DateTime ModifiedDate);

/// <summary>
/// Illustration has no editable data of its own — its real Diagram XML column is intentionally
/// not mapped. Creating one allocates a new id and stamps ModifiedDate; the Diagram column
/// stays NULL on insert from this app.
/// </summary>
public sealed record CreateIllustrationRequest;

public sealed record UpdateIllustrationRequest;

public sealed record IllustrationAuditLogDto(
    int Id, int IllustrationId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

public static class IllustrationMappings
{
    public static IllustrationDto ToDto(this Illustration e) => new(e.Id, e.ModifiedDate);

    public static Illustration ToEntity(this CreateIllustrationRequest _) => new()
    {
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateIllustrationRequest _, Illustration e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static IllustrationAuditLogDto ToDto(this IllustrationAuditLog a) => new(
        a.Id, a.IllustrationId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.SourceModifiedDate);
}
