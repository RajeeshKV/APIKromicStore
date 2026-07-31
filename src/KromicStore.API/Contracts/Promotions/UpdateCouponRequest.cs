namespace KromicStore.API.Contracts.Promotions;

/// <summary>
/// Request to update an existing coupon code.
/// </summary>
public class UpdateCouponRequest
{
    /// <summary>
    /// Discount percentage applied by this coupon (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Maximum number of times this coupon can be used (0 = unlimited).
    /// </summary>
    public int MaxUsageCount { get; set; }

    /// <summary>
    /// Whether coupon is currently active.
    /// </summary>
    public bool IsActive { get; set; }
}
