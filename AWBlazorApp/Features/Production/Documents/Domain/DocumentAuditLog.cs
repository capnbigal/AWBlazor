using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Production.Documents.Domain;

/// <summary>
/// Audit log for <see cref="Document"/>. EF-managed table <c>dbo.DocumentAuditLogs</c>.
/// Stores the DocumentNode as a string representation of the hierarchyid path.
/// Does NOT snapshot the Document varbinary content (would be huge and not useful).
/// </summary>
public class DocumentAuditLog : AdventureWorksAuditLogBase
{
    /// <summary>String representation of the hierarchyid PK, e.g. "/1/2/".</summary>
    [MaxLength(256)]
    public string DocumentNode { get; set; } = string.Empty;

    [MaxLength(50)] public string? Title { get; set; }
    public int Owner { get; set; }
    public bool FolderFlag { get; set; }
    [MaxLength(400)] public string? FileName { get; set; }
    [MaxLength(8)] public string? FileExtension { get; set; }
    [MaxLength(5)] public string? Revision { get; set; }
    public int ChangeNumber { get; set; }
    public byte Status { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
