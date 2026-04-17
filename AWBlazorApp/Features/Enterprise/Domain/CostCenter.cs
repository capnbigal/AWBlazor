using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Enterprise.Domain;

/// <summary>
/// Finance-side label that <see cref="OrgUnit"/> rows roll up to. Kept thin on purpose; a real
/// GL integration can layer on later without reshaping this table.
/// </summary>
[Table("CostCenter", Schema = "org")]
public class CostCenter
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>FK into <c>HumanResources.Employee.BusinessEntityID</c>; nullable.</summary>
    public int? OwnerBusinessEntityId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ModifiedDate { get; set; }
}
