using AWBlazorApp.Features.AdventureWorks.Audit;
using System.Text;
using AWBlazorApp.Features.Person.Domain;

namespace AWBlazorApp.Features.Person.Audit;

public static class StateProvinceAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    public static Snapshot CaptureSnapshot(StateProvince e) => new(e);

    public static StateProvinceAuditLog RecordCreate(StateProvince e, string? by)
        => BuildLog(e, ActionCreated, by, "Created");

    public static StateProvinceAuditLog RecordUpdate(Snapshot before, StateProvince after, string? by)
        => BuildLog(after, ActionUpdated, by, BuildDiffSummary(before, after));

    public static StateProvinceAuditLog RecordDelete(StateProvince e, string? by)
        => BuildLog(e, ActionDeleted, by, "Deleted");

    private static StateProvinceAuditLog BuildLog(StateProvince e, string action, string? by, string? summary)
        => new()
        {
            StateProvinceId = e.Id,
            Action = action,
            ChangedBy = by,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = AuditDiffHelpers.Truncate(summary, 2048),
            StateProvinceCode = e.StateProvinceCode,
            CountryRegionCode = e.CountryRegionCode,
            IsOnlyStateProvinceFlag = e.IsOnlyStateProvinceFlag,
            Name = e.Name,
            TerritoryId = e.TerritoryId,
            RowGuid = e.RowGuid,
            SourceModifiedDate = e.ModifiedDate,
        };

    private static string BuildDiffSummary(Snapshot before, StateProvince after)
    {
        var sb = new StringBuilder();
        AuditDiffHelpers.AppendIfChanged(sb, "StateProvinceCode", before.StateProvinceCode, after.StateProvinceCode);
        AuditDiffHelpers.AppendIfChanged(sb, "CountryRegionCode", before.CountryRegionCode, after.CountryRegionCode);
        AuditDiffHelpers.AppendIfChanged(sb, "IsOnlyStateProvinceFlag", before.IsOnlyStateProvinceFlag, after.IsOnlyStateProvinceFlag);
        AuditDiffHelpers.AppendIfChanged(sb, "Name", before.Name, after.Name);
        AuditDiffHelpers.AppendIfChanged(sb, "TerritoryId", before.TerritoryId, after.TerritoryId);
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    public readonly record struct Snapshot(
        string StateProvinceCode, string CountryRegionCode, bool IsOnlyStateProvinceFlag,
        string Name, int TerritoryId)
    {
        public Snapshot(StateProvince e) : this(
            e.StateProvinceCode, e.CountryRegionCode, e.IsOnlyStateProvinceFlag, e.Name, e.TerritoryId)
        { }
    }
}
