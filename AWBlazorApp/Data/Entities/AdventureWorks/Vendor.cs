using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Vendor / supplier. Maps onto the pre-existing <c>Purchasing.Vendor</c> table. The PK is <c>BusinessEntityID</c> and is <b>not</b> an identity column — it's shared with <c>Person.BusinessEntity</c>, so callers must supply the id explicitly on create.</summary>
[Table("Vendor", Schema = "Purchasing")]
public class Vendor
{
    /// <summary>Shared PK / FK to <c>Person.BusinessEntity.BusinessEntityID</c>. NOT an identity column.</summary>
    [Key]
    [Column("BusinessEntityID")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column("AccountNumber")]
    [MaxLength(15)]
    public string AccountNumber { get; set; } = string.Empty;

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>1–5 credit rating scale.</summary>
    [Column("CreditRating")]
    public byte CreditRating { get; set; }

    [Column("PreferredVendorStatus")]
    public bool PreferredVendorStatus { get; set; }

    [Column("ActiveFlag")]
    public bool ActiveFlag { get; set; }

    [Column("PurchasingWebServiceURL")]
    [MaxLength(1024)]
    public string? PurchasingWebServiceUrl { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
