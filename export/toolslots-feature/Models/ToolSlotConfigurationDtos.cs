using ElementaryApp.Data.Entities;

namespace ElementaryApp.Models;

public sealed record ToolSlotConfigurationDto(
    int Id,
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

public sealed record CreateToolSlotConfigurationRequest
{
    public string? Family { get; set; }
    public string? MtCode { get; set; }
    public string? Destination { get; set; }
    public string? Fcl1 { get; set; }
    public string? Fcl2 { get; set; }
    public string? Fcr1 { get; set; }
    public string? Ffl1 { get; set; }
    public string? Ffl2 { get; set; }
    public string? Ffr1 { get; set; }
    public string? Ffr2 { get; set; }
    public string? Ffr3 { get; set; }
    public string? Ffr4 { get; set; }
    public string? Rcl1 { get; set; }
    public string? Rcr1 { get; set; }
    public string? Rcr2 { get; set; }
    public string? Rfl1 { get; set; }
    public string? Rfr1 { get; set; }
    public string? Rfr2 { get; set; }
    public bool IsActive { get; set; }
}

public sealed record UpdateToolSlotConfigurationRequest
{
    public string? Family { get; set; }
    public string? MtCode { get; set; }
    public string? Destination { get; set; }
    public string? Fcl1 { get; set; }
    public string? Fcl2 { get; set; }
    public string? Fcr1 { get; set; }
    public string? Ffl1 { get; set; }
    public string? Ffl2 { get; set; }
    public string? Ffr1 { get; set; }
    public string? Ffr2 { get; set; }
    public string? Ffr3 { get; set; }
    public string? Ffr4 { get; set; }
    public string? Rcl1 { get; set; }
    public string? Rcr1 { get; set; }
    public string? Rcr2 { get; set; }
    public string? Rfl1 { get; set; }
    public string? Rfr1 { get; set; }
    public string? Rfr2 { get; set; }
    public bool? IsActive { get; set; }
}

public static class ToolSlotConfigurationMappings
{
    public static ToolSlotConfigurationDto ToDto(this ToolSlotConfiguration t) => new(
        t.Id, t.Family, t.MtCode, t.Destination,
        t.Fcl1, t.Fcl2, t.Fcr1,
        t.Ffl1, t.Ffl2,
        t.Ffr1, t.Ffr2, t.Ffr3, t.Ffr4,
        t.Rcl1,
        t.Rcr1, t.Rcr2,
        t.Rfl1,
        t.Rfr1, t.Rfr2,
        t.IsActive);

    public static ToolSlotConfiguration ToEntity(this CreateToolSlotConfigurationRequest r) => new()
    {
        Family = r.Family,
        MtCode = r.MtCode,
        Destination = r.Destination,
        Fcl1 = r.Fcl1, Fcl2 = r.Fcl2, Fcr1 = r.Fcr1,
        Ffl1 = r.Ffl1, Ffl2 = r.Ffl2,
        Ffr1 = r.Ffr1, Ffr2 = r.Ffr2, Ffr3 = r.Ffr3, Ffr4 = r.Ffr4,
        Rcl1 = r.Rcl1,
        Rcr1 = r.Rcr1, Rcr2 = r.Rcr2,
        Rfl1 = r.Rfl1,
        Rfr1 = r.Rfr1, Rfr2 = r.Rfr2,
        IsActive = r.IsActive,
    };

    public static void ApplyTo(this UpdateToolSlotConfigurationRequest r, ToolSlotConfiguration t)
    {
        if (r.Family is not null) t.Family = r.Family;
        if (r.MtCode is not null) t.MtCode = r.MtCode;
        if (r.Destination is not null) t.Destination = r.Destination;
        if (r.Fcl1 is not null) t.Fcl1 = r.Fcl1;
        if (r.Fcl2 is not null) t.Fcl2 = r.Fcl2;
        if (r.Fcr1 is not null) t.Fcr1 = r.Fcr1;
        if (r.Ffl1 is not null) t.Ffl1 = r.Ffl1;
        if (r.Ffl2 is not null) t.Ffl2 = r.Ffl2;
        if (r.Ffr1 is not null) t.Ffr1 = r.Ffr1;
        if (r.Ffr2 is not null) t.Ffr2 = r.Ffr2;
        if (r.Ffr3 is not null) t.Ffr3 = r.Ffr3;
        if (r.Ffr4 is not null) t.Ffr4 = r.Ffr4;
        if (r.Rcl1 is not null) t.Rcl1 = r.Rcl1;
        if (r.Rcr1 is not null) t.Rcr1 = r.Rcr1;
        if (r.Rcr2 is not null) t.Rcr2 = r.Rcr2;
        if (r.Rfl1 is not null) t.Rfl1 = r.Rfl1;
        if (r.Rfr1 is not null) t.Rfr1 = r.Rfr1;
        if (r.Rfr2 is not null) t.Rfr2 = r.Rfr2;
        if (r.IsActive.HasValue) t.IsActive = r.IsActive.Value;
    }
}
