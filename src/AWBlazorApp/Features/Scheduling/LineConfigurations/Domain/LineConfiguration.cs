using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;

[Table("LineConfiguration", Schema = "Scheduling")]
public class LineConfiguration
{
    [Key, Column("LineConfigurationID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("TaktSeconds")] public int TaktSeconds { get; set; }
    [Column("ShiftsPerDay")] public byte ShiftsPerDay { get; set; }
    [Column("MinutesPerShift")] public short MinutesPerShift { get; set; }
    [Column("FrozenLookaheadHours")] public int FrozenLookaheadHours { get; set; } = 72;
    [Column("IsActive")] public bool IsActive { get; set; } = true;
    [Column("ModifiedDate")] public DateTime ModifiedDate { get; set; }
}
