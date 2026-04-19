using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 

namespace AWBlazorApp.Features.Quality.Capa.Dtos;

public sealed record CapaCaseDto(
    int Id, string CaseNumber, string Title, CapaStatus Status,
    string? RootCause, string? CorrectiveAction, string? PreventiveAction, string? VerificationNotes,
    int? OwnerBusinessEntityId, DateTime OpenedAt, DateTime? ClosedAt, DateTime ModifiedDate);

public sealed record CreateCapaCaseRequest
{
    public string? Title { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public List<int>? LinkedNcrIds { get; set; }
}

public sealed record UpdateCapaCaseRequest
{
    public string? Title { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public string? PreventiveAction { get; set; }
    public string? VerificationNotes { get; set; }
}

public sealed record TransitionCapaCaseRequest
{
    public CapaStatus TargetStatus { get; set; }
}

public sealed record CapaCaseAuditLogDto(
    int Id, int CapaCaseId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? CaseNumber, string? Title, CapaStatus Status,
    string? RootCause, string? CorrectiveAction, string? PreventiveAction, string? VerificationNotes,
    int? OwnerBusinessEntityId, DateTime OpenedAt, DateTime? ClosedAt, DateTime SourceModifiedDate);

public sealed record CapaCaseNonConformanceDto(int Id, int CapaCaseId, int NonConformanceId, DateTime LinkedAt);

public sealed record LinkNcrRequest { public int NonConformanceId { get; set; } }

public static class CapaCaseMappings
{
    public static CapaCaseDto ToDto(this CapaCase e) => new(
        e.Id, e.CaseNumber, e.Title, e.Status,
        e.RootCause, e.CorrectiveAction, e.PreventiveAction, e.VerificationNotes,
        e.OwnerBusinessEntityId, e.OpenedAt, e.ClosedAt, e.ModifiedDate);

    public static void ApplyTo(this UpdateCapaCaseRequest r, CapaCase e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.OwnerBusinessEntityId is not null) e.OwnerBusinessEntityId = r.OwnerBusinessEntityId;
        if (r.RootCause is not null) e.RootCause = r.RootCause.Trim();
        if (r.CorrectiveAction is not null) e.CorrectiveAction = r.CorrectiveAction.Trim();
        if (r.PreventiveAction is not null) e.PreventiveAction = r.PreventiveAction.Trim();
        if (r.VerificationNotes is not null) e.VerificationNotes = r.VerificationNotes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static CapaCaseAuditLogDto ToDto(this CapaCaseAuditLog a) => new(
        a.Id, a.CapaCaseId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.CaseNumber, a.Title, a.Status,
        a.RootCause, a.CorrectiveAction, a.PreventiveAction, a.VerificationNotes,
        a.OwnerBusinessEntityId, a.OpenedAt, a.ClosedAt, a.SourceModifiedDate);

    public static CapaCaseNonConformanceDto ToDto(this CapaCaseNonConformance e) => new(e.Id, e.CapaCaseId, e.NonConformanceId, e.LinkedAt);
}
