using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using System.Text;

namespace AWBlazorApp.Features.Engineering.Audit;

public static class ManufacturingRoutingAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ManufacturingRouting e) => new(e);
    public static ManufacturingRoutingAuditLog RecordCreate(ManufacturingRouting e, string? by) => Build(e, ActionCreated, by, "Created");
    public static ManufacturingRoutingAuditLog RecordUpdate(Snapshot b, ManufacturingRouting a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static ManufacturingRoutingAuditLog RecordDelete(ManufacturingRouting e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static ManufacturingRoutingAuditLog Build(ManufacturingRouting e, string action, string? by, string? summary) => new()
    {
        ManufacturingRoutingId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        ProductId = e.ProductId, RevisionNumber = e.RevisionNumber,
        IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, ManufacturingRouting a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "RevisionNumber", b.RevisionNumber, a.RevisionNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, int RevisionNumber, bool IsActive)
    {
        public Snapshot(ManufacturingRouting e) : this(e.Name, e.RevisionNumber, e.IsActive) { }
    }
}

public static class BomHeaderAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(BomHeader e) => new(e);
    public static BomHeaderAuditLog RecordCreate(BomHeader e, string? by) => Build(e, ActionCreated, by, "Created");
    public static BomHeaderAuditLog RecordUpdate(Snapshot b, BomHeader a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static BomHeaderAuditLog RecordDelete(BomHeader e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static BomHeaderAuditLog Build(BomHeader e, string action, string? by, string? summary) => new()
    {
        BomHeaderId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Name = e.Name, Description = e.Description,
        ProductId = e.ProductId, RevisionNumber = e.RevisionNumber,
        IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, BomHeader a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Name", b.Name, a.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "RevisionNumber", b.RevisionNumber, a.RevisionNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Name, int RevisionNumber, bool IsActive)
    {
        public Snapshot(BomHeader e) : this(e.Name, e.RevisionNumber, e.IsActive) { }
    }
}

public static class EngineeringDocumentAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(EngineeringDocument e) => new(e);
    public static EngineeringDocumentAuditLog RecordCreate(EngineeringDocument e, string? by) => Build(e, ActionCreated, by, "Created");
    public static EngineeringDocumentAuditLog RecordUpdate(Snapshot b, EngineeringDocument a, string? by) => Build(a, ActionUpdated, by, Diff(b, a));
    public static EngineeringDocumentAuditLog RecordDelete(EngineeringDocument e, string? by) => Build(e, ActionDeleted, by, "Deleted");

    private static EngineeringDocumentAuditLog Build(EngineeringDocument e, string action, string? by, string? summary) => new()
    {
        EngineeringDocumentId = e.Id,
        Action = action, ChangedBy = by, ChangedDate = DateTime.UtcNow,
        ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
        Code = e.Code, Title = e.Title, Kind = e.Kind, ProductId = e.ProductId,
        RevisionNumber = e.RevisionNumber, Url = e.Url, Description = e.Description,
        IsActive = e.IsActive, SourceModifiedDate = e.ModifiedDate,
    };

    private static string Diff(Snapshot b, EngineeringDocument a)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "Title", b.Title, a.Title);
        AuditDiffHelpers.AppendIfChanged(sb, "RevisionNumber", b.RevisionNumber, a.RevisionNumber);
        AuditDiffHelpers.AppendIfChanged(sb, "Url", b.Url, a.Url);
        AuditDiffHelpers.AppendIfChanged(sb, "IsActive", b.IsActive, a.IsActive);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string Title, int RevisionNumber, string? Url, bool IsActive)
    {
        public Snapshot(EngineeringDocument e) : this(e.Title, e.RevisionNumber, e.Url, e.IsActive) { }
    }
}
