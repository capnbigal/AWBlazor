using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Enterprise.OrgUnits.Domain;

/// <summary>
/// Any non-leaf node in the organization hierarchy: Plant → Division → Department → Subdepartment
/// → Team → Area. Stored as one table keyed by <see cref="Kind"/> so traversal is a single query.
/// Stations (the leaves that actually do work) live in their own table because they carry
/// machine-specific fields the other kinds don't need.
/// </summary>
[Table("OrgUnit", Schema = "org")]
public class OrgUnit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? ParentOrgUnitId { get; set; }

    public OrgUnitKind Kind { get; set; }

    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Materialized breadcrumb, slash-separated codes from root → this node. Rebuilt on save.</summary>
    [MaxLength(1024)]
    public string Path { get; set; } = string.Empty;

    public byte Depth { get; set; }

    public int? CostCenterId { get; set; }

    /// <summary>FK into <c>HumanResources.Employee.BusinessEntityID</c>. Nullable for vacant posts.</summary>
    public int? ManagerBusinessEntityId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}

public enum OrgUnitKind : byte
{
    Plant = 1,
    Division = 2,
    Department = 3,
    Subdepartment = 4,
    Team = 5,
    Area = 6,
}
