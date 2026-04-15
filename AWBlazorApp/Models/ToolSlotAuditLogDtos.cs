using AWBlazorApp.Data.Entities;
using AWBlazorApp.Data.Entities.ToolSlots;

namespace AWBlazorApp.Models;

public sealed record ToolSlotAuditLogDto(
    int Id,
    int ToolSlotConfigurationId,
    string Action,
    string? ChangedBy,
    DateTime ChangedDate,
    string? ChangeSummary,
    string? Family,
    string? MtCode,
    string? Destination,
    string? Fcl1, string? Fcl2, string? Fcr1,
    string? Ffl1, string? Ffl2,
    string? Ffr1, string? Ffr2, string? Ffr3, string? Ffr4,
    string? Rcl1,
    string? Rcr1, string? Rcr2,
    string? Rfl1,
    string? Rfr1, string? Rfr2,
    bool IsActive);

public static class ToolSlotAuditLogMappings
{
    public static ToolSlotAuditLogDto ToDto(this ToolSlotAuditLog a) => new(
        a.Id,
        a.ToolSlotConfigurationId,
        a.Action,
        a.ChangedBy,
        a.ChangedDate,
        a.ChangeSummary,
        a.Family, a.MtCode, a.Destination,
        a.Fcl1, a.Fcl2, a.Fcr1,
        a.Ffl1, a.Ffl2,
        a.Ffr1, a.Ffr2, a.Ffr3, a.Ffr4,
        a.Rcl1,
        a.Rcr1, a.Rcr2,
        a.Rfl1,
        a.Rfr1, a.Rfr2,
        a.IsActive);
}
