using AWBlazorApp.Features.Scheduling.Rules.Domain;
namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public interface IRecalcAction
{
    RecalcActionType ActionType { get; }
    Task<RecalcResult> ExecuteAsync(RecalcContext ctx, CancellationToken ct);
}
