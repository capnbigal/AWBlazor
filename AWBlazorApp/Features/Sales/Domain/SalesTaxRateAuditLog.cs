using AWBlazorApp.Features.AdventureWorks.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Audit log for <see cref="SalesTaxRate"/>. EF-managed table <c>dbo.SalesTaxRateAuditLogs</c>.</summary>
public class SalesTaxRateAuditLog : AdventureWorksAuditLogBase
{
    public int SalesTaxRateId { get; set; }

    public int StateProvinceId { get; set; }
    public byte TaxType { get; set; }
    public decimal TaxRate { get; set; }
    [MaxLength(50)] public string? Name { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
