using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 

namespace AWBlazorApp.Features.Quality.Plans.Dtos;

public sealed record InspectionPlanDto(
    int Id, string PlanCode, string Name, string? Description, InspectionScope Scope,
    int? ProductId, int? WorkOrderRoutingId, int? VendorBusinessEntityId,
    string? SamplingRule, bool AutoTriggerOnReceipt, bool AutoTriggerOnShipment, bool AutoTriggerOnProductionRun,
    bool IsActive, DateTime ModifiedDate);

public sealed record CreateInspectionPlanRequest
{
    public string? PlanCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public InspectionScope Scope { get; set; } = InspectionScope.Inbound;
    public int? ProductId { get; set; }
    public int? WorkOrderRoutingId { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public string? SamplingRule { get; set; }
    public bool AutoTriggerOnReceipt { get; set; }
    public bool AutoTriggerOnShipment { get; set; }
    public bool AutoTriggerOnProductionRun { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateInspectionPlanRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public InspectionScope? Scope { get; set; }
    public int? ProductId { get; set; }
    public int? WorkOrderRoutingId { get; set; }
    public int? VendorBusinessEntityId { get; set; }
    public string? SamplingRule { get; set; }
    public bool? AutoTriggerOnReceipt { get; set; }
    public bool? AutoTriggerOnShipment { get; set; }
    public bool? AutoTriggerOnProductionRun { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record InspectionPlanAuditLogDto(
    int Id, int InspectionPlanId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? PlanCode, string? Name, string? Description, InspectionScope Scope,
    int? ProductId, int? WorkOrderRoutingId, int? VendorBusinessEntityId,
    string? SamplingRule, bool AutoTriggerOnReceipt, bool AutoTriggerOnShipment, bool AutoTriggerOnProductionRun,
    bool IsActive, DateTime SourceModifiedDate);

public sealed record InspectionPlanCharacteristicDto(
    int Id, int InspectionPlanId, int SequenceNumber, string Name, CharacteristicKind Kind,
    decimal? MinValue, decimal? MaxValue, decimal? TargetValue, string? UnitMeasureCode,
    string? ExpectedValue, bool IsCritical, DateTime ModifiedDate);

public sealed record CreateInspectionPlanCharacteristicRequest
{
    public int InspectionPlanId { get; set; }
    public int SequenceNumber { get; set; }
    public string? Name { get; set; }
    public CharacteristicKind Kind { get; set; } = CharacteristicKind.Numeric;
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? UnitMeasureCode { get; set; }
    public string? ExpectedValue { get; set; }
    public bool IsCritical { get; set; }
}

public sealed record UpdateInspectionPlanCharacteristicRequest
{
    public int? SequenceNumber { get; set; }
    public string? Name { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? UnitMeasureCode { get; set; }
    public string? ExpectedValue { get; set; }
    public bool? IsCritical { get; set; }
}

public sealed record InspectionPlanCharacteristicAuditLogDto(
    int Id, int InspectionPlanCharacteristicId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    int InspectionPlanId, int SequenceNumber, string? Name, CharacteristicKind Kind,
    decimal? MinValue, decimal? MaxValue, decimal? TargetValue, string? UnitMeasureCode,
    string? ExpectedValue, bool IsCritical, DateTime SourceModifiedDate);

public static class InspectionPlanMappings
{
    public static InspectionPlanDto ToDto(this InspectionPlan e) => new(
        e.Id, e.PlanCode, e.Name, e.Description, e.Scope,
        e.ProductId, e.WorkOrderRoutingId, e.VendorBusinessEntityId,
        e.SamplingRule, e.AutoTriggerOnReceipt, e.AutoTriggerOnShipment, e.AutoTriggerOnProductionRun,
        e.IsActive, e.ModifiedDate);

    public static InspectionPlan ToEntity(this CreateInspectionPlanRequest r) => new()
    {
        PlanCode = (r.PlanCode ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        Scope = r.Scope,
        ProductId = r.ProductId,
        WorkOrderRoutingId = r.WorkOrderRoutingId,
        VendorBusinessEntityId = r.VendorBusinessEntityId,
        SamplingRule = r.SamplingRule?.Trim(),
        AutoTriggerOnReceipt = r.AutoTriggerOnReceipt,
        AutoTriggerOnShipment = r.AutoTriggerOnShipment,
        AutoTriggerOnProductionRun = r.AutoTriggerOnProductionRun,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateInspectionPlanRequest r, InspectionPlan e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.Scope is not null) e.Scope = r.Scope.Value;
        if (r.ProductId is not null) e.ProductId = r.ProductId;
        if (r.WorkOrderRoutingId is not null) e.WorkOrderRoutingId = r.WorkOrderRoutingId;
        if (r.VendorBusinessEntityId is not null) e.VendorBusinessEntityId = r.VendorBusinessEntityId;
        if (r.SamplingRule is not null) e.SamplingRule = r.SamplingRule.Trim();
        if (r.AutoTriggerOnReceipt is not null) e.AutoTriggerOnReceipt = r.AutoTriggerOnReceipt.Value;
        if (r.AutoTriggerOnShipment is not null) e.AutoTriggerOnShipment = r.AutoTriggerOnShipment.Value;
        if (r.AutoTriggerOnProductionRun is not null) e.AutoTriggerOnProductionRun = r.AutoTriggerOnProductionRun.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static InspectionPlanAuditLogDto ToDto(this InspectionPlanAuditLog a) => new(
        a.Id, a.InspectionPlanId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.PlanCode, a.Name, a.Description, a.Scope,
        a.ProductId, a.WorkOrderRoutingId, a.VendorBusinessEntityId,
        a.SamplingRule, a.AutoTriggerOnReceipt, a.AutoTriggerOnShipment, a.AutoTriggerOnProductionRun,
        a.IsActive, a.SourceModifiedDate);

    public static InspectionPlanCharacteristicDto ToDto(this InspectionPlanCharacteristic e) => new(
        e.Id, e.InspectionPlanId, e.SequenceNumber, e.Name, e.Kind,
        e.MinValue, e.MaxValue, e.TargetValue, e.UnitMeasureCode,
        e.ExpectedValue, e.IsCritical, e.ModifiedDate);

    public static InspectionPlanCharacteristic ToEntity(this CreateInspectionPlanCharacteristicRequest r) => new()
    {
        InspectionPlanId = r.InspectionPlanId,
        SequenceNumber = r.SequenceNumber,
        Name = (r.Name ?? string.Empty).Trim(),
        Kind = r.Kind,
        MinValue = r.MinValue,
        MaxValue = r.MaxValue,
        TargetValue = r.TargetValue,
        UnitMeasureCode = r.UnitMeasureCode?.Trim().ToUpperInvariant(),
        ExpectedValue = r.ExpectedValue?.Trim(),
        IsCritical = r.IsCritical,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateInspectionPlanCharacteristicRequest r, InspectionPlanCharacteristic e)
    {
        if (r.SequenceNumber is not null) e.SequenceNumber = r.SequenceNumber.Value;
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.MinValue is not null) e.MinValue = r.MinValue;
        if (r.MaxValue is not null) e.MaxValue = r.MaxValue;
        if (r.TargetValue is not null) e.TargetValue = r.TargetValue;
        if (r.UnitMeasureCode is not null) e.UnitMeasureCode = r.UnitMeasureCode.Trim().ToUpperInvariant();
        if (r.ExpectedValue is not null) e.ExpectedValue = r.ExpectedValue.Trim();
        if (r.IsCritical is not null) e.IsCritical = r.IsCritical.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static InspectionPlanCharacteristicAuditLogDto ToDto(this InspectionPlanCharacteristicAuditLog a) => new(
        a.Id, a.InspectionPlanCharacteristicId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.InspectionPlanId, a.SequenceNumber, a.Name, a.Kind,
        a.MinValue, a.MaxValue, a.TargetValue, a.UnitMeasureCode,
        a.ExpectedValue, a.IsCritical, a.SourceModifiedDate);
}
