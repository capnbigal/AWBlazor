using System.Text;
using AWBlazorApp.Features.AdventureWorks.Domain;

namespace AWBlazorApp.Features.AdventureWorks.Audit;

public static class SalesTaxRateAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(SalesTaxRate e) => new(e);

    public static SalesTaxRateAuditLog RecordCreate(SalesTaxRate e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static SalesTaxRateAuditLog RecordUpdate(Snapshot before, SalesTaxRate after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static SalesTaxRateAuditLog RecordDelete(SalesTaxRate e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static SalesTaxRateAuditLog BuildLog(SalesTaxRate e, string action, string? by, string? summary)
        => new()
        {
            SalesTaxRateId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            StateProvinceId = e.StateProvinceId,
            TaxType = e.TaxType,
            TaxRate = e.TaxRate,
            Name = e.Name,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, SalesTaxRate after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "StateProvinceId", before.StateProvinceId, after.StateProvinceId);
        AuditDiffHelpers.AppendIfChanged(sb, "TaxType", before.TaxType, after.TaxType);
        AuditDiffHelpers.AppendIfChanged(sb, "TaxRate", before.TaxRate, after.TaxRate);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(int StateProvinceId, byte TaxType, decimal TaxRate, string Name)
    {
        public Snapshot(SalesTaxRate e) : this(e.StateProvinceId, e.TaxType, e.TaxRate, e.Name) { }
    }
}
