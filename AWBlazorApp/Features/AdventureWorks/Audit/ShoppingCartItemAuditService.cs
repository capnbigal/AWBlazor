using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class ShoppingCartItemAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(ShoppingCartItem e) => new(e);

    public static ShoppingCartItemAuditLog RecordCreate(ShoppingCartItem e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static ShoppingCartItemAuditLog RecordUpdate(Snapshot before, ShoppingCartItem after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static ShoppingCartItemAuditLog RecordDelete(ShoppingCartItem e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static ShoppingCartItemAuditLog BuildLog(ShoppingCartItem e, string action, string? by, string? summary)
        => new()
        {
            ShoppingCartItemId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            ShoppingCartId = e.ShoppingCartId,
            Quantity = e.Quantity,
            ProductId = e.ProductId,
            DateCreated = e.DateCreated,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, ShoppingCartItem after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "ShoppingCartId", before.ShoppingCartId, after.ShoppingCartId);
        AuditDiffHelpers.AppendIfChanged(sb, "Quantity", before.Quantity, after.Quantity);
        AuditDiffHelpers.AppendIfChanged(sb, "ProductId", before.ProductId, after.ProductId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(string ShoppingCartId, int Quantity, int ProductId)
    {
        public Snapshot(ShoppingCartItem e) : this(e.ShoppingCartId, e.Quantity, e.ProductId) { }
    }
}
