using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Maintenance.ToolSlots.Domain;

/// <summary>
/// Represents the configuration of tool slots for different machine types and families.
/// </summary>
/// <remarks>
/// This entity is mapped onto the pre-existing <c>dbo.ToolSlotConfigurations</c> table in
/// AdventureWorks2022 and is excluded from EF migrations (see
/// <see cref="ApplicationDbContext.OnModelCreating"/>). Column names are explicit because the
/// real table uses a mix of UPPERCASE, snake_case, and PascalCase that doesn't match the C#
/// property casing — even though SQL Server's case-insensitive collation tolerates the
/// differences for the simple ones, the <c>CID</c> primary key and <c>MT_CODE</c> column
/// require explicit mapping.
/// </remarks>
[Table("ToolSlotConfigurations")]
public class ToolSlotConfiguration
{
    [Key]
    [Column("CID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Machine family identifier.</summary>
    [Column("FAMILY")]
    [MaxLength(255)]
    public string? Family { get; set; }

    /// <summary>Machine type code.</summary>
    [Column("MT_CODE")]
    [MaxLength(255)]
    public string? MtCode { get; set; }

    /// <summary>Destination identifier for the configuration.</summary>
    [Column("DESTINATION")]
    [MaxLength(255)]
    public string? Destination { get; set; }

    /// <summary>Front Center Left slot 1.</summary>
    [Column("FCL1")]
    [MaxLength(255)]
    public string? Fcl1 { get; set; }

    /// <summary>Front Center Left slot 2.</summary>
    [Column("FCL2")]
    [MaxLength(255)]
    public string? Fcl2 { get; set; }

    /// <summary>Front Center Right slot 1.</summary>
    [Column("FCR1")]
    [MaxLength(255)]
    public string? Fcr1 { get; set; }

    /// <summary>Front Far Left slot 1.</summary>
    [Column("FFL1")]
    [MaxLength(255)]
    public string? Ffl1 { get; set; }

    /// <summary>Front Far Left slot 2.</summary>
    [Column("FFL2")]
    [MaxLength(255)]
    public string? Ffl2 { get; set; }

    /// <summary>Front Far Right slot 1.</summary>
    [Column("FFR1")]
    [MaxLength(255)]
    public string? Ffr1 { get; set; }

    /// <summary>Front Far Right slot 2.</summary>
    [Column("FFR2")]
    [MaxLength(255)]
    public string? Ffr2 { get; set; }

    /// <summary>Front Far Right slot 3.</summary>
    [Column("FFR3")]
    [MaxLength(255)]
    public string? Ffr3 { get; set; }

    /// <summary>Front Far Right slot 4.</summary>
    [Column("FFR4")]
    [MaxLength(255)]
    public string? Ffr4 { get; set; }

    /// <summary>Rear Center Left slot 1.</summary>
    [Column("RCL1")]
    [MaxLength(255)]
    public string? Rcl1 { get; set; }

    /// <summary>Rear Center Right slot 1.</summary>
    [Column("RCR1")]
    [MaxLength(255)]
    public string? Rcr1 { get; set; }

    /// <summary>Rear Center Right slot 2.</summary>
    [Column("RCR2")]
    [MaxLength(255)]
    public string? Rcr2 { get; set; }

    /// <summary>Rear Far Left slot 1.</summary>
    [Column("RFL1")]
    [MaxLength(255)]
    public string? Rfl1 { get; set; }

    /// <summary>Rear Far Right slot 1.</summary>
    [Column("RFR1")]
    [MaxLength(255)]
    public string? Rfr1 { get; set; }

    /// <summary>Rear Far Right slot 2.</summary>
    [Column("RFR2")]
    [MaxLength(255)]
    public string? Rfr2 { get; set; }

    /// <summary>Indicates whether this configuration is currently active.</summary>
    [Column("IsActive")]
    public bool IsActive { get; set; }
}
