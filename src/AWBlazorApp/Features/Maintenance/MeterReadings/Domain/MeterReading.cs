using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.MeterReadings.Domain;

/// <summary>
/// A periodic reading of a meter attached to an asset — runtime hours, cycles, kWh, etc.
/// PmScheduleService reads the latest per-asset value to decide whether a meter-based PM is
/// due. Append-only: new readings are added, old ones not modified.
/// </summary>
[Table("MeterReading", Schema = "maint")]
public class MeterReading
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int AssetId { get; set; }

    public MeterKind Kind { get; set; } = MeterKind.RuntimeHours;

    [Column(TypeName = "decimal(18,2)")] public decimal Value { get; set; }

    public DateTime RecordedAt { get; set; }

    [MaxLength(450)] public string? RecordedByUserId { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum MeterKind : byte
{
    RuntimeHours = 1,
    Cycles = 2,
    KilowattHours = 3,
    MilesOrKm = 4,
}
