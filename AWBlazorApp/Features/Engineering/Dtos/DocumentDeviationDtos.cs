using AWBlazorApp.Features.Engineering.Domain;

namespace AWBlazorApp.Features.Engineering.Dtos;

public sealed record EngineeringDocumentDto(
    int Id, string Code, string Title, EngineeringDocumentKind Kind,
    int? ProductId, int RevisionNumber, string? Url, string? Description,
    bool IsActive, DateTime ModifiedDate);

public sealed record CreateEngineeringDocumentRequest
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public EngineeringDocumentKind Kind { get; set; } = EngineeringDocumentKind.Drawing;
    public int? ProductId { get; set; }
    public int RevisionNumber { get; set; } = 1;
    public string? Url { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateEngineeringDocumentRequest
{
    public string? Title { get; set; }
    public EngineeringDocumentKind? Kind { get; set; }
    public int? RevisionNumber { get; set; }
    public string? Url { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record EngineeringDocumentAuditLogDto(
    int Id, int EngineeringDocumentId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Title, EngineeringDocumentKind Kind, int? ProductId, int RevisionNumber,
    string? Url, string? Description, bool IsActive, DateTime SourceModifiedDate);

public sealed record DeviationRequestDto(
    int Id, string Code, int ProductId, string Reason, string? ProposedDisposition,
    decimal? AuthorizedQuantity, string? UnitMeasureCode,
    DateOnly? ValidFrom, DateOnly? ValidTo, DeviationStatus Status,
    string? RaisedByUserId, DateTime RaisedAt,
    string? DecidedByUserId, DateTime? DecidedAt, string? DecisionNotes, DateTime ModifiedDate);

public sealed record CreateDeviationRequestRequest
{
    public string? Code { get; set; }
    public int ProductId { get; set; }
    public string? Reason { get; set; }
    public string? ProposedDisposition { get; set; }
    public decimal? AuthorizedQuantity { get; set; }
    public string? UnitMeasureCode { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}

public sealed record ReviewDeviationRequest { public string? Notes { get; set; } }

public sealed record DeviationRequestAuditLogDto(
    int Id, int DeviationRequestId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, int ProductId, string? Reason, string? ProposedDisposition,
    decimal? AuthorizedQuantity, string? UnitMeasureCode,
    DateOnly? ValidFrom, DateOnly? ValidTo, DeviationStatus Status,
    string? RaisedByUserId, DateTime RaisedAt,
    string? DecidedByUserId, DateTime? DecidedAt, string? DecisionNotes, DateTime SourceModifiedDate);

public static class DocumentDeviationMappings
{
    public static EngineeringDocumentDto ToDto(this EngineeringDocument e) => new(
        e.Id, e.Code, e.Title, e.Kind, e.ProductId, e.RevisionNumber,
        e.Url, e.Description, e.IsActive, e.ModifiedDate);

    public static EngineeringDocument ToEntity(this CreateEngineeringDocumentRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Title = (r.Title ?? string.Empty).Trim(),
        Kind = r.Kind,
        ProductId = r.ProductId,
        RevisionNumber = r.RevisionNumber,
        Url = r.Url?.Trim(),
        Description = r.Description?.Trim(),
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateEngineeringDocumentRequest r, EngineeringDocument e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Kind is not null) e.Kind = r.Kind.Value;
        if (r.RevisionNumber is not null) e.RevisionNumber = r.RevisionNumber.Value;
        if (r.Url is not null) e.Url = r.Url.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static EngineeringDocumentAuditLogDto ToDto(this EngineeringDocumentAuditLog a) => new(
        a.Id, a.EngineeringDocumentId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Title, a.Kind, a.ProductId, a.RevisionNumber,
        a.Url, a.Description, a.IsActive, a.SourceModifiedDate);

    public static DeviationRequestDto ToDto(this DeviationRequest e) => new(
        e.Id, e.Code, e.ProductId, e.Reason, e.ProposedDisposition,
        e.AuthorizedQuantity, e.UnitMeasureCode,
        e.ValidFrom, e.ValidTo, e.Status,
        e.RaisedByUserId, e.RaisedAt,
        e.DecidedByUserId, e.DecidedAt, e.DecisionNotes, e.ModifiedDate);

    public static DeviationRequest ToEntity(this CreateDeviationRequestRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new DeviationRequest
        {
            Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
            ProductId = r.ProductId,
            Reason = (r.Reason ?? string.Empty).Trim(),
            ProposedDisposition = r.ProposedDisposition?.Trim(),
            AuthorizedQuantity = r.AuthorizedQuantity,
            UnitMeasureCode = r.UnitMeasureCode?.Trim().ToUpperInvariant(),
            ValidFrom = r.ValidFrom,
            ValidTo = r.ValidTo,
            Status = DeviationStatus.Pending,
            RaisedByUserId = userId,
            RaisedAt = now,
            ModifiedDate = now,
        };
    }

    public static DeviationRequestAuditLogDto ToDto(this DeviationRequestAuditLog a) => new(
        a.Id, a.DeviationRequestId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.ProductId, a.Reason, a.ProposedDisposition,
        a.AuthorizedQuantity, a.UnitMeasureCode,
        a.ValidFrom, a.ValidTo, a.Status,
        a.RaisedByUserId, a.RaisedAt,
        a.DecidedByUserId, a.DecidedAt, a.DecisionNotes, a.SourceModifiedDate);
}
