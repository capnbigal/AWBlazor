using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Quality.Domain;
using System.Text;

namespace AWBlazorApp.Features.Quality.Audit;

public static class NonConformanceAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(NonConformance e) => new(e);
    public static NonConformanceAuditLog RecordCreate(NonConformance e, string? by) => Build(e, ActionCreated, by, "Created");
    public static NonConformanceAuditLog RecordUpdate(Snapshot before, NonConformance after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static NonConformanceAuditLog RecordDelete(NonConformance e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static NonConformanceAuditLog Build(NonConformance e, string action, string? by, string? summary) => new()
    {
        NonConformanceId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        NcrNumber = e.NcrNumber,
        InspectionId = e.InspectionId,
        InventoryItemId = e.InventoryItemId,
        LotId = e.LotId,
        LocationId = e.LocationId,
        Quantity = e.Quantity,
        UnitMeasureCode = e.UnitMeasureCode,
        Description = e.Description,
        Status = e.Status,
        Disposition = e.Disposition,
        DispositionedByUserId = e.DispositionedByUserId,
        DispositionedAt = e.DispositionedAt,
        DispositionNotes = e.DispositionNotes,
        PostedTransactionId = e.PostedTransactionId,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, NonConformance a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "Disposition", b.Disposition, a.Disposition);
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", b.Quantity, a.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "Description", b.Description, a.Description);
        AuditDiffHelpers.AppendIfChanged(sb, "DispositionNotes", b.DispositionNotes, a.DispositionNotes);
        AuditDiffHelpers.AppendIfChanged(sb, "PostedTransactionId", b.PostedTransactionId, a.PostedTransactionId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        NonConformanceStatus Status, NonConformanceDisposition? Disposition, decimal Quantity,
        string Description, string? DispositionNotes, long? PostedTransactionId)
    {
        public Snapshot(NonConformance e) : this(e.Status, e.Disposition, e.Quantity, e.Description, e.DispositionNotes, e.PostedTransactionId) { }
    }
}

public static class NonConformanceActionAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static NonConformanceActionAuditLog RecordCreate(NonConformanceAction e, string? by) => Build(e, ActionCreated, by, "Created");
    public static NonConformanceActionAuditLog RecordDelete(NonConformanceAction e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static NonConformanceActionAuditLog Build(NonConformanceAction e, string action, string? by, string? summary) => new()
    {
        NonConformanceActionId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        NonConformanceId = e.NonConformanceId,
        PerformedByUserId = e.PerformedByUserId,
        PerformedAt = e.PerformedAt,
        Notes = e.Notes,
        SourceModifiedDate = e.ModifiedDate,
    };
}

public static class CapaCaseAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(CapaCase e) => new(e);
    public static CapaCaseAuditLog RecordCreate(CapaCase e, string? by) => Build(e, ActionCreated, by, "Created");
    public static CapaCaseAuditLog RecordUpdate(Snapshot before, CapaCase after, string? by) => Build(after, ActionUpdated, by, Diff(before, after));
    public static CapaCaseAuditLog RecordDelete(CapaCase e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static CapaCaseAuditLog Build(CapaCase e, string action, string? by, string? summary) => new()
    {
        CapaCaseId = e.Id,
        Action = action,
        ChangedBy = by,
        ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        CaseNumber = e.CaseNumber,
        Title = e.Title,
        Status = e.Status,
        RootCause = e.RootCause,
        CorrectiveAction = e.CorrectiveAction,
        PreventiveAction = e.PreventiveAction,
        VerificationNotes = e.VerificationNotes,
        OwnerBusinessEntityId = e.OwnerBusinessEntityId,
        OpenedAt = e.OpenedAt,
        ClosedAt = e.ClosedAt,
        SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, CapaCase a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Title", b.Title, a.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "Status", b.Status, a.Status);
        AuditDiffHelpers.AppendIfChanged(sb, "OwnerBusinessEntityId", b.OwnerBusinessEntityId, a.OwnerBusinessEntityId);
        AuditDiffHelpers.AppendIfChanged(sb, "RootCause", b.RootCause, a.RootCause);
        AuditDiffHelpers.AppendIfChanged(sb, "ClosedAt", b.ClosedAt, a.ClosedAt);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Title, CapaStatus Status, int? OwnerBusinessEntityId, string? RootCause, DateTime? ClosedAt)
    {
        public Snapshot(CapaCase e) : this(e.Title, e.Status, e.OwnerBusinessEntityId, e.RootCause, e.ClosedAt) { }
    }
}
