namespace ElementaryApp.Data.Entities;

/// <summary>
/// Base class for entities that need created/modified/deleted audit tracking.
/// Audit fields are populated automatically by <see cref="AuditingInterceptor"/>.
/// </summary>
public abstract class AuditableEntity
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
