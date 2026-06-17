using AWBlazorApp.Features.Scheduling.LineProductAssignments.Domain;

namespace AWBlazorApp.Features.Scheduling.LineProductAssignments.Dtos;

public sealed record LineProductAssignmentDto(
    int Id, short LocationId, int ProductModelId, bool IsActive, DateTime ModifiedDate);

public sealed record CreateLineProductAssignmentRequest
{
    public short LocationId { get; set; }
    public int ProductModelId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateLineProductAssignmentRequest
{
    public bool? IsActive { get; set; }
}

public static class LineProductAssignmentMappings
{
    public static LineProductAssignmentDto ToDto(this LineProductAssignment e)
        => new(e.Id, e.LocationId, e.ProductModelId, e.IsActive, e.ModifiedDate);

    public static LineProductAssignment ToEntity(this CreateLineProductAssignmentRequest r) => new()
    {
        LocationId = r.LocationId,
        ProductModelId = r.ProductModelId,
        IsActive = r.IsActive,
        ModifiedDate = DateTime.UtcNow,
    };

    public static void ApplyTo(this UpdateLineProductAssignmentRequest r, LineProductAssignment e)
    {
        if (r.IsActive.HasValue) e.IsActive = r.IsActive.Value;
        e.ModifiedDate = DateTime.UtcNow;
    }
}
