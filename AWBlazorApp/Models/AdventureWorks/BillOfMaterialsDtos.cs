using AWBlazorApp.Data.Entities.AdventureWorks;

namespace AWBlazorApp.Models.AdventureWorks;

public sealed record BillOfMaterialsDto(
    int Id, int? ProductAssemblyId, int ComponentId,
    DateTime StartDate, DateTime? EndDate,
    string UnitMeasureCode, short BomLevel, decimal PerAssemblyQty, DateTime ModifiedDate);

public sealed record CreateBillOfMaterialsRequest
{
    public int? ProductAssemblyId { get; set; }
    public int ComponentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UnitMeasureCode { get; set; }
    public short BomLevel { get; set; }
    public decimal PerAssemblyQty { get; set; }
}

public sealed record UpdateBillOfMaterialsRequest
{
    public int? ProductAssemblyId { get; set; }
    public int? ComponentId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UnitMeasureCode { get; set; }
    public short? BomLevel { get; set; }
    public decimal? PerAssemblyQty { get; set; }
}

public sealed record BillOfMaterialsAuditLogDto(
    int Id, int BillOfMaterialsId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int? ProductAssemblyId, int ComponentId,
    DateTime StartDate, DateTime? EndDate, string? UnitMeasureCode,
    short BomLevel, decimal PerAssemblyQty, DateTime SourceModifiedDate);

public static class BillOfMaterialsMappings
{
    public static BillOfMaterialsDto ToDto(this BillOfMaterials e) => new(
        e.Id, e.ProductAssemblyId, e.ComponentId, e.StartDate, e.EndDate,
        e.UnitMeasureCode, e.BomLevel, e.PerAssemblyQty, e.ModifiedDate);

    public static BillOfMaterials ToEntity(this CreateBillOfMaterialsRequest r) => new()
    {
        ProductAssemblyId = r.ProductAssemblyId,
        ComponentId = r.ComponentId,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        UnitMeasureCode = (r.UnitMeasureCode ?? string.Empty).Trim(),
        BomLevel = r.BomLevel,
        PerAssemblyQty = r.PerAssemblyQty,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateBillOfMaterialsRequest r, BillOfMaterials e)
    {
        if (r.ProductAssemblyId.HasValue) e.ProductAssemblyId = r.ProductAssemblyId.Value;
        if (r.ComponentId.HasValue) e.ComponentId = r.ComponentId.Value;
        if (r.StartDate.HasValue) e.StartDate = r.StartDate.Value;
        if (r.EndDate.HasValue) e.EndDate = r.EndDate.Value;
        if (r.UnitMeasureCode is not null) e.UnitMeasureCode = r.UnitMeasureCode.Trim();
        if (r.BomLevel.HasValue) e.BomLevel = r.BomLevel.Value;
        if (r.PerAssemblyQty.HasValue) e.PerAssemblyQty = r.PerAssemblyQty.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static BillOfMaterialsAuditLogDto ToDto(this BillOfMaterialsAuditLog a) => new(
        a.Id, a.BillOfMaterialsId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ProductAssemblyId, a.ComponentId, a.StartDate, a.EndDate,
        a.UnitMeasureCode, a.BomLevel, a.PerAssemblyQty, a.SourceModifiedDate);
}
