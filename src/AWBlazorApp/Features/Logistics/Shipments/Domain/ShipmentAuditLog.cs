using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace AWBlazorApp.Features.Logistics.Shipments.Domain;

public class ShipmentAuditLog : AdventureWorksAuditLogBase
{
    public int ShipmentId { get; set; }

    [MaxLength(32)] public string? ShipmentNumber { get; set; }
    public int? SalesOrderId { get; set; }
    public int? CustomerBusinessEntityId { get; set; }
    public int? ShipMethodId { get; set; }
    [MaxLength(128)] public string? TrackingNumber { get; set; }
    public int ShippedFromLocationId { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? PostedAt { get; set; }
    [MaxLength(450)] public string? PostedByUserId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime SourceModifiedDate { get; set; }
}
