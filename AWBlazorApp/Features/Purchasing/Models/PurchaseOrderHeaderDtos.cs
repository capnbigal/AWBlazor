using AWBlazorApp.Features.Purchasing.Domain;

namespace AWBlazorApp.Features.Purchasing.Models;

public sealed record PurchaseOrderHeaderDto(
    int Id, byte RevisionNumber, byte Status, int EmployeeId, int VendorId, int ShipMethodId,
    DateTime OrderDate, DateTime? ShipDate, decimal SubTotal, decimal TaxAmt, decimal Freight,
    decimal TotalDue, DateTime ModifiedDate);

public sealed record CreatePurchaseOrderHeaderRequest
{
    public byte RevisionNumber { get; set; }
    public byte Status { get; set; } = 1;
    public int EmployeeId { get; set; }
    public int VendorId { get; set; }
    public int ShipMethodId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal Freight { get; set; }
}

public sealed record UpdatePurchaseOrderHeaderRequest
{
    public byte? RevisionNumber { get; set; }
    public byte? Status { get; set; }
    public int? EmployeeId { get; set; }
    public int? VendorId { get; set; }
    public int? ShipMethodId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmt { get; set; }
    public decimal? Freight { get; set; }
}

public sealed record PurchaseOrderHeaderAuditLogDto(
    int Id, int PurchaseOrderId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, byte RevisionNumber, byte Status, int EmployeeId, int VendorId,
    int ShipMethodId, DateTime OrderDate, DateTime? ShipDate, decimal SubTotal, decimal TaxAmt,
    decimal Freight, decimal TotalDue, DateTime SourceModifiedDate);

public static class PurchaseOrderHeaderMappings
{
    public static PurchaseOrderHeaderDto ToDto(this PurchaseOrderHeader e) => new(
        e.Id, e.RevisionNumber, e.Status, e.EmployeeId, e.VendorId, e.ShipMethodId,
        e.OrderDate, e.ShipDate, e.SubTotal, e.TaxAmt, e.Freight, e.TotalDue, e.ModifiedDate);

    public static PurchaseOrderHeader ToEntity(this CreatePurchaseOrderHeaderRequest r) => new()
    {
        RevisionNumber = r.RevisionNumber,
        Status = r.Status,
        EmployeeId = r.EmployeeId,
        VendorId = r.VendorId,
        ShipMethodId = r.ShipMethodId,
        OrderDate = r.OrderDate,
        ShipDate = r.ShipDate,
        SubTotal = r.SubTotal,
        TaxAmt = r.TaxAmt,
        Freight = r.Freight,
        // TotalDue is computed by SQL Server (SubTotal + TaxAmt + Freight) — leave default.
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdatePurchaseOrderHeaderRequest r, PurchaseOrderHeader e)
    {
        if (r.RevisionNumber.HasValue) e.RevisionNumber = r.RevisionNumber.Value;
        if (r.Status.HasValue) e.Status = r.Status.Value;
        if (r.EmployeeId.HasValue) e.EmployeeId = r.EmployeeId.Value;
        if (r.VendorId.HasValue) e.VendorId = r.VendorId.Value;
        if (r.ShipMethodId.HasValue) e.ShipMethodId = r.ShipMethodId.Value;
        if (r.OrderDate.HasValue) e.OrderDate = r.OrderDate.Value;
        if (r.ShipDate.HasValue) e.ShipDate = r.ShipDate.Value;
        if (r.SubTotal.HasValue) e.SubTotal = r.SubTotal.Value;
        if (r.TaxAmt.HasValue) e.TaxAmt = r.TaxAmt.Value;
        if (r.Freight.HasValue) e.Freight = r.Freight.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static PurchaseOrderHeaderAuditLogDto ToDto(this PurchaseOrderHeaderAuditLog a) => new(
        a.Id, a.PurchaseOrderId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.RevisionNumber, a.Status, a.EmployeeId, a.VendorId, a.ShipMethodId,
        a.OrderDate, a.ShipDate, a.SubTotal, a.TaxAmt, a.Freight, a.TotalDue, a.SourceModifiedDate);
}
