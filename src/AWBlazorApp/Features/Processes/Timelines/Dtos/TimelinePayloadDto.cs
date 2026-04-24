using AWBlazorApp.Features.Processes.Timelines.Application;

namespace AWBlazorApp.Features.Processes.Timelines.Dtos;

public sealed record TimelinePayloadDto(
    ChainDescriptorDto Chain,
    string RootEntityId,
    string? RootLabel,
    bool Truncated,
    IReadOnlyList<TimelineEventDto> Events);

public sealed record TimelineEventDto(
    long AuditLogId,
    string EntityType,
    string EntityId,
    string Action,
    DateTime At,
    string? ChangedBy,
    string? Summary,
    string? ChangesJson);

public static class TimelinePayloadMappings
{
    public static TimelineEventDto ToDto(this TimelineEvent e) =>
        new(e.AuditLogId, e.EntityType, e.EntityId, e.Action, e.At, e.ChangedBy, e.Summary, e.ChangesJson);
}
