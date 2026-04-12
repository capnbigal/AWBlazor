using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities;

public class Coupon
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Description { get; set; } = string.Empty;

    public int Discount { get; set; }

    public DateTime ExpiryDate { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
