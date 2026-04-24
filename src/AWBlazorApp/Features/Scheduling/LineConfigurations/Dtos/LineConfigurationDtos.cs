using AWBlazorApp.Features.Scheduling.LineConfigurations.Domain;

namespace AWBlazorApp.Features.Scheduling.LineConfigurations.Dtos;

public sealed record LineConfigurationDto(
    int Id, short LocationId, int TaktSeconds, byte ShiftsPerDay,
    short MinutesPerShift, int FrozenLookaheadHours, bool IsActive, DateTime ModifiedDate);

public sealed record CreateLineConfigurationRequest
{
    public short LocationId { get; set; }
    public int TaktSeconds { get; set; }
    public byte ShiftsPerDay { get; set; } = 1;
    public short MinutesPerShift { get; set; } = 480;
    public int FrozenLookaheadHours { get; set; } = 72;
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateLineConfigurationRequest
{
    public int? TaktSeconds { get; set; }
    public byte? ShiftsPerDay { get; set; }
    public short? MinutesPerShift { get; set; }
    public int? FrozenLookaheadHours { get; set; }
    public bool? IsActive { get; set; }
}

public static class LineConfigurationMappings
{
    public static LineConfigurationDto ToDto(this LineConfiguration e)
        => new(e.Id, e.LocationId, e.TaktSeconds, e.ShiftsPerDay, e.MinutesPerShift,
            e.FrozenLookaheadHours, e.IsActive, e.ModifiedDate);

    public static LineConfiguration ToEntity(this CreateLineConfigurationRequest r) => new()
    {
        LocationId = r.LocationId,
        TaktSeconds = r.TaktSeconds,
        ShiftsPerDay = r.ShiftsPerDay,
        MinutesPerShift = r.MinutesPerShift,
        FrozenLookaheadHours = r.FrozenLookaheadHours,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateLineConfigurationRequest r, LineConfiguration e)
    {
        if (r.TaktSeconds.HasValue) e.TaktSeconds = r.TaktSeconds.Value;
        if (r.ShiftsPerDay.HasValue) e.ShiftsPerDay = r.ShiftsPerDay.Value;
        if (r.MinutesPerShift.HasValue) e.MinutesPerShift = r.MinutesPerShift.Value;
        if (r.FrozenLookaheadHours.HasValue) e.FrozenLookaheadHours = r.FrozenLookaheadHours.Value;
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }
}
