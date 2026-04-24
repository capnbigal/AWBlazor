namespace AWBlazorApp.Features.Processes.Timelines.Application;

public sealed record TimelineEvent(
    long AuditLogId,
    string EntityType,
    string EntityId,
    string Action,
    DateTime At,
    string? ChangedBy,
    string? Summary,
    string? ChangesJson);
