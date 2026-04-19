using AWBlazorApp.Features.Purchasing.Domain;

namespace AWBlazorApp.Features.Purchasing.Dtos;

public sealed record VendorDto(
    int Id, string AccountNumber, string Name, byte CreditRating,
    bool PreferredVendorStatus, bool ActiveFlag, string? PurchasingWebServiceUrl, DateTime ModifiedDate);

public sealed record CreateVendorRequest
{
    /// <summary>PK / FK to Person.BusinessEntity. NOT identity — caller must supply.</summary>
    public int Id { get; set; }
    public string? AccountNumber { get; set; }
    public string? Name { get; set; }
    public byte CreditRating { get; set; }
    public bool PreferredVendorStatus { get; set; } = true;
    public bool ActiveFlag { get; set; } = true;
    public string? PurchasingWebServiceUrl { get; set; }
}

public sealed record UpdateVendorRequest
{
    public string? AccountNumber { get; set; }
    public string? Name { get; set; }
    public byte? CreditRating { get; set; }
    public bool? PreferredVendorStatus { get; set; }
    public bool? ActiveFlag { get; set; }
    public string? PurchasingWebServiceUrl { get; set; }
}

public sealed record VendorAuditLogDto(
    int Id, int VendorId, string Action, string? ChangedBy, DateTime ChangedDate,
    string? ChangeSummary, string? AccountNumber, string? Name, byte CreditRating,
    bool PreferredVendorStatus, bool ActiveFlag, string? PurchasingWebServiceUrl, DateTime SourceModifiedDate);

public static class VendorMappings
{
    public static VendorDto ToDto(this Vendor e) => new(
        e.Id, e.AccountNumber, e.Name, e.CreditRating,
        e.PreferredVendorStatus, e.ActiveFlag, e.PurchasingWebServiceUrl, e.ModifiedDate);

    public static Vendor ToEntity(this CreateVendorRequest r) => new()
    {
        Id = r.Id,
        AccountNumber = (r.AccountNumber ?? string.Empty).Trim(),
        Name = (r.Name ?? string.Empty).Trim(),
        CreditRating = r.CreditRating,
        PreferredVendorStatus = r.PreferredVendorStatus,
        ActiveFlag = r.ActiveFlag,
        PurchasingWebServiceUrl = r.PurchasingWebServiceUrl?.Trim(),
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateVendorRequest r, Vendor e)
    {
        if (r.AccountNumber is not null) e.AccountNumber = r.AccountNumber.Trim();
        if (r.Name is not null) e.Name = r.Name.Trim();
        if (r.CreditRating.HasValue) e.CreditRating = r.CreditRating.Value;
        if (r.PreferredVendorStatus.HasValue) e.PreferredVendorStatus = r.PreferredVendorStatus.Value;
        if (r.ActiveFlag.HasValue) e.ActiveFlag = r.ActiveFlag.Value;
        if (r.PurchasingWebServiceUrl is not null) e.PurchasingWebServiceUrl = r.PurchasingWebServiceUrl.Trim();
        e.ModifiedDate = DateTime.UtcNow;
    }

    public static VendorAuditLogDto ToDto(this VendorAuditLog a) => new(
        a.Id, a.VendorId, a.Action, a.ChangedBy, a.ChangedDate, a.ChangeSummary,
        a.AccountNumber, a.Name, a.CreditRating,
        a.PreferredVendorStatus, a.ActiveFlag, a.PurchasingWebServiceUrl, a.SourceModifiedDate);
}
