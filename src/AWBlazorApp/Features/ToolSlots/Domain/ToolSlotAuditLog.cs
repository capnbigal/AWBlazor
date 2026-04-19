using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.ToolSlots.Domain;

/// <summary>
/// Records a single change to a <see cref="ToolSlotConfiguration"/>. Each row is a
/// self-contained snapshot of the post-change state so the history can be read without
/// touching the (externally-managed) <c>dbo.ToolSlotConfigurations</c> table.
/// </summary>
/// <remarks>
/// Written by <c>ToolSlotAuditService</c> from both the Blazor CRUD pages and the
/// minimal-API endpoints. This table IS managed by EF migrations — unlike the
/// <c>ToolSlotConfigurations</c> source table, which the DBA owns.
/// </remarks>
public class ToolSlotAuditLog
{
    public int Id { get; set; }

    /// <summary>Id (CID) of the affected <see cref="ToolSlotConfiguration"/> row.</summary>
    public int ToolSlotConfigurationId { get; set; }

    /// <summary>One of <c>Created</c>, <c>Updated</c>, <c>Deleted</c>.</summary>
    [MaxLength(16)]
    public string Action { get; set; } = string.Empty;

    /// <summary>Identity name of the user who made the change (may be null for background jobs).</summary>
    [MaxLength(256)]
    public string? ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    /// <summary>
    /// Human-readable summary of what changed. For <c>Updated</c> rows, this is a compact
    /// diff like "Family: A → B; IsActive: True → False". For <c>Created</c>/<c>Deleted</c>
    /// rows, this is a short description (the snapshot columns hold the actual state).
    /// </summary>
    [MaxLength(2048)]
    public string? ChangeSummary { get; set; }

    // ── Snapshot of the row at the time of the change ────────────────────────────────────
    [MaxLength(255)] public string? Family { get; set; }
    [MaxLength(255)] public string? MtCode { get; set; }
    [MaxLength(255)] public string? Destination { get; set; }
    [MaxLength(255)] public string? Fcl1 { get; set; }
    [MaxLength(255)] public string? Fcl2 { get; set; }
    [MaxLength(255)] public string? Fcr1 { get; set; }
    [MaxLength(255)] public string? Ffl1 { get; set; }
    [MaxLength(255)] public string? Ffl2 { get; set; }
    [MaxLength(255)] public string? Ffr1 { get; set; }
    [MaxLength(255)] public string? Ffr2 { get; set; }
    [MaxLength(255)] public string? Ffr3 { get; set; }
    [MaxLength(255)] public string? Ffr4 { get; set; }
    [MaxLength(255)] public string? Rcl1 { get; set; }
    [MaxLength(255)] public string? Rcr1 { get; set; }
    [MaxLength(255)] public string? Rcr2 { get; set; }
    [MaxLength(255)] public string? Rfl1 { get; set; }
    [MaxLength(255)] public string? Rfr1 { get; set; }
    [MaxLength(255)] public string? Rfr2 { get; set; }
    public bool IsActive { get; set; }
}
