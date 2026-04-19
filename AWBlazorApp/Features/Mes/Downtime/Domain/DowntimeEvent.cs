using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Downtime.Domain;

/// <summary>
/// Append-only downtime event. Captures when a station (optionally a specific run) was down
/// and why. Drives the Availability denominator in OEE rollups.
/// </summary>
[Table("DowntimeEvent", Schema = "mes")]
public class DowntimeEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int? ProductionRunId { get; set; }

    public int StationId { get; set; }

    public int DowntimeReasonId { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
