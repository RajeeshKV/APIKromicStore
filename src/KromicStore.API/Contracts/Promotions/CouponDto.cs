namespace KromicStore.API.Contracts.Promotions;

/// <summary>
/// DTO representing a coupon code.
/// </summary>
public class CouponDto
{
    /// <summary>
    /// Unique identifier for the coupon.
    /// </summary>
    public Guid CouponId { get; set; }

    /// <summary>
    /// Coupon code (e.g., "SUMMER20").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Discount percentage applied by this coupon (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Maximum number of times this coupon can be used (0 = unlimited).
    /// </summary>
    public int MaxUsageCount { get; set; }

    /// <summary>
    /// Number of times this coupon has been used.
    /// </summary>
    public int TimesUsed { get; set; }

    /// <summary>
    /// Whether coupon is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When coupon was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When coupon was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
