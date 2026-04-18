namespace AWBlazorApp.Features.Engineering.Services;

/// <summary>
/// Engineering Change Order workflow. Transitions: Draft → UnderReview → Approved or Rejected.
/// Cancel is allowed from Draft or UnderReview. When an ECO is approved, any
/// <c>EcoAffectedItem</c> rows of kind <c>Bom</c> or <c>Routing</c> cause the referenced
/// revision to be activated (and prior active revisions for the same product to be cleared).
/// Product and Document affected-kinds are informational only.
/// </summary>
public interface IEcoService
{
    Task SubmitForReviewAsync(int ecoId, string? userId, CancellationToken cancellationToken);
    Task ApproveAsync(int ecoId, string? decisionNotes, string? userId, CancellationToken cancellationToken);
    Task RejectAsync(int ecoId, string? decisionNotes, string? userId, CancellationToken cancellationToken);
    Task CancelAsync(int ecoId, string? userId, CancellationToken cancellationToken);
}
