using AWBlazorApp.Features.Engineering.Domain;

namespace AWBlazorApp.Features.Engineering.Models;

public sealed record ManufacturingRoutingDto(
    int Id, string Code, string Name, string? Description,
    int ProductId, int RevisionNumber, bool IsActive, DateTime ModifiedDate);

public sealed record CreateManufacturingRoutingRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int ProductId { get; set; }
    public int RevisionNumber { get; set; } = 1;
    public bool IsActive { get; set; } = false;
}

public sealed record UpdateManufacturingRoutingRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? RevisionNumber { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record ManufacturingRoutingAuditLogDto(
    int Id, int ManufacturingRoutingId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? Code, string? Name, string? Description,
    int ProductId, int RevisionNumber, bool IsActive, DateTime SourceModifiedDate);

public sealed record RoutingStepDto(
    int Id, int ManufacturingRoutingId, int SequenceNumber, string OperationName,
    int? StationId, decimal StandardMinutes, string? Instructions, DateTime ModifiedDate);

public sealed record CreateRoutingStepRequest
{
    public int ManufacturingRoutingId { get; set; }
    public int SequenceNumber { get; set; }
    public string? OperationName { get; set; }
    public int? StationId { get; set; }
    public decimal StandardMinutes { get; set; }
    public string? Instructions { get; set; }
}

public sealed record UpdateRoutingStepRequest
{
    public int? SequenceNumber { get; set; }
    public string? OperationName { get; set; }
    public int? StationId { get; set; }
    public decimal? StandardMinutes { get; set; }
    public string? Instructions { get; set; }
}

public static class RoutingMappings
{
    public static ManufacturingRoutingDto ToDto(this ManufacturingRouting e) => new(
        e.Id, e.Code, e.Name, e.Description,
        e.ProductId, e.RevisionNumber, e.IsActive, e.ModifiedDate);

    public static ManufacturingRouting ToEntity(this CreateManufacturingRoutingRequest r) => new()
    {
        Code = (r.Code ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        ProductId = r.ProductId,
        RevisionNumber = r.RevisionNumber,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateManufacturingRoutingRequest r, ManufacturingRouting e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.RevisionNumber is not null) e.RevisionNumber = r.RevisionNumber.Value;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static ManufacturingRoutingAuditLogDto ToDto(this ManufacturingRoutingAuditLog a) => new(
        a.Id, a.ManufacturingRoutingId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Code, a.Name, a.Description, a.ProductId, a.RevisionNumber, a.IsActive, a.SourceModifiedDate);

    public static RoutingStepDto ToDto(this RoutingStep e) => new(
        e.Id, e.ManufacturingRoutingId, e.SequenceNumber, e.OperationName,
        e.StationId, e.StandardMinutes, e.Instructions, e.ModifiedDate);

    public static RoutingStep ToEntity(this CreateRoutingStepRequest r) => new()
    {
        ManufacturingRoutingId = r.ManufacturingRoutingId,
        SequenceNumber = r.SequenceNumber,
        OperationName = (r.OperationName ?? string.Empty).Trim(),
        StationId = r.StationId,
        StandardMinutes = r.StandardMinutes,
        Instructions = r.Instructions?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateRoutingStepRequest r, RoutingStep e)
    {
        if (r.SequenceNumber is not null) e.SequenceNumber = r.SequenceNumber.Value;
        if (r.OperationName is not null) e.OperationName = r.OperationName.Trim();
        if (r.StationId is not null) e.StationId = r.StationId;
        if (r.StandardMinutes is not null) e.StandardMinutes = r.StandardMinutes.Value;
        if (r.Instructions is not null) e.Instructions = r.Instructions.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }
}
