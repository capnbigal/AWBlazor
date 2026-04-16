using AWBlazorApp.Features.AdventureWorks.Audit;
using System.Text;
using AWBlazorApp.Features.Sales.Domain;

namespace AWBlazorApp.Features.Sales.Audit;

public static class CurrencyRateAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CurrencyRate e) => new(e);

    public static CurrencyRateAuditLog RecordCreate(CurrencyRate e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static CurrencyRateAuditLog RecordUpdate(Snapshot before, CurrencyRate after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static CurrencyRateAuditLog RecordDelete(CurrencyRate e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static CurrencyRateAuditLog BuildLog(CurrencyRate e, string action, string? by, string? summary)
        => new()
        {
            CurrencyRateId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            CurrencyRateDate = e.CurrencyRateDate,
            FromCurrencyCode = e.FromCurrencyCode,
            ToCurrencyCode = e.ToCurrencyCode,
            AverageRate = e.AverageRate,
            EndOfDayRate = e.EndOfDayRate,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, CurrencyRate after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "CurrencyRateDate", before.CurrencyRateDate, after.CurrencyRateDate);
        AuditDiffHelpers.AppendIfChanged(sb, "FromCurrencyCode", before.FromCurrencyCode, after.FromCurrencyCode);
        AuditDiffHelpers.AppendIfChanged(sb, "ToCurrencyCode", before.ToCurrencyCode, after.ToCurrencyCode);
        AuditDiffHelpers.AppendIfChanged(sb, "AverageRate", before.AverageRate, after.AverageRate);
        AuditDiffHelpers.AppendIfChanged(sb, "EndOfDayRate", before.EndOfDayRate, after.EndOfDayRate);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        DateTime CurrencyRateDate, string FromCurrencyCode, string ToCurrencyCode,
        decimal AverageRate, decimal EndOfDayRate)
    {
        public Snapshot(CurrencyRate e) : this(
            e.CurrencyRateDate, e.FromCurrencyCode, e.ToCurrencyCode,
            e.AverageRate, e.EndOfDayRate)
        { }
    }
}
