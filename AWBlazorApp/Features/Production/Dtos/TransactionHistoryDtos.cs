using AWBlazorApp.Features.Production.Domain;

namespace AWBlazorApp.Features.Production.Dtos;

public sealed record TransactionHistoryDto(
    int Id, int ProductId, int ReferenceOrderId, int ReferenceOrderLineId,
    DateTime TransactionDate, string TransactionType, int Quantity,
    decimal ActualCost, DateTime ModifiedDate);

public sealed record CreateTransactionHistoryRequest
{
    public int ProductId { get; set; }
    public int ReferenceOrderId { get; set; }
    public int ReferenceOrderLineId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ActualCost { get; set; }
}

public sealed record UpdateTransactionHistoryRequest
{
    public int? ProductId { get; set; }
    public int? ReferenceOrderId { get; set; }
    public int? ReferenceOrderLineId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? TransactionType { get; set; }
    public int? Quantity { get; set; }
    public decimal? ActualCost { get; set; }
}

public sealed record TransactionHistoryAuditLogDto(
    int Id, int TransactionId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, int ProductId, int ReferenceOrderId, int ReferenceOrderLineId,
    DateTime TransactionDate, string TransactionType, int Quantity,
    decimal ActualCost, DateTime SourceModifiedDate);

public static class TransactionHistoryMappings
{
    public static TransactionHistoryDto ToDto(this TransactionHistory e) => new(
        e.Id, e.ProductId, e.ReferenceOrderId, e.ReferenceOrderLineId,
        e.TransactionDate, e.TransactionType, e.Quantity, e.ActualCost, e.ModifiedDate);

    public static TransactionHistory ToEntity(this CreateTransactionHistoryRequest r) => new()
    {
        ProductId = r.ProductId,
        ReferenceOrderId = r.ReferenceOrderId,
        ReferenceOrderLineId = r.ReferenceOrderLineId,
        TransactionDate = r.TransactionDate,
        TransactionType = r.TransactionType,
        Quantity = r.Quantity,
        ActualCost = r.ActualCost,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateTransactionHistoryRequest r, TransactionHistory e)
    {
        if (r.ProductId.HasValue) e.ProductId = r.ProductId.Value;
        if (r.ReferenceOrderId.HasValue) e.ReferenceOrderId = r.ReferenceOrderId.Value;
        if (r.ReferenceOrderLineId.HasValue) e.ReferenceOrderLineId = r.ReferenceOrderLineId.Value;
        if (r.TransactionDate.HasValue) e.TransactionDate = r.TransactionDate.Value;
        if (r.TransactionType is not null) e.TransactionType = r.TransactionType;
        if (r.Quantity.HasValue) e.Quantity = r.Quantity.Value;
        if (r.ActualCost.HasValue) e.ActualCost = r.ActualCost.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static TransactionHistoryAuditLogDto ToDto(this TransactionHistoryAuditLog a) => new(
        a.Id, a.TransactionId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.ProductId, a.ReferenceOrderId, a.ReferenceOrderLineId,
        a.TransactionDate, a.TransactionType, a.Quantity, a.ActualCost, a.SourceModifiedDate);
}
