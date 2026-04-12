using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.AdventureWorks;

/// <summary>Audit log for <see cref="BillOfMaterials"/>. EF-managed table <c>dbo.BillOfMaterialsAuditLogs</c>.</summary>
public class BillOfMaterialsAuditLog : AdventureWorksAuditLogBase
{
    public int BillOfMaterialsId { get; set; }

    public int? ProductAssemblyId { get; set; }
    public int ComponentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [MaxLength(3)] public string? UnitMeasureCode { get; set; }
    public short BomLevel { get; set; }
    public decimal PerAssemblyQty { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
