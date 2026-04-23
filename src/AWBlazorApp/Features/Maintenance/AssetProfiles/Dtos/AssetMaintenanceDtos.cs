using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 

namespace AWBlazorApp.Features.Maintenance.AssetProfiles.Dtos;

public sealed record AssetMaintenanceProfileDto(
    int Id, int AssetId, AssetCriticality Criticality, int? OwnerBusinessEntityId,
    int? TargetMtbfHours, DateTime? NextPmDueAt, string? Notes, DateTime ModifiedDate);

public sealed record CreateAssetMaintenanceProfileRequest
{
    public int AssetId { get; set; }
    public AssetCriticality Criticality { get; set; } = AssetCriticality.Medium;
    public int? OwnerBusinessEntityId { get; set; }
    public int? TargetMtbfHours { get; set; }
    public string? Notes { get; set; }
}

public sealed record UpdateAssetMaintenanceProfileRequest
{
    public AssetCriticality? Criticality { get; set; }
    public int? OwnerBusinessEntityId { get; set; }
    public int? TargetMtbfHours { get; set; }
    public string? Notes { get; set; }
}

public sealed record MeterReadingDto(
    long Id, int AssetId, MeterKind Kind, decimal Value,
    DateTime RecordedAt, string? RecordedByUserId, string? Notes, DateTime ModifiedDate);

public sealed record CreateMeterReadingRequest
{
    public int AssetId { get; set; }
    public MeterKind Kind { get; set; } = MeterKind.RuntimeHours;
    public decimal Value { get; set; }
    public DateTime? RecordedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record MaintenanceLogDto(
    long Id, int AssetId, MaintenanceLogKind Kind, string Note,
    string? AuthoredByUserId, DateTime AuthoredAt, int? MaintenanceWorkOrderId, DateTime ModifiedDate);

public sealed record CreateMaintenanceLogRequest
{
    public int AssetId { get; set; }
    public MaintenanceLogKind Kind { get; set; } = MaintenanceLogKind.Observation;
    public string? Note { get; set; }
    public int? MaintenanceWorkOrderId { get; set; }
}

public static class AssetMaintenanceMappings
{
    public static AssetMaintenanceProfileDto ToDto(this AssetMaintenanceProfile e) => new(
        e.Id, e.AssetId, e.Criticality, e.OwnerBusinessEntityId,
        e.TargetMtbfHours, e.NextPmDueAt, e.Notes, e.ModifiedDate);

    public static AssetMaintenanceProfile ToEntity(this CreateAssetMaintenanceProfileRequest r) => new()
    {
        AssetId = r.AssetId,
        Criticality = r.Criticality,
        OwnerBusinessEntityId = r.OwnerBusinessEntityId,
        TargetMtbfHours = r.TargetMtbfHours,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateAssetMaintenanceProfileRequest r, AssetMaintenanceProfile e)
    {
        if (r.Criticality is not null) e.Criticality = r.Criticality.Value;
        if (r.OwnerBusinessEntityId is not null) e.OwnerBusinessEntityId = r.OwnerBusinessEntityId;
        if (r.TargetMtbfHours is not null) e.TargetMtbfHours = r.TargetMtbfHours;
        if (r.Notes is not null) e.Notes = r.Notes.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static MeterReadingDto ToDto(this MeterReading e) => new(
        e.Id, e.AssetId, e.Kind, e.Value,
        e.RecordedAt, e.RecordedByUserId, e.Notes, e.ModifiedDate);

    public static MeterReading ToEntity(this CreateMeterReadingRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new MeterReading
        {
            AssetId = r.AssetId,
            Kind = r.Kind,
            Value = r.Value,
            RecordedAt = r.RecordedAt ?? now,
            RecordedByUserId = userId,
            Notes = r.Notes?.Trim(),
            ModifiedDate = now,
        };
    }

    public static MaintenanceLogDto ToDto(this MaintenanceLog e) => new(
        e.Id, e.AssetId, e.Kind, e.Note,
        e.AuthoredByUserId, e.AuthoredAt, e.MaintenanceWorkOrderId, e.ModifiedDate);

    public static MaintenanceLog ToEntity(this CreateMaintenanceLogRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new MaintenanceLog
        {
            AssetId = r.AssetId,
            Kind = r.Kind,
            Note = (r.Note ?? string.Empty).Trim(),
            AuthoredByUserId = userId,
            AuthoredAt = now,
            MaintenanceWorkOrderId = r.MaintenanceWorkOrderId,
            ModifiedDate = now,
        };
    }
}
