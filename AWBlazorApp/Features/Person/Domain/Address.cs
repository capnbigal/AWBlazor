using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Person.Domain;

/// <summary>
/// Street address. Maps onto the pre-existing <c>Person.Address</c> table. The real table also
/// has a <c>SpatialLocation</c> column of SQL <c>geography</c>; we deliberately do NOT map it
/// here because the CRUD UI has no use for it and the geography type would pull in a
/// NetTopologySuite dependency. SQL Server allows that column to be NULL on insert, so EF will
/// pass NULL when creating new rows from this app.
/// </summary>
[Table("Address", Schema = "Person")]
public class Address
{
    [Key]
    [Column("AddressID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("AddressLine1")]
    [MaxLength(60)]
    public string AddressLine1 { get; set; } = string.Empty;

    [Column("AddressLine2")]
    [MaxLength(60)]
    public string? AddressLine2 { get; set; }

    [Column("City")]
    [MaxLength(30)]
    public string City { get; set; } = string.Empty;

    /// <summary>FK to <c>Person.StateProvince.StateProvinceID</c>.</summary>
    [Column("StateProvinceID")]
    public int StateProvinceId { get; set; }

    [Column("PostalCode")]
    [MaxLength(15)]
    public string PostalCode { get; set; } = string.Empty;

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
