using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Documents.Domain;

/// <summary>
/// Product maintenance documents. Maps onto the pre-existing <c>Production.Document</c> table.
/// The PK is <c>DocumentNode</c> of SQL type <c>hierarchyid</c> — EF Core maps this to
/// <see cref="HierarchyId"/> via the <c>Microsoft.EntityFrameworkCore.SqlServer.HierarchyId</c>
/// package. <c>DocumentLevel</c> is a computed column derived from <c>DocumentNode</c>.
///
/// The real table also has a <c>Document</c> varbinary(max) column for the file content.
/// We map it as <see cref="byte"/>[] but the dialog UI does NOT expose it for editing
/// (same pattern as ProductPhoto image bytes).
/// </summary>
[Table("Document", Schema = "Production")]
public class Document
{
    /// <summary>Primary key. SQL <c>hierarchyid</c> — a path like <c>/1/</c>, <c>/1/2/</c>, etc.</summary>
    [Key]
    [Column("DocumentNode")]
    public HierarchyId DocumentNode { get; set; } = HierarchyId.GetRoot();

    /// <summary>Computed from DocumentNode. Read-only.</summary>
    [Column("DocumentLevel")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public short? DocumentLevel { get; set; }

    [Column("Title")]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    /// <summary>FK to <c>HumanResources.Employee.BusinessEntityID</c>.</summary>
    [Column("Owner")]
    public int Owner { get; set; }

    [Column("FolderFlag")]
    public bool FolderFlag { get; set; }

    [Column("FileName")]
    [MaxLength(400)]
    public string FileName { get; set; } = string.Empty;

    [Column("FileExtension")]
    [MaxLength(8)]
    public string FileExtension { get; set; } = string.Empty;

    [Column("Revision")]
    [MaxLength(5)]
    public string Revision { get; set; } = string.Empty;

    [Column("ChangeNumber")]
    public int ChangeNumber { get; set; }

    /// <summary>1=Pending, 2=Approved, 3=Obsolete.</summary>
    [Column("Status")]
    public byte Status { get; set; }

    [Column("DocumentSummary")]
    public string? DocumentSummary { get; set; }

    /// <summary>File content bytes. Mapped but not editable through the CRUD UI.</summary>
    [Column("Document")]
    public byte[]? DocumentContent { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
