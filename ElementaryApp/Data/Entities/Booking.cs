using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElementaryApp.Data.Entities;

public class Booking : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    public RoomType RoomType { get; set; }

    public int RoomNumber { get; set; }

    public DateTime BookingStartDate { get; set; }

    public DateTime? BookingEndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [MaxLength(64)]
    public string? CouponId { get; set; }

    [ForeignKey(nameof(CouponId))]
    public Coupon? Coupon { get; set; }

    public string? Notes { get; set; }

    public bool? Cancelled { get; set; }
}
