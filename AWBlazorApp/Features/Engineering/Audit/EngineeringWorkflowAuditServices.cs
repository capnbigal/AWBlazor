using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Engineering.Audit;

public static class EngineeringChangeOrderAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(EngineeringChangeOrder e) => new(e);
    public static EngineeringChangeOrderAuditLog RecordCreate(EngineeringChangeOrder e, string? by) => Build(e, ActionCreated, by, "Created");
    public static EngineeringChangeOrderAuditLog RecordUpdate(Snapshot b, EngineeringChangeOrder a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static EngineeringChangeOrderAuditLog RecordDelete(EngineeringChangeOrder e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static EngineeringChangeOrderAuditLog Build(EngineeringChangeOrder e, string action, string? by, string? summary) => new()
    {
        EngineeringChangeOrderId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Title = e.Title, Description = e.Description,
        Status = e.Status,
        RaisedByUserId = e.RaisedByUserId, RaisedAt = e.RaisedAt,
        SubmittedAt = e.SubmittedAt,
        DecidedAt = e.DecidedAt, DecidedByUserId = e.DecidedByUserId, DecisionNotes = e.DecisionNotes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, EngineeringChangeOrder a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "Title", b.Title, a.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "DecisionNotes", b.DecisionNotes, a.DecisionNotes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(EcoStatus Status, string Title, string? DecisionNotes)
    {
        public Snapshot(EngineeringChangeOrder e) : this(e.Status, e.Title, e.DecisionNotes) { }
    }
}

public static class DeviationRequestAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(DeviationRequest e) => new(e);
    public static DeviationRequestAuditLog RecordCreate(DeviationRequest e, string? by) => Build(e, ActionCreated, by, "Submitted");
    public static DeviationRequestAuditLog RecordUpdate(Snapshot b, DeviationRequest a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static DeviationRequestAuditLog RecordDelete(DeviationRequest e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static DeviationRequestAuditLog Build(DeviationRequest e, string action, string? by, string? summary) => new()
    {
        DeviationRequestId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, ProductId = e.ProductId, Reason = e.Reason,
        ProposedDisposition = e.ProposedDisposition,
        AuthorizedQuantity = e.AuthorizedQuantity, UnitMeasureCode = e.UnitMeasureCode,
        ValidFrom = e.ValidFrom, ValidTo = e.ValidTo, Status = e.Status,
        RaisedByUserId = e.RaisedByUserId, RaisedAt = e.RaisedAt,
        DecidedByUserId = e.DecidedByUserId, DecidedAt = e.DecidedAt, DecisionNotes = e.DecisionNotes,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, DeviationRequest a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "DecisionNotes", b.DecisionNotes, a.DecisionNotes);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(DeviationStatus Status, string? DecisionNotes)
    {
        public Snapshot(DeviationRequest e) : this(e.Status, e.DecisionNotes) { }
    }
}
