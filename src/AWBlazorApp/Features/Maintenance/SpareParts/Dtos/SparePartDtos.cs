using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 

namespace AWBlazorApp.Features.Maintenance.SpareParts.Dtos;

public sealed record SparePartDto(
    int Id, string PartNumber, string Name, string? Description,
    int? ProductId, string UnitMeasureCode, decimal? StandardCost,
    int? ReorderPoint, int? ReorderQuantity, bool IsActive, DateTime ModifiedDate);

public sealed record CreateSparePartRequest
{
    public string? PartNumber { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public string UnitMeasureCode { get; set; } = "EA";
    public decimal? StandardCost { get; set; }
    public int? ReorderPoint { get; set; }
    public int? ReorderQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateSparePartRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public string? UnitMeasureCode { get; set; }
    public decimal? StandardCost { get; set; }
    public int? ReorderPoint { get; set; }
    public int? ReorderQuantity { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record SparePartAuditLogDto(
    int Id, int SparePartId, string Action, string? ChangedBy, DateTime ChangedDate, string? ChangeSummary,
    string? PartNumber, string? Name, string? Description,
    int? ProductId, string? UnitMeasureCode, decimal? StandardCost,
    int? ReorderPoint, int? ReorderQuantity, bool IsActive, DateTime SourceModifiedDate);

public sealed record WorkOrderPartUsageDto(
    int Id, int MaintenanceWorkOrderId, int SparePartId, decimal Quantity,
    decimal? UnitCost, DateTime UsedAt, string? UsedByUserId, string? Notes, DateTime ModifiedDate);

public sealed record CreateWorkOrderPartUsageRequest
{
    public int MaintenanceWorkOrderId { get; set; }
    public int SparePartId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }
}

public static class SparePartMappings
{
    public static SparePartDto ToDto(this SparePart e) => new(
        e.Id, e.PartNumber, e.Name, e.Description,
        e.ProductId, e.UnitMeasureCode, e.StandardCost,
        e.ReorderPoint, e.ReorderQuantity, e.IsActive, e.ModifiedDate);

    public static SparePart ToEntity(this CreateSparePartRequest r) => new()
    {
        PartNumber = (r.PartNumber ?? string.Empty).Trim().ToUpperInvariant(),
        Name = (r.Name ?? string.Empty).Trim(),
        Description = r.Description?.Trim(),
        ProductId = r.ProductId,
        UnitMeasureCode = (r.UnitMeasureCode ?? "EA").Trim().ToUpperInvariant(),
        StandardCost = r.StandardCost,
        ReorderPoint = r.ReorderPoint,
        ReorderQuantity = r.ReorderQuantity,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSparePartRequest r, SparePart e)
    {
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.Description is not null) e.Description = r.Description.Trim();
        if (r.ProductId is not null) e.ProductId = r.ProductId;
        if (r.UnitMeasureCode is not null) e.UnitMeasureCode = r.UnitMeasureCode.Trim().ToUpperInvariant();
        if (r.StandardCost is not null) e.StandardCost = r.StandardCost;
        if (r.ReorderPoint is not null) e.ReorderPoint = r.ReorderPoint;
        if (r.ReorderQuantity is not null) e.ReorderQuantity = r.ReorderQuantity;
        if (r.IsActive is not null) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SparePartAuditLogDto ToDto(this SparePartAuditLog a) => new(
        a.Id, a.SparePartId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.PartNumber, a.Name, a.Description,
        a.ProductId, a.UnitMeasureCode, a.StandardCost,
        a.ReorderPoint, a.ReorderQuantity, a.IsActive, a.SourceModifiedDate);

    public static WorkOrderPartUsageDto ToDto(this WorkOrderPartUsage e) => new(
        e.Id, e.MaintenanceWorkOrderId, e.SparePartId, e.Quantity,
        e.UnitCost, e.UsedAt, e.UsedByUserId, e.Notes, e.ModifiedDate);

    public static WorkOrderPartUsage ToEntity(this CreateWorkOrderPartUsageRequest r, string? userId)
    {
        var now = DateTime.UtcNow;
        return new WorkOrderPartUsage
        {
            MaintenanceWorkOrderId = r.MaintenanceWorkOrderId,
            SparePartId = r.SparePartId,
            Quantity = r.Quantity,
            UnitCost = r.UnitCost,
            UsedAt = now,
            UsedByUserId = userId,
            Notes = r.Notes?.Trim(),
            ModifiedDate = now,
        };
    }
}
