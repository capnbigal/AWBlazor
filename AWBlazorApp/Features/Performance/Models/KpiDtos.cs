using AWBlazorApp.Features.Performance.Domain;

namespace AWBlazorApp.Features.Performance.Models;

public sealed record KpiDefinitionDto(
    int Id, string Code, string Name, string? Description, string? Unit,
    KpiSource Source, KpiAggregation Aggregation,
    decimal? TargetValue, decimal? WarningThreshold, decimal? CriticalThreshold,
    KpiDirection Direction, bool IsActive, DateTime ModifiedDate);

public sealed record CreateKpiDefinitionRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public KpiSource Source { get; set; } = KpiSource.OeeOverall;
    public KpiAggregation Aggregation { get; set; } = KpiAggregation.Average;
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public KpiDirection Direction { get; set; } = KpiDirection.HigherIsBetter;
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateKpiDefinitionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public KpiSource? Source { get; set; }
    public KpiAggregation? Aggregation { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public KpiDirection? Direction { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record KpiDefinitionAuditLogDto(
    int Id, int KpiDefinitionId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description, string? Unit,
    KpiSource Source, KpiAggregation Aggregation,
    decimal? TargetValue, decimal? WarningThreshold, decimal? CriticalThreshold,
    KpiDirection Direction, bool IsActive, DateTime SourceModifiedDate);

public sealed record KpiValueDto(
    long Id, int KpiDefinitionId, PerformancePeriodKind PeriodKind,
    DateTime PeriodStart, DateTime PeriodEnd,
    decimal? Value, decimal? TargetAtPeriod,
    KpiStatus Status, DateTime ComputedAt, DateTime ModifiedDate);

public sealed record EvaluateKpiRequest
{
    public int KpiDefinitionId { get; set; }
    public PerformancePeriodKind PeriodKind { get; set; } = PerformancePeriodKind.Day;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public static class KpiMappings
{
    public static KpiDefinitionDto ToDto(this KpiDefinition e) => new(
        e.Id, e.Code, e.Name, e.Description, e.Unit,
        e.Source, e.Aggregation,
        e.TargetValue, e.WarningThreshold, e.CriticalThreshold,
        e.Direction, e.IsActive, e.ModifiedDate);

    public static KpiDefinition ToEntity(this CreateKpiDefinitionRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        Unit = r.Unit?.Trim(),
        Source = r.Source, Aggregation = r.Aggregation,
        TargetValue = r.TargetValue,
        WarningThreshold = r.WarningThreshold, CriticalThreshold = r.CriticalThreshold,
        Direction = r.Direction, IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateKpiDefinitionRequest r, KpiDefinition e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.Unit is not null) e.Unit = r.Unit.Trim();
        if (r.Source is not null) e.Source = r.Source.Value;
        if (r.Aggregation is not null) e.Aggregation = r.Aggregation.Value;
        if (r.TargetValue is not null) e.TargetValue = r.TargetValue;
        if (r.WarningThreshold is not null) e.WarningThreshold = r.WarningThreshold;
        if (r.CriticalThreshold is not null) e.CriticalThreshold = r.CriticalThreshold;
        if (r.Direction is not null) e.Direction = r.Direction.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static KpiDefinitionAuditLogDto ToDto(this KpiDefinitionAuditLog a) => new(
        a.Id, a.KpiDefinitionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.Unit,
        a.Source, a.Aggregation,
        a.TargetValue, a.WarningThreshold, a.CriticalThreshold,
        a.Direction, a.IsActive, a.SourceModifiedDate);

    public static KpiValueDto ToDto(this KpiValue e) => new(
        e.Id, e.KpiDefinitionId, e.PeriodKind,
        e.PeriodStart, e.PeriodEnd,
        e.Value, e.TargetAtPeriod,
        e.Status, e.ComputedAt, e.ModifiedDate);
}
