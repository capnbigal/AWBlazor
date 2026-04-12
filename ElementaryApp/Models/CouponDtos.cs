using ElementaryApp.Data.Entities;

namespace ElementaryApp.Models;

public sealed record CouponDto(string Id, string Description, int Discount, DateTime ExpiryDate);

public sealed record CreateCouponRequest
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Discount { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public sealed record UpdateCouponRequest
{
    public string? Description { get; set; }
    public int? Discount { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public static class CouponMappings
{
    public static CouponDto ToDto(this Coupon c) => new(c.Id, c.Description, c.Discount, c.ExpiryDate);

    public static Coupon ToEntity(this CreateCouponRequest r) => new()
    {
        Id = r.Id,
        Description = r.Description,
        Discount = r.Discount,
        ExpiryDate = r.ExpiryDate,
    };

    public static void ApplyTo(this UpdateCouponRequest r, Coupon c)
    {
        if (r.Description is not null) c.Description = r.Description;
        if (r.Discount.HasValue) c.Discount = r.Discount.Value;
        if (r.ExpiryDate.HasValue) c.ExpiryDate = r.ExpiryDate.Value;
    }
}
