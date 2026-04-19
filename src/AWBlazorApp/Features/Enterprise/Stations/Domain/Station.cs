using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Enterprise.Stations.Domain;

/// <summary>
/// A work location where a person does something — the leaf of the hierarchy. Belongs to an
/// <see cref="OrgUnit"/> (typically of kind <c>Area</c>). Every station is manned by a default
/// operator; an installed <see cref="Asset"/> (machine/tool) is optional since not every station
/// needs one (manual assembly, inspection, packaging, etc.).
/// </summary>
[Table("Station", Schema = "org")]
public class Station
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OrgUnitId { get; set; }

    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public StationKind StationKind { get; set; }

    /// <summary>
    /// Default / primary operator assigned to this station. FK into
    /// <c>HumanResources.Employee.BusinessEntityID</c> (which in turn joins <c>Person.BusinessEntity</c>),
    /// so employees, contractors, and non-employee persons are all addressable through one column.
    /// Nullable for temporarily unassigned stations. Real-time per-shift assignment comes later
    /// as a separate <c>StationAssignment</c> table (Phase G: Workforce &amp; Capability).
    /// </summary>
    public int? OperatorBusinessEntityId { get; set; }

    /// <summary>Optional — a station is a location+operator first; machines are not required.</summary>
    public int? AssetId { get; set; }

    /// <summary>
    /// Target seconds-per-unit at full speed. Drives the Performance denominator in OEE
    /// computation: Performance = (units × IdealCycleSeconds) / actualSeconds. Null means
    /// "fall back to the caller-supplied default" (the nightly Hangfire rollup uses 60s).
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? IdealCycleSeconds { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum StationKind : byte
{
    Workstation = 1,
    Assembly = 2,
    Inspection = 3,
    Packaging = 4,
    Shipping = 5,
    Receiving = 6,
    Rework = 7,
    Storage = 8,
}
