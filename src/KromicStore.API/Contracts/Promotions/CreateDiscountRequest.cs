namespace KromicStore.API.Contracts.Promotions;

/// <summary>
/// Request to create a new discount.
/// </summary>
public class CreateDiscountRequest
{
    /// <summary>
    /// Discount name/description.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Discount percentage (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Minimum order value to apply discount (optional).
    /// </summary>
    public decimal? MinOrderValue { get; set; }

    /// <summary>
    /// Whether discount is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
