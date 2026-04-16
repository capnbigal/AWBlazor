using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.HumanResources.Domain;

/// <summary>Manufacturing shifts (Day, Evening, Night). Maps onto the pre-existing <c>HumanResources.Shift</c> table in AdventureWorks2022. PK is a <c>tinyint</c>, Start/End are <c>time</c> columns.</summary>
[Table("Shift", Schema = "HumanResources")]
public class Shift
{
    [Key]
    [Column("ShiftID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public byte Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Shift start time-of-day. SQL Server <c>time(7)</c> maps to <see cref="TimeSpan"/>.</summary>
    [Column("StartTime")]
    public TimeSpan StartTime { get; set; }

    /// <summary>Shift end time-of-day.</summary>
    [Column("EndTime")]
    public TimeSpan EndTime { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
