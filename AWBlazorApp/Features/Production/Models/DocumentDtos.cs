using AWBlazorApp.Features.Production.Domain;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.Production.Models;

public sealed record DocumentDto(
    string DocumentNode, short? DocumentLevel, string Title, int Owner, bool FolderFlag,
    string FileName, string FileExtension, string Revision, int ChangeNumber, byte Status,
    string? DocumentSummary, bool HasDocumentContent, Guid RowGuid, DateTime ModifiedDate);

public sealed record CreateDocumentRequest
{
    /// <summary>Hierarchyid path, e.g. "/1/", "/1/2/". Must be a valid hierarchyid string.</summary>
    public string? DocumentNode { get; set; }
    public string? Title { get; set; }
    public int Owner { get; set; }
    public bool FolderFlag { get; set; }
    public string? FileName { get; set; }
    public string? FileExtension { get; set; }
    public string? Revision { get; set; }
    public int ChangeNumber { get; set; }
    public byte Status { get; set; }
    public string? DocumentSummary { get; set; }
}

public sealed record UpdateDocumentRequest
{
    public string? Title { get; set; }
    public int? Owner { get; set; }
    public bool? FolderFlag { get; set; }
    public string? FileName { get; set; }
    public string? FileExtension { get; set; }
    public string? Revision { get; set; }
    public int? ChangeNumber { get; set; }
    public byte? Status { get; set; }
    public string? DocumentSummary { get; set; }
}

public sealed record DocumentAuditLogDto(
    int Id, string DocumentNode, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? Title, int Owner, bool FolderFlag, string? FileName,
    string? FileExtension, string? Revision, int ChangeNumber, byte Status,
    Guid RowGuid, DateTime SourceModifiedDate);

public static class DocumentMappings
{
    public static DocumentDto ToDto(this Document e) => new(
        e.DocumentNode.ToString(), e.DocumentLevel, e.Title, e.Owner, e.FolderFlag,
        e.FileName, e.FileExtension, e.Revision, e.ChangeNumber, e.Status,
        e.DocumentSummary, e.DocumentContent is { Length: > 0 },
        e.RowGuid, e.ModifiedDate);

    public static Document ToEntity(this CreateDocumentRequest r) => new()
    {
        DocumentNode = HierarchyId.Parse(r.DocumentNode ?? "/"),
        Title = (r.Title ?? string.Empty).Trim(),
        Owner = r.Owner,
        FolderFlag = r.FolderFlag,
        FileName = (r.FileName ?? string.Empty).Trim(),
        FileExtension = (r.FileExtension ?? string.Empty).Trim(),
        Revision = (r.Revision ?? string.Empty).Trim(),
        ChangeNumber = r.ChangeNumber,
        Status = r.Status,
        DocumentSummary = string.IsNullOrWhiteSpace(r.DocumentSummary) ? null : r.DocumentSummary.Trim(),
        RowGuid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateDocumentRequest r, Document e)
    {
        if (r.Title is not null) e.Title = r.Title.Trim();
        if (r.Owner.HasValue) e.Owner = r.Owner.Value;
        if (r.FolderFlag.HasValue) e.FolderFlag = r.FolderFlag.Value;
        if (r.FileName is not null) e.FileName = r.FileName.Trim();
        if (r.FileExtension is not null) e.FileExtension = r.FileExtension.Trim();
        if (r.Revision is not null) e.Revision = r.Revision.Trim();
        if (r.ChangeNumber.HasValue) e.ChangeNumber = r.ChangeNumber.Value;
        if (r.Status.HasValue) e.Status = r.Status.Value;
        if (r.DocumentSummary is not null) e.DocumentSummary = string.IsNullOrWhiteSpace(r.DocumentSummary) ? null : r.DocumentSummary.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static DocumentAuditLogDto ToDto(this DocumentAuditLog a) => new(
        a.Id, a.DocumentNode, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.Title, a.Owner, a.FolderFlag, a.FileName, a.FileExtension, a.Revision,
        a.ChangeNumber, a.Status, a.RowGuid, a.SourceModifiedDate);
}
