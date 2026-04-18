using AWBlazorApp.Features.Performance.Domain;

namespace AWBlazorApp.Features.Performance.Models;

public sealed record ScorecardDefinitionDto(
    int Id, string Code, string Name, string? Description,
    string? OwnerUserId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateScorecardDefinitionRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateScorecardDefinitionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record ScorecardDefinitionAuditLogDto(
    int Id, int ScorecardDefinitionId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description, string? OwnerUserId,
    bool IsActive, DateTime SourceModifiedDate);

public sealed record ScorecardKpiDto(
    int Id, int ScorecardDefinitionId, int KpiDefinitionId,
    int DisplayOrder, ScorecardKpiVisual Visual, DateTime ModifiedDate);

public sealed record CreateScorecardKpiRequest
{
    public int ScorecardDefinitionId { get; set; }
    public int KpiDefinitionId { get; set; }
    public int DisplayOrder { get; set; }
    public ScorecardKpiVisual Visual { get; set; } = ScorecardKpiVisual.KpiCard;
}

public sealed record PerformanceReportDto(
    int Id, string Code, string Name, string? Description,
    PerformanceReportKind Kind, string DefinitionJson,
    DateTime? LastRunAt, bool IsActive, DateTime ModifiedDate);

public sealed record CreatePerformanceReportRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PerformanceReportKind Kind { get; set; } = PerformanceReportKind.OeeSummary;
    public string DefinitionJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
}

public sealed record UpdatePerformanceReportRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? DefinitionJson { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record PerformanceReportAuditLogDto(
    int Id, int PerformanceReportId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description,
    PerformanceReportKind Kind, string? DefinitionJson,
    DateTime? LastRunAt, bool IsActive, DateTime SourceModifiedDate);

public sealed record PerformanceReportRunDto(
    long Id, int PerformanceReportId, DateTime RunAt, string? RunByUserId,
    int RowCount, int DurationMs, string? ResultJson, string? ErrorMessage, DateTime ModifiedDate);

public static class ScorecardMappings
{
    public static ScorecardDefinitionDto ToDto(this ScorecardDefinition e) => new(
        e.Id, e.Code, e.Name, e.Description,
        e.OwnerUserId, e.IsActive, e.ModifiedDate);

    public static ScorecardDefinition ToEntity(this CreateScorecardDefinitionRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new ScorecardDefinition
        {
            Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
            Name = (r.Name ?? string.Empty).Trim(),
            Description = r.Description?.Trim(),
            OwnerUserId = userId,
            IsActive = r.IsActive,
            ModifiedDate = now,
        };
    }

    public static void ApplyTo(this UpdateScorecardDefinitionRequest r, ScorecardDefinition e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ScorecardDefinitionAuditLogDto ToDto(this ScorecardDefinitionAuditLog a) => new(
        a.Id, a.ScorecardDefinitionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.OwnerUserId,
        a.IsActive, a.SourceModifiedDate);

    public static ScorecardKpiDto ToDto(this ScorecardKpi e) => new(
        e.Id, e.ScorecardDefinitionId, e.KpiDefinitionId,
        e.DisplayOrder, e.Visual, e.ModifiedDate);

    public static ScorecardKpi ToEntity(this CreateScorecardKpiRequest r) => new()
    {
        ScorecardDefinitionId = r.ScorecardDefinitionId,
        KpiDefinitionId = r.KpiDefinitionId,
        DisplayOrder = r.DisplayOrder,
        Visual = r.Visual,
        ModifiedDate = DateTime.UtcNow,
    };

    public static PerformanceReportDto ToDto(this PerformanceReport e) => new(
        e.Id, e.Code, e.Name, e.Description,
        e.Kind, e.DefinitionJson,
        e.LastRunAt, e.IsActive, e.ModifiedDate);

    public static PerformanceReport ToEntity(this CreatePerformanceReportRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        Kind = r.Kind,
        DefinitionJson = r.DefinitionJson ?? "{}",
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePerformanceReportRequest r, PerformanceReport e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.DefinitionJson is not null) e.DefinitionJson = r.DefinitionJson;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PerformanceReportAuditLogDto ToDto(this PerformanceReportAuditLog a) => new(
        a.Id, a.PerformanceReportId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description,
        a.Kind, a.DefinitionJson,
        a.LastRunAt, a.IsActive, a.SourceModifiedDate);

    public static PerformanceReportRunDto ToDto(this PerformanceReportRun e) => new(
        e.Id, e.PerformanceReportId, e.RunAt, e.RunByUserId,
        e.RowCount, e.DurationMs, e.ResultJson, e.ErrorMessage, e.ModifiedDate);
}
