namespace AWBlazorApp.Features.Mes.Services;

/// <summary>
/// State-transition service for <c>mes.ProductionRun</c>. Each method enforces a state guard,
/// stamps the relevant timestamps, and (for Complete) walks the planned/produced quantities
/// through <c>IInventoryService</c> to write WIP_ISSUE + WIP_RECEIPT inventory transactions.
/// </summary>
public interface IProductionRunService
{
    Task<RunTransitionResult> StartAsync(int productionRunId, string? userId, CancellationToken cancellationToken);
    Task<RunTransitionResult> PauseAsync(int productionRunId, string? userId, CancellationToken cancellationToken);
    Task<RunTransitionResult> ResumeAsync(int productionRunId, string? userId, CancellationToken cancellationToken);
    Task<RunTransitionResult> CancelAsync(int productionRunId, string? userId, CancellationToken cancellationToken);

    /// <summary>Complete the run: posts WIP_RECEIPT for the produced quantity, optionally a
    /// WIP_ISSUE for any consumed-from-stock material when <paramref name="materialIssue"/> is
    /// provided, flips status to Completed, and stamps ActualEndAt.</summary>
    Task<RunCompletionResult> CompleteAsync(
        int productionRunId,
        decimal quantityProduced,
        decimal quantityScrapped,
        MaterialIssueRequest? materialIssue,
        string? userId,
        CancellationToken cancellationToken);
}

public sealed record RunTransitionResult(int RunId, string Status);

public sealed record RunCompletionResult(
    int RunId, string RunNumber, decimal QuantityProduced, decimal QuantityScrapped,
    long? WipReceiptTransactionId, long? WipIssueTransactionId);

public sealed record MaterialIssueRequest(
    int InventoryItemId, decimal Quantity, string UnitMeasureCode,
    int FromLocationId, int? LotId);
