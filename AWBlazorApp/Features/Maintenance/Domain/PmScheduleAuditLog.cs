using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Maintenance.Domain;

public class PmScheduleAuditLog : AdventureWorksAuditLogBase
{
    public int PmScheduleId { get; set; }

    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(200)] public string? Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public int AssetId { get; set; }
    public PmIntervalKind IntervalKind { get; set; }
    public int IntervalValue { get; set; }
    public WorkOrderPriority DefaultPriority { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public decimal? LastCompletedMeterValue { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
