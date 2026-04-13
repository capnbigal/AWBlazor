using System.Text;
using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Services.AdventureWorksAudit;

public static class CreditCardAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CreditCard e) => new(e);

    public static CreditCardAuditLog RecordCreate(CreditCard e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CreditCardAuditLog RecordUpdate(Snapshot before, CreditCard after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CreditCardAuditLog RecordDelete(CreditCard e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CreditCardAuditLog BuildLog(CreditCard e, string action, string? by, string? summary)
        => new()
        {
            CreditCardId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            CardType = e.CardType,
            CardNumber = e.CardNumber,
            ExpMonth = e.ExpMonth,
            ExpYear = e.ExpYear,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, CreditCard after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "CardType", before.CardType, after.CardType);
        AuditDiffHelpers.AppendIfChanged(sb, "CardNumber", before.CardNumber, after.CardNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpMonth", before.ExpMonth, after.ExpMonth);
        AuditDiffHelpers.AppendIfChanged(sb, "ExpYear", before.ExpYear, after.ExpYear);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string CardType, string CardNumber, byte ExpMonth, short ExpYear)
    {
        public Snapshot(CreditCard e) : this(e.CardType, e.CardNumber, e.ExpMonth, e.ExpYear) { }
    }
}
