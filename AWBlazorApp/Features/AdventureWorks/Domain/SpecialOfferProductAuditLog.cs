namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Audit log for <see cref="SpecialOfferProduct"/>. EF-managed table <c>dbo.SpecialOfferProductAuditLogs</c>. Pure junction — audit rows only carry the composite key plus timestamp.</summary>
public class SpecialOfferProductAuditLog : AdventureWorksAuditLogBase
{
    public int SpecialOfferId { get; set; }
    public int ProductId { get; set; }

    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
