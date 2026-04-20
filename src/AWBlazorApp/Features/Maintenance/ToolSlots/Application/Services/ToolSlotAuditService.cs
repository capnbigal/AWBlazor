using System.Text;
using AWBlazorApp.Features.Maintenance.ToolSlots.Domain;
using AWBlazorApp.Features.Maintenance.ToolSlots.Dtos;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Domain;

namespace AWBlazorApp.Features.Maintenance.ToolSlots.Application.Services;

/// <summary>
/// Builds <see cref="ToolSlotAuditLog"/> rows for create/update/delete operations against
/// <see cref="ToolSlotConfiguration"/>. Callers add the returned row to their existing
/// <see cref="ApplicationDbContext"/> and save in the same transaction as the underlying
/// CRUD operation.
/// </summary>
public static class ToolSlotAuditService
{
    public const string ActionCreated = "Created";
    public const string ActionUpdated = "Updated";
    public const string ActionDeleted = "Deleted";

    /// <summary>Snapshot builder — call this BEFORE mutating an entity to capture the pre-update state.</summary>
    public static ToolSlotSnapshot Snapshot(ToolSlotConfiguration slot) => new(slot);

    /// <summary>
    /// Records a create. The <paramref name="slot"/> entity must already have been saved
    /// so its identity-generated <see cref="ToolSlotConfiguration.Id"/> is populated.
    /// </summary>
    public static ToolSlotAuditLog RecordCreate(ToolSlotConfiguration slot, string? changedBy)
        => BuildLog(slot, ActionCreated, changedBy, changeSummary: "Created");

    /// <summary>
    /// Records an update. Pass the pre-update <paramref name="before"/> snapshot and the
    /// current <paramref name="after"/> entity to get a diff summary.
    /// </summary>
    public static ToolSlotAuditLog RecordUpdate(ToolSlotSnapshot before, ToolSlotConfiguration after, string? changedBy)
        => BuildLog(after, ActionUpdated, changedBy, changeSummary: BuildDiffSummary(before, after));

    /// <summary>Records a delete. <paramref name="slot"/> is the about-to-be-deleted entity.</summary>
    public static ToolSlotAuditLog RecordDelete(ToolSlotConfiguration slot, string? changedBy)
        => BuildLog(slot, ActionDeleted, changedBy, changeSummary: "Deleted");

    private static ToolSlotAuditLog BuildLog(
        ToolSlotConfiguration slot, string action, string? changedBy, string? changeSummary)
        => new()
        {
            ToolSlotConfigurationId = slot.Id,
            Action = action,
            ChangedBy = changedBy,
            ChangedDate = DateTime.UtcNow,
            ChangeSummary = Truncate(changeSummary, 2048),
            Family = slot.Family,
            MtCode = slot.MtCode,
            Destination = slot.Destination,
            Fcl1 = slot.Fcl1, Fcl2 = slot.Fcl2, Fcr1 = slot.Fcr1,
            Ffl1 = slot.Ffl1, Ffl2 = slot.Ffl2,
            Ffr1 = slot.Ffr1, Ffr2 = slot.Ffr2, Ffr3 = slot.Ffr3, Ffr4 = slot.Ffr4,
            Rcl1 = slot.Rcl1,
            Rcr1 = slot.Rcr1, Rcr2 = slot.Rcr2,
            Rfl1 = slot.Rfl1,
            Rfr1 = slot.Rfr1, Rfr2 = slot.Rfr2,
            IsActive = slot.IsActive,
        };

    private static string BuildDiffSummary(ToolSlotSnapshot before, ToolSlotConfiguration after)
    {
        var sb = new StringBuilder();
        AppendIfChanged(sb, "Family", before.Family, after.Family);
        AppendIfChanged(sb, "MtCode", before.MtCode, after.MtCode);
        AppendIfChanged(sb, "Destination", before.Destination, after.Destination);
        AppendIfChanged(sb, "Fcl1", before.Fcl1, after.Fcl1);
        AppendIfChanged(sb, "Fcl2", before.Fcl2, after.Fcl2);
        AppendIfChanged(sb, "Fcr1", before.Fcr1, after.Fcr1);
        AppendIfChanged(sb, "Ffl1", before.Ffl1, after.Ffl1);
        AppendIfChanged(sb, "Ffl2", before.Ffl2, after.Ffl2);
        AppendIfChanged(sb, "Ffr1", before.Ffr1, after.Ffr1);
        AppendIfChanged(sb, "Ffr2", before.Ffr2, after.Ffr2);
        AppendIfChanged(sb, "Ffr3", before.Ffr3, after.Ffr3);
        AppendIfChanged(sb, "Ffr4", before.Ffr4, after.Ffr4);
        AppendIfChanged(sb, "Rcl1", before.Rcl1, after.Rcl1);
        AppendIfChanged(sb, "Rcr1", before.Rcr1, after.Rcr1);
        AppendIfChanged(sb, "Rcr2", before.Rcr2, after.Rcr2);
        AppendIfChanged(sb, "Rfl1", before.Rfl1, after.Rfl1);
        AppendIfChanged(sb, "Rfr1", before.Rfr1, after.Rfr1);
        AppendIfChanged(sb, "Rfr2", before.Rfr2, after.Rfr2);
        if (before.IsActive != after.IsActive)
        {
            AppendSeparator(sb);
            sb.Append("IsActive: ").Append(before.IsActive).Append(" → ").Append(after.IsActive);
        }
        return sb.Length == 0 ? "No changes" : sb.ToString();
    }

    private static void AppendIfChanged(StringBuilder sb, string name, string? a, string? b)
    {
        if (string.Equals(a, b, StringComparison.Ordinal)) return;
        AppendSeparator(sb);
        sb.Append(name).Append(": ").Append(Format(a)).Append(" → ").Append(Format(b));
    }

    private static void AppendSeparator(StringBuilder sb)
    {
        if (sb.Length > 0) sb.Append("; ");
    }

    private static string Format(string? v) => string.IsNullOrEmpty(v) ? "(empty)" : v;

    private static string? Truncate(string? value, int maxLength)
    {
        if (value is null) return null;
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }

    /// <summary>
    /// Snapshot struct capturing every mutable scalar on a <see cref="ToolSlotConfiguration"/>
    /// at a point in time. Used for diff computation.
    /// </summary>
    public readonly record struct ToolSlotSnapshot(
        string? Family, string? MtCode, string? Destination,
        string? Fcl1, string? Fcl2, string? Fcr1,
        string? Ffl1, string? Ffl2,
        string? Ffr1, string? Ffr2, string? Ffr3, string? Ffr4,
        string? Rcl1,
        string? Rcr1, string? Rcr2,
        string? Rfl1,
        string? Rfr1, string? Rfr2,
        bool IsActive)
    {
        public ToolSlotSnapshot(ToolSlotConfiguration slot) : this(
            slot.Family, slot.MtCode, slot.Destination,
            slot.Fcl1, slot.Fcl2, slot.Fcr1,
            slot.Ffl1, slot.Ffl2,
            slot.Ffr1, slot.Ffr2, slot.Ffr3, slot.Ffr4,
            slot.Rcl1,
            slot.Rcr1, slot.Rcr2,
            slot.Rfl1,
            slot.Rfr1, slot.Rfr2,
            slot.IsActive)
        { }
    }
}
