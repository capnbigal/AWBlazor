using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Purchasing.Vendors.Domain;

/// <summary>Audit log for <see cref="Vendor"/>. EF-managed table <c>dbo.VendorAuditLogs</c>.</summary>
public class VendorAuditLog : AdventureWorksAuditLogBase
{
    public int VendorId { get; set; }

    [MaxLength(15)] public string? AccountNumber { get; set; }
    [MaxLength(50)] public string? Name { get; set; }
    public byte CreditRating { get; set; }
    public bool PreferredVendorStatus { get; set; }
    public bool ActiveFlag { get; set; }
    [MaxLength(1024)] public string? PurchasingWebServiceUrl { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
