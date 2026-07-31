namespace KromicStore.API.Contracts.Promotions;

/// <summary>
/// Request to create a new coupon code.
/// </summary>
public class CreateCouponRequest
{
    /// <summary>
    /// Coupon code (e.g., "SUMMER20", "WELCOME10").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Discount percentage applied by this coupon (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Maximum number of times this coupon can be used (0 = unlimited).
    /// </summary>
    public int MaxUsageCount { get; set; } = 0;

    /// <summary>
    /// Whether coupon is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
