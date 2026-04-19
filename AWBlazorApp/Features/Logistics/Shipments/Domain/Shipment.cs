using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Logistics.Shipments.Domain;

/// <summary>
/// Outbound shipment header. Covers the full Pick → Pack → Ship → Delivered lifecycle on
/// one row (per the user's preference to fold Delivery into Shipment rather than split).
/// Posting flips <see cref="Status"/> to Shipped and writes one <c>SHIP</c> inventory
/// transaction per line. <see cref="DeliveredAt"/> can be stamped post-hoc when carrier
/// confirmation arrives.
/// </summary>
[Table("Shipment", Schema = "lgx")]
public class Shipment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(32)]
    public string ShipmentNumber { get; set; } = string.Empty;

    public int? SalesOrderId { get; set; }

    public int? CustomerBusinessEntityId { get; set; }

    public int? ShipMethodId { get; set; }

    [MaxLength(128)]
    public string? TrackingNumber { get; set; }

    public int ShippedFromLocationId { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;

    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public DateTime? PostedAt { get; set; }

    [MaxLength(450)]
    public string? PostedByUserId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime ModifiedDate { get; set; }
}

public enum ShipmentStatus : byte
{
    Draft = 1,
    Picked = 2,
    Packed = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
}
