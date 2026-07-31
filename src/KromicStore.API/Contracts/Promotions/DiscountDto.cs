namespace KromicStore.API.Contracts.Promotions;

/// <summary>
/// DTO representing a discount/promotion.
/// </summary>
public class DiscountDto
{
    /// <summary>
    /// Unique identifier for the discount.
    /// </summary>
    public Guid DiscountId { get; set; }

    /// <summary>
    /// Discount name/description.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Discount percentage (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Minimum order value required to apply discount.
    /// </summary>
    public decimal? MinOrderValue { get; set; }

    /// <summary>
    /// Whether discount is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When discount was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When discount was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
