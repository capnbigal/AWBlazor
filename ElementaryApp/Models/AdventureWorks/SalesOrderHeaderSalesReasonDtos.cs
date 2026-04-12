using ElementaryApp.Data.Entities.AdventureWorks;

namespace ElementaryApp.Models.AdventureWorks;

public sealed record SalesOrderHeaderSalesReasonDto(
    int SalesOrderId, int SalesReasonId, DateTime ModifiedDate);

public sealed record CreateSalesOrderHeaderSalesReasonRequest
{
    public int SalesOrderId { get; set; }
    public int SalesReasonId { get; set; }
}

/// <summary>
/// Pure junction table — there are no non-key columns to update beyond ModifiedDate, so
/// PATCH effectively just touches the row's modified-date stamp.
/// </summary>
public sealed record UpdateSalesOrderHeaderSalesReasonRequest
{
    // Reserved for future expansion. Touching this row updates ModifiedDate.
}

public sealed record SalesOrderHeaderSalesReasonAuditLogDto(
    int Id, int SalesOrderId, int SalesReasonId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, DateTime SourceModifiedDate);

public static class SalesOrderHeaderSalesReasonMappings
{
    public static SalesOrderHeaderSalesReasonDto ToDto(this SalesOrderHeaderSalesReason e) => new(
        e.SalesOrderId, e.SalesReasonId, e.ModifiedDate);

    public static SalesOrderHeaderSalesReason ToEntity(this CreateSalesOrderHeaderSalesReasonRequest r) => new()
    {
        SalesOrderId = r.SalesOrderId,
        SalesReasonId = r.SalesReasonId,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateSalesOrderHeaderSalesReasonRequest _, SalesOrderHeaderSalesReason e)
    {
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static SalesOrderHeaderSalesReasonAuditLogDto ToDto(this SalesOrderHeaderSalesReasonAuditLog a) => new(
        a.Id, a.SalesOrderId, a.SalesReasonId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary, a.SourceModifiedDate);
}
