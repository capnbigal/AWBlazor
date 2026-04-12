using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record SalesOrderHeaderDto(
    int Id, byte RevisionNumber, DateTime OrderDate, DateTime DueDate, DateTime? ShipDate,
    byte Status, bool OnlineOrderFlag, string SalesOrderNumber, string? PurchaseOrderNumber,
    string? AccountNumber, int CustomerId, int? SalesPersonId, int? TerritoryId,
    int BillToAddressId, int ShipToAddressId, int ShipMethodId,
    int? CreditCardId, string? CreditCardApprovalCode, int? CurrencyRateId,
    decimal SubTotal, decimal TaxAmt, decimal Freight, decimal TotalDue,
    string? Comment, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateSalesOrderHeaderRequest
{
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public byte Status { get; set; } = 1;
    public bool OnlineOrderFlag { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? AccountNumber { get; set; }
    public int CustomerId { get; set; }
    public int? SalesPersonId { get; set; }
    public int? TerritoryId { get; set; }
    public int BillToAddressId { get; set; }
    public int ShipToAddressId { get; set; }
    public int ShipMethodId { get; set; }
    public int? CreditCardId { get; set; }
    public string? CreditCardApprovalCode { get; set; }
    public int? CurrencyRateId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal Freight { get; set; }
    public string? Comment { get; set; }
}

public sealed record UpdateSalesOrderHeaderRequest
{
    public DateTime? DueDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public byte? Status { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? AccountNumber { get; set; }
    public int? SalesPersonId { get; set; }
    public int? TerritoryId { get; set; }
    public int? ShipMethodId { get; set; }
    public int? CreditCardId { get; set; }
    public string? CreditCardApprovalCode { get; set; }
    public int? CurrencyRateId { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmt { get; set; }
    public decimal? Freight { get; set; }
    public string? Comment { get; set; }
}

public sealed record SalesOrderHeaderAuditLogDto(
    int Id, int SalesOrderId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, byte RevisionNumber, DateTime OrderDate, DateTime DueDate, DateTime? ShipDate,
    byte Status, bool OnlineOrderFlag, string? SalesOrderNumber, string? PurchaseOrderNumber,
    string? AccountNumber, int CustomerId, int? SalesPersonId, int? TerritoryId,
    int BillToAddressId, int ShipToAddressId, int ShipMethodId,
    int? CreditCardId, string? CreditCardApprovalCode, int? CurrencyRateId,
    decimal SubTotal, decimal TaxAmt, decimal Freight, decimal TotalDue,
    string? Comment, Guid RowGuid, DateTime SourceModifiedDate);

public static class SalesOrderHeaderMappings
{
    public static SalesOrderHeaderDto ToDto(this SalesOrderHeader e) => new(
        e.Id, e.RevisionNumber, e.OrderDate, e.DueDate, e.ShipDate,
        e.Status, e.OnlineOrderFlag, e.SalesOrderNumber, e.PurchaseOrderNumber,
        e.AccountNumber, e.CustomerId, e.SalesPersonId, e.TerritoryId,
        e.BillToAddressId, e.ShipToAddressId, e.ShipMethodId,
        e.CreditCardId, e.CreditCardApprovalCode, e.CurrencyRateId,
        e.SubTotal, e.TaxAmt, e.Freight, e.TotalDue,
        e.Comment, e.RowGuid, e.ModifiedDate);

    public static SalesOrderHeader ToEntity(this CreateSalesOrderHeaderRequest r) => new()
    {
        RevisionNumber = 0,
        OrderDate = r.OrderDate,
        DueDate = r.DueDate,
        ShipDate = r.ShipDate,
        Status = r.Status,
        OnlineOrderFlag = r.OnlineOrderFlag,
        PurchaseOrderNumber = TrimToNull(r.PurchaseOrderNumber),
        AccountNumber = TrimToNull(r.AccountNumber),
        CustomerId = r.CustomerId,
        SalesPersonId = r.SalesPersonId,
        TerritoryId = r.TerritoryId,
        BillToAddressId = r.BillToAddressId,
        ShipToAddressId = r.ShipToAddressId,
        ShipMethodId = r.ShipMethodId,
        CreditCardId = r.CreditCardId,
        CreditCardApprovalCode = TrimToNull(r.CreditCardApprovalCode),
        CurrencyRateId = r.CurrencyRateId,
        SubTotal = r.SubTotal,
        TaxAmt = r.TaxAmt,
        Freight = r.Freight,
        Comment = TrimToNull(r.Comment),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesOrderHeaderRequest r, SalesOrderHeader e)
    {
        if (r.DueDate.HasValue) e.DueDate = r.DueDate.Value;
        if (r.ShipDate.HasValue) e.ShipDate = r.ShipDate.Value;
        if (r.Status.HasValue) e.Status = r.Status.Value;
        if (r.PurchaseOrderNumber is not null) e.PurchaseOrderNumber = TrimToNull(r.PurchaseOrderNumber);
        if (r.AccountNumber is not null) e.AccountNumber = TrimToNull(r.AccountNumber);
        if (r.SalesPersonId.HasValue) e.SalesPersonId = r.SalesPersonId.Value;
        if (r.TerritoryId.HasValue) e.TerritoryId = r.TerritoryId.Value;
        if (r.ShipMethodId.HasValue) e.ShipMethodId = r.ShipMethodId.Value;
        if (r.CreditCardId.HasValue) e.CreditCardId = r.CreditCardId.Value;
        if (r.CreditCardApprovalCode is not null) e.CreditCardApprovalCode = TrimToNull(r.CreditCardApprovalCode);
        if (r.CurrencyRateId.HasValue) e.CurrencyRateId = r.CurrencyRateId.Value;
        if (r.SubTotal.HasValue) e.SubTotal = r.SubTotal.Value;
        if (r.TaxAmt.HasValue) e.TaxAmt = r.TaxAmt.Value;
        if (r.Freight.HasValue) e.Freight = r.Freight.Value;
        if (r.Comment is not null) e.Comment = TrimToNull(r.Comment);
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesOrderHeaderAuditLogDto ToDto(this SalesOrderHeaderAuditLog a) => new(
        a.Id, a.SalesOrderId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.RevisionNumber, a.OrderDate, a.DueDate, a.ShipDate,
        a.Status, a.OnlineOrderFlag, a.SalesOrderNumber, a.PurchaseOrderNumber,
        a.AccountNumber, a.CustomerId, a.SalesPersonId, a.TerritoryId,
        a.BillToAddressId, a.ShipToAddressId, a.ShipMethodId,
        a.CreditCardId, a.CreditCardApprovalCode, a.CurrencyRateId,
        a.SubTotal, a.TaxAmt, a.Freight, a.TotalDue,
        a.Comment, a.RowGuid, a.SourceModifiedDate);

    private static string? TrimToNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
