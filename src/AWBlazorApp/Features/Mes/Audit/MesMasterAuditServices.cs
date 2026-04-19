using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Mes.Audit;

public static class DowntimeReasonAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(DowntimeReason e) => new(e);
    public static DowntimeReasonAuditLog RecordCreate(DowntimeReason e, string? by) => Build(e, ActionCreated, by, "Created");
    public static DowntimeReasonAuditLog RecordUpdate(Snapshot before, DowntimeReason after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static DowntimeReasonAuditLog RecordDelete(DowntimeReason e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static DowntimeReasonAuditLog Build(DowntimeReason e, string action, string? by, string? summary) => new()
    {
        DowntimeReasonId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        IsActive = e.IsActive,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, DowntimeReason a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "Description", b.Description, a.Description);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, string? Description, bool IsActive)
    {
        public Snapshot(DowntimeReason e) : this(e.Name, e.Description, e.IsActive) { }
    }
}

public static class WorkInstructionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(WorkInstruction e) => new(e);
    public static WorkInstructionAuditLog RecordCreate(WorkInstruction e, string? by) => Build(e, ActionCreated, by, "Created");
    public static WorkInstructionAuditLog RecordUpdate(Snapshot before, WorkInstruction after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static WorkInstructionAuditLog RecordDelete(WorkInstruction e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static WorkInstructionAuditLog Build(WorkInstruction e, string action, string? by, string? summary) => new()
    {
        WorkInstructionId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        WorkOrderRoutingId = e.WorkOrderRoutingId,
        Title = e.Title,
        ActiveRevisionId = e.ActiveRevisionId,
        IsActive = e.IsActive,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, WorkInstruction a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Title", b.Title, a.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "ActiveRevisionId", b.ActiveRevisionId, a.ActiveRevisionId);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Title, int? ActiveRevisionId, bool IsActive)
    {
        public Snapshot(WorkInstruction e) : this(e.Title, e.ActiveRevisionId, e.IsActive) { }
    }
}

public static class WorkInstructionRevisionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(WorkInstructionRevision e) => new(e);
    public static WorkInstructionRevisionAuditLog RecordCreate(WorkInstructionRevision e, string? by) => Build(e, ActionCreated, by, "Created");
    public static WorkInstructionRevisionAuditLog RecordUpdate(Snapshot before, WorkInstructionRevision after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static WorkInstructionRevisionAuditLog RecordDelete(WorkInstructionRevision e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static WorkInstructionRevisionAuditLog Build(WorkInstructionRevision e, string action, string? by, string? summary) => new()
    {
        WorkInstructionRevisionId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        WorkInstructionId = e.WorkInstructionId,
        RevisionNumber = e.RevisionNumber,
        Status = e.Status,
        CreatedByUserId = e.CreatedByUserId,
        CreatedDate = e.CreatedDate,
        PublishedAt = e.PublishedAt,
        Notes = e.Notes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, WorkInstructionRevision a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "PublishedAt", b.PublishedAt, a.PublishedAt);
        AuditDiffHelpers.AppendIfChanged(sb, "Notes", b.Notes, a.Notes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(WorkInstructionRevisionStatus Status, DateTime? PublishedAt, string? Notes)
    {
        public Snapshot(WorkInstructionRevision e) : this(e.Status, e.PublishedAt, e.Notes) { }
    }
}

public static class WorkInstructionStepAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(WorkInstructionStep e) => new(e);
    public static WorkInstructionStepAuditLog RecordCreate(WorkInstructionStep e, string? by) => Build(e, ActionCreated, by, "Created");
    public static WorkInstructionStepAuditLog RecordUpdate(Snapshot before, WorkInstructionStep after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static WorkInstructionStepAuditLog RecordDelete(WorkInstructionStep e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static WorkInstructionStepAuditLog Build(WorkInstructionStep e, string action, string? by, string? summary) => new()
    {
        WorkInstructionStepId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        WorkInstructionRevisionId = e.WorkInstructionRevisionId,
        SequenceNumber = e.SequenceNumber,
        Title = e.Title,
        AttachmentUrl = e.AttachmentUrl,
        EstimatedDurationMinutes = e.EstimatedDurationMinutes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, WorkInstructionStep a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "SequenceNumber", b.SequenceNumber, a.SequenceNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "Title", b.Title, a.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "AttachmentUrl", b.AttachmentUrl, a.AttachmentUrl);
        AuditDiffHelpers.AppendIfChanged(sb, "EstimatedDurationMinutes", b.EstimatedDurationMinutes, a.EstimatedDurationMinutes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(int SequenceNumber, string Title, string? AttachmentUrl, int? EstimatedDurationMinutes)
    {
        public Snapshot(WorkInstructionStep e) : this(e.SequenceNumber, e.Title, e.AttachmentUrl, e.EstimatedDurationMinutes) { }
    }
}
