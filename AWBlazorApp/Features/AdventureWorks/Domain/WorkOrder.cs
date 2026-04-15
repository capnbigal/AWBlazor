using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Manufacturing work order. Maps onto the pre-existing <c>Production.WorkOrder</c> table. <c>StockedQty</c> is a computed column (<c>ISNULL(OrderQty - ScrappedQty, 0)</c>) — EF never writes it.</summary>
[Table("WorkOrder", Schema = "Production")]
public class WorkOrder
{
    [Key]
    [Column("WorkOrderID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("OrderQty")]
    public int OrderQty { get; set; }

    /// <summary>Computed in SQL as <c>ISNULL(OrderQty - ScrappedQty, 0)</c>. Read-only.</summary>
    [Column("StockedQty")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int StockedQty { get; set; }

    [Column("ScrappedQty")]
    public short ScrappedQty { get; set; }

    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("DueDate")]
    public DateTime DueDate { get; set; }

    /// <summary>FK to <c>Production.ScrapReason.ScrapReasonID</c>. Null when nothing was scrapped.</summary>
    [Column("ScrapReasonID")]
    public short? ScrapReasonId { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
