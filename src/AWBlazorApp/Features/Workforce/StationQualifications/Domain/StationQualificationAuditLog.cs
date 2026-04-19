using AWBlazorApp.Shared.Domain;

namespace AWBlazorApp.Features.Workforce.StationQualifications.Domain;

public class StationQualificationAuditLog : AdventureWorksAuditLogBase
{
    public int StationQualificationId { get; set; }

    public int StationId { get; set; }
    public int QualificationId { get; set; }
    public bool IsRequired { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
