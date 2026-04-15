using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="SpecialOffer"/>. EF-managed table <c>dbo.SpecialOfferAuditLogs</c>.</summary>
public class SpecialOfferAuditLog : AdventureWorksAuditLogBase
{
    public int SpecialOfferId { get; set; }

    [MaxLength(255)] public string? Description { get; set; }
    public decimal DiscountPct { get; set; }
    [MaxLength(50)] public string? OfferType { get; set; }
    [MaxLength(50)] public string? Category { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MinQty { get; set; }
    public int? MaxQty { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
