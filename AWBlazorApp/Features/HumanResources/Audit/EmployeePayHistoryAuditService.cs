using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.HumanResources.Domain;

namespace AWBlazorApp.Features.HumanResources.Audit;

public static class EmployeePayHistoryAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(EmployeePayHistory e) => new(e);

    public static EmployeePayHistoryAuditLog RecordCreate(EmployeePayHistory e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static EmployeePayHistoryAuditLog RecordUpdate(Snapshot before, EmployeePayHistory after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static EmployeePayHistoryAuditLog RecordDelete(EmployeePayHistory e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static EmployeePayHistoryAuditLog BuildLog(EmployeePayHistory e, string action, string? by, string? summary)
        => new()
        {
            BusinessEntityId = e.BusinessEntityId,
            RateChangeDate = e.RateChangeDate,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            Rate = e.Rate,
            PayFrequency = e.PayFrequency,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, EmployeePayHistory after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Rate", before.Rate, after.Rate);
        AuditDiffHelpers.AppendIfChanged(sb, "PayFrequency", before.PayFrequency, after.PayFrequency);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(decimal Rate, byte PayFrequency)
    {
        public Snapshot(EmployeePayHistory e) : this(e.Rate, e.PayFrequency) { }
    }
}
