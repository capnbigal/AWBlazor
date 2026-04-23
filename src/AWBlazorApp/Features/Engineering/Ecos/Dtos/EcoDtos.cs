using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 

namespace AWBlazorApp.Features.Engineering.Ecos.Dtos;

public sealed record EngineeringChangeOrderDto(
    int Id, string Code, string Title, string? Description, EcoStatus Status,
    string? RaisedByUserId, DateTime RaisedAt, DateTime? SubmittedAt,
    DateTime? DecidedAt, string? DecidedByUserId, string? DecisionNotes, DateTime ModifiedDate);

public sealed record CreateEngineeringChangeOrderRequest
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public sealed record UpdateEngineeringChangeOrderRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public sealed record EcoAffectedItemDto(
    int Id, int EngineeringChangeOrderId, EcoAffectedKind AffectedKind, int TargetId, string? Notes, DateTime ModifiedDate);

public sealed record CreateEcoAffectedItemRequest
{
    public EcoAffectedKind AffectedKind { get; set; }
    public int TargetId { get; set; }
    public string? Notes { get; set; }
}

public sealed record EcoApprovalDto(
    int Id, int EngineeringChangeOrderId, string? ApproverUserId,
    EcoApprovalDecision Decision, DateTime DecidedAt, string? Notes, DateTime ModifiedDate);

public sealed record ReviewEcoRequest { public string? Notes { get; set; } }

public static class EcoMappings
{
    public static EngineeringChangeOrderDto ToDto(this EngineeringChangeOrder e) => new(
        e.Id, e.Code, e.Title, e.Description, e.Status,
        e.RaisedByUserId, e.RaisedAt, e.SubmittedAt,
        e.DecidedAt, e.DecidedByUserId, e.DecisionNotes, e.ModifiedDate);

    public static EngineeringChangeOrder ToEntity(this CreateEngineeringChangeOrderRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new EngineeringChangeOrder
        {
            Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
            Title = (r.Title ?? string.Empty).Trim(),
            Description = r.Description?.Trim(),
            Status = EcoStatus.Draft,
            RaisedByUserId = userId,
            RaisedAt = now,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateEngineeringChangeOrderRequest r, EngineeringChangeOrder e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static EcoAffectedItemDto ToDto(this EcoAffectedItem e) => new(
        e.Id, e.EngineeringChangeOrderId, e.AffectedKind, e.TargetId, e.Notes, e.ModifiedDate);

    public static EcoAffectedItem ToEntity(this CreateEcoAffectedItemRequest r, int ecoId) => new()
    {
        EngineeringChangeOrderId = ecoId,
        AffectedKind = r.AffectedKind,
        TargetId = r.TargetId,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static EcoApprovalDto ToDto(this EcoApproval e) => new(
        e.Id, e.EngineeringChangeOrderId, e.ApproverUserId, e.Decision, e.DecidedAt, e.Notes, e.ModifiedDate);
}
