using AWBlazorApp.Shared.Audit;
using System.Text;
using AWBlazorApp.Features.HumanResources.Domain;

namespace AWBlazorApp.Features.HumanResources.Audit;

public static class JobCandidateAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(JobCandidate e) => new(e);

    public static JobCandidateAuditLog RecordCreate(JobCandidate e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static JobCandidateAuditLog RecordUpdate(Snapshot before, JobCandidate after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static JobCandidateAuditLog RecordDelete(JobCandidate e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static JobCandidateAuditLog BuildLog(JobCandidate e, string action, string? by, string? summary)
        => new()
        {
            JobCandidateId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            BusinessEntityId = e.BusinessEntityId,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, JobCandidate after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "BusinessEntityId", before.BusinessEntityId, after.BusinessEntityId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(int? BusinessEntityId)
    {
        public Snapshot(JobCandidate e) : this(e.BusinessEntityId) { }
    }
}
