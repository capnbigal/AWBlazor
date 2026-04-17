using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Mes.Domain;

/// <summary>
/// Append-only clock-in/clock-out event. Ties an operator to a station, optionally bound to
/// a specific run. Drives the Performance numerator in OEE rollups: run-time vs. operator-
/// hours-on-station.
/// </summary>
[Table("OperatorClockEvent", Schema = "mes")]
public class OperatorClockEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int? ProductionRunId { get; set; }

    public int StationId { get; set; }

    /// <summary>FK → <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    public int BusinessEntityId { get; set; }

    public DateTime ClockInAt { get; set; }
    public DateTime? ClockOutAt { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}
