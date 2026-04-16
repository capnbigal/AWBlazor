using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Sales.Domain;

/// <summary>Sales discount / special offer. Maps onto the pre-existing <c>Sales.SpecialOffer</c> table. The SQL column <c>Type</c> is renamed to <see cref="OfferType"/> in C# to avoid shadowing <see cref="System.Type"/>.</summary>
[Table("SpecialOffer", Schema = "Sales")]
public class SpecialOffer
{
    [Key]
    [Column("SpecialOfferID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("Description")]
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Discount percentage (0.0 – 1.0). SQL <c>smallmoney</c>.</summary>
    [Column("DiscountPct", TypeName = "smallmoney")]
    public decimal DiscountPct { get; set; }

    /// <summary>Offer category (No Discount, Reseller, Customer, ...). Mapped from the SQL column <c>Type</c>.</summary>
    [Column("Type")]
    [MaxLength(50)]
    public string OfferType { get; set; } = string.Empty;

    [Column("Category")]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime EndDate { get; set; }

    [Column("MinQty")]
    public int MinQty { get; set; }

    [Column("MaxQty")]
    public int? MaxQty { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
