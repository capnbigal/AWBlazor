using System.ComponentModel.DataAnnotations.Schema;
namespace AWBlazorApp.Features.Scheduling.DeliverySchedules.Domain;

// Maps to Scheduling.vw_CurrentDeliverySchedule. Configured .HasNoKey().ToView(...) in DbContext (Task 3).
public class CurrentDeliveryScheduleRow
{
    [Column("WeekId")] public int WeekId { get; set; }
    [Column("LocationID")] public short LocationId { get; set; }
    [Column("SalesOrderID")] public int SalesOrderId { get; set; }
    [Column("SalesOrderDetailID")] public int SalesOrderDetailId { get; set; }
    [Column("ProductID")] public int ProductId { get; set; }
    [Column("PlannedSequence")] public int? PlannedSequence { get; set; }
    [Column("PlannedStart")] public DateTime? PlannedStart { get; set; }
    [Column("PlannedEnd")] public DateTime? PlannedEnd { get; set; }
    [Column("PlannedQty")] public short? PlannedQty { get; set; }
    [Column("CurrentSequence")] public int? CurrentSequence { get; set; }
    [Column("CurrentStart")] public DateTime? CurrentStart { get; set; }
    [Column("CurrentEnd")] public DateTime? CurrentEnd { get; set; }
    [Column("CurrentQty")] public short? CurrentQty { get; set; }
    [Column("SequenceDrift")] public int? SequenceDrift { get; set; }
    [Column("StartDriftMinutes")] public int? StartDriftMinutes { get; set; }
    [Column("PromiseDate")] public DateTime? PromiseDate { get; set; }
    [Column("PromiseDriftMinutes")] public int? PromiseDriftMinutes { get; set; }
    [Column("ExceptionType")] public byte? ExceptionType { get; set; }
    [Column("ExceptionReason")] public string? ExceptionReason { get; set; }
    [Column("SoStatus")] public byte? SoStatus { get; set; }
    [Column("IsCancelled")] public bool IsCancelled { get; set; }
    [Column("IsHotOrder")] public bool IsHotOrder { get; set; }
}
