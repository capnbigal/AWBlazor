using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.WorkOrderRoutings.Domain;

/// <summary>Work-order routing steps through locations. Maps onto the pre-existing <c>Production.WorkOrderRouting</c> table. 3-column composite PK = (WorkOrderID, ProductID, OperationSequence).</summary>
[PrimaryKey(nameof(WorkOrderId), nameof(ProductId), nameof(OperationSequence))]
[Table("WorkOrderRouting", Schema = "Production")]
public class WorkOrderRouting
{
    /// <summary>FK to <c>Production.WorkOrder.WorkOrderID</c>. Part of the composite PK.</summary>
    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    /// <summary>FK to <c>Production.Product.ProductID</c>. Part of the composite PK.</summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>Sequence of the routing step. Part of the composite PK.</summary>
    [Column("OperationSequence")]
    public short OperationSequence { get; set; }

    /// <summary>FK to <c>Production.Location.LocationID</c>.</summary>
    [Column("LocationID")]
    public short LocationId { get; set; }

    [Column("ScheduledStartDate")]
    public DateTime ScheduledStartDate { get; set; }

    [Column("ScheduledEndDate")]
    public DateTime ScheduledEndDate { get; set; }

    [Column("ActualStartDate")]
    public DateTime? ActualStartDate { get; set; }

    [Column("ActualEndDate")]
    public DateTime? ActualEndDate { get; set; }

    /// <summary>Actual resource hours consumed (decimal(9,4)).</summary>
    [Column("ActualResourceHrs", TypeName = "decimal(9,4)")]
    public decimal? ActualResourceHrs { get; set; }

    /// <summary>Estimated manufacturing cost (money).</summary>
    [Column("PlannedCost", TypeName = "money")]
    public decimal PlannedCost { get; set; }

    /// <summary>Actual manufacturing cost (money, nullable).</summary>
    [Column("ActualCost", TypeName = "money")]
    public decimal? ActualCost { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
