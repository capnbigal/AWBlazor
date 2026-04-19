using AWBlazorApp.Features.Mes.Domain;

namespace AWBlazorApp.Features.Mes.Dtos;

// Operator clock + downtime + downtime reason DTOs.

public sealed record OperatorClockEventDto(
    long Id, int? ProductionRunId, int StationId, int BusinessEntityId,
    DateTime ClockInAt, DateTime? ClockOutAt, string? Notes, DateTime ModifiedDate);

public sealed record CreateOperatorClockEventRequest
{
    public int? ProductionRunId { get; set; }
    public int StationId { get; set; }
    public int BusinessEntityId { get; set; }
    public DateTime? ClockInAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record CloseOperatorClockEventRequest
{
    public DateTime? ClockOutAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record DowntimeEventDto(
    long Id, int? ProductionRunId, int StationId, int DowntimeReasonId,
    DateTime StartAt, DateTime? EndAt, string? Notes, DateTime ModifiedDate);

public sealed record CreateDowntimeEventRequest
{
    public int? ProductionRunId { get; set; }
    public int StationId { get; set; }
    public int DowntimeReasonId { get; set; }
    public DateTime? StartAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record CloseDowntimeEventRequest
{
    public DateTime? EndAt { get; set; }
    public string? Notes { get; set; }
}

public sealed record DowntimeReasonDto(
    int Id, string Code, string Name, string? Description, bool IsActive, DateTime ModifiedDate);

public sealed record CreateDowntimeReasonRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateDowntimeReasonRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record DowntimeReasonAuditLogDto(
    int Id, int DowntimeReasonId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description, bool IsActive, DateTime SourceModifiedDate);

public static class ShopFloorMappings
{
    public static OperatorClockEventDto ToDto(this OperatorClockEvent e) => new(
        e.Id, e.ProductionRunId, e.StationId, e.BusinessEntityId,
        e.ClockInAt, e.ClockOutAt, e.Notes, e.ModifiedDate);

    public static OperatorClockEvent ToEntity(this CreateOperatorClockEventRequest r) => new()
    {
        ProductionRunId = r.ProductionRunId,
        StationId = r.StationId,
        BusinessEntityId = r.BusinessEntityId,
        ClockInAt = r.ClockInAt ?? DateTime.UtcNow,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static DowntimeEventDto ToDto(this DowntimeEvent e) => new(
        e.Id, e.ProductionRunId, e.StationId, e.DowntimeReasonId,
        e.StartAt, e.EndAt, e.Notes, e.ModifiedDate);

    public static DowntimeEvent ToEntity(this CreateDowntimeEventRequest r) => new()
    {
        ProductionRunId = r.ProductionRunId,
        StationId = r.StationId,
        DowntimeReasonId = r.DowntimeReasonId,
        StartAt = r.StartAt ?? DateTime.UtcNow,
        Notes = r.Notes?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static DowntimeReasonDto ToDto(this DowntimeReason e) => new(
        e.Id, e.Code, e.Name, e.Description, e.IsActive, e.ModifiedDate);

    public static DowntimeReason ToEntity(this CreateDowntimeReasonRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateDowntimeReasonRequest r, DowntimeReason e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static DowntimeReasonAuditLogDto ToDto(this DowntimeReasonAuditLog a) => new(
        a.Id, a.DowntimeReasonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.IsActive, a.SourceModifiedDate);
}
