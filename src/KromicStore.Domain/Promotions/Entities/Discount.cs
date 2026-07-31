using KromicStore.Domain.Common;

namespace KromicStore.Domain.Promotions.Entities;

/// <summary>
/// DiscountType enumeration for different discount mechanisms.
/// </summary>
public enum DiscountType
{
    FixedAmount,      // $X off
    PercentageAmount, // X% off
    BuyXGetY,         // Buy X quantity of product A, get Y quantity free/discounted of product B
    FreeShipping      // Free shipping
}

/// <summary>
/// Discount aggregate root representing a promotional discount rule.
/// Can be applied via coupons or campaigns.
/// </summary>
public sealed class Discount : TenantEntity, IAuditable, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DiscountType Type { get; private set; }
    
    // Fixed amount discount
    public decimal? FixedAmount { get; private set; }
    
    // Percentage discount
    public decimal? PercentageAmount { get; private set; } // e.g., 0.15 for 15%
    public decimal? MaxDiscountAmount { get; private set; } // Cap on discount
    
    // Buy X Get Y
    public string? BuyProductId { get; private set; }
    public int? BuyQuantity { get; private set; }
    public string? GetProductId { get; private set; }
    public int? GetQuantity { get; private set; }
    public decimal? GetDiscount { get; private set; } // Percentage or fixed amount
    
    // Free Shipping
    public decimal? FreeShippingMinimum { get; private set; } // Minimum order value for free shipping
    
    // Application scope
    public bool AppliesToWholeOrder { get; private set; } // true = applies to entire order, false = applies to items only
    public string? ApplicableProductIds { get; private set; } // Comma-separated, null = all products
    public string? ApplicableCategories { get; private set; } // Comma-separated, null = all categories
    
    // Validity
    public DateTime ValidFromUtc { get; private set; }
    public DateTime ValidToUtc { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    
    // Usage
    public int? MaxUsageCount { get; private set; }
    public int CurrentUsageCount { get; private set; }
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    
    private Discount()
    {
    }
    
    private Discount(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a fixed amount discount.
    /// </summary>
    public static Discount CreateFixedAmountDiscount(
        Guid tenantId,
        string name,
        decimal amount,
        DateTime validFromUtc,
        DateTime validToUtc,
        string? description = null,
        bool appliesToWholeOrder = true,
        string? applicableProductIds = null,
        string? applicableCategories = null,
        int? maxUsageCount = null,
        int displayOrder = 0)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        var discount = new Discount(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = DiscountType.FixedAmount,
            FixedAmount = amount,
            AppliesToWholeOrder = appliesToWholeOrder,
            ApplicableProductIds = applicableProductIds?.Trim(),
            ApplicableCategories = applicableCategories?.Trim(),
            ValidFromUtc = validFromUtc,
            ValidToUtc = validToUtc,
            IsActive = true,
            DisplayOrder = displayOrder,
            MaxUsageCount = maxUsageCount,
            CurrentUsageCount = 0
        };
        
        ValidateDateRange(validFromUtc, validToUtc);
        return discount;
    }
    
    /// <summary>
    /// Create a percentage discount.
    /// </summary>
    public static Discount CreatePercentageDiscount(
        Guid tenantId,
        string name,
        decimal percentage,
        DateTime validFromUtc,
        DateTime validToUtc,
        string? description = null,
        decimal? maxDiscountAmount = null,
        bool appliesToWholeOrder = true,
        string? applicableProductIds = null,
        string? applicableCategories = null,
        int? maxUsageCount = null,
        int displayOrder = 0)
    {
        if (percentage <= 0 || percentage > 1)
            throw new ArgumentException("Percentage must be between 0 and 1 (0-100%)", nameof(percentage));
        
        if (maxDiscountAmount.HasValue && maxDiscountAmount < 0)
            throw new ArgumentException("Max discount amount cannot be negative", nameof(maxDiscountAmount));
        
        var discount = new Discount(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = DiscountType.PercentageAmount,
            PercentageAmount = percentage,
            MaxDiscountAmount = maxDiscountAmount,
            AppliesToWholeOrder = appliesToWholeOrder,
            ApplicableProductIds = applicableProductIds?.Trim(),
            ApplicableCategories = applicableCategories?.Trim(),
            ValidFromUtc = validFromUtc,
            ValidToUtc = validToUtc,
            IsActive = true,
            DisplayOrder = displayOrder,
            MaxUsageCount = maxUsageCount,
            CurrentUsageCount = 0
        };
        
        ValidateDateRange(validFromUtc, validToUtc);
        return discount;
    }
    
    /// <summary>
    /// Create a free shipping discount.
    /// </summary>
    public static Discount CreateFreeShippingDiscount(
        Guid tenantId,
        string name,
        DateTime validFromUtc,
        DateTime validToUtc,
        string? description = null,
        decimal? minimumOrderValue = null,
        int? maxUsageCount = null,
        int displayOrder = 0)
    {
        if (minimumOrderValue.HasValue && minimumOrderValue < 0)
            throw new ArgumentException("Minimum order value cannot be negative", nameof(minimumOrderValue));
        
        var discount = new Discount(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = DiscountType.FreeShipping,
            FreeShippingMinimum = minimumOrderValue,
            ValidFromUtc = validFromUtc,
            ValidToUtc = validToUtc,
            IsActive = true,
            DisplayOrder = displayOrder,
            MaxUsageCount = maxUsageCount,
            CurrentUsageCount = 0
        };
        
        ValidateDateRange(validFromUtc, validToUtc);
        return discount;
    }
    
    /// <summary>
    /// Check if the discount is currently valid.
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive || IsDeleted)
            return false;
        
        var now = DateTime.UtcNow;
        if (now < ValidFromUtc || now > ValidToUtc)
            return false;
        
        if (MaxUsageCount.HasValue && CurrentUsageCount >= MaxUsageCount)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Increment usage count.
    /// </summary>
    public void IncrementUsage()
    {
        if (!IsValid())
            throw new InvalidOperationException("Discount cannot be used");
        
        CurrentUsageCount++;
    }
    
    /// <summary>
    /// Calculate the discount amount for a given order amount.
    /// </summary>
    public decimal CalculateDiscountAmount(decimal orderAmount)
    {
        if (orderAmount <= 0)
            return 0;
        
        return Type switch
        {
            DiscountType.FixedAmount => Math.Min(FixedAmount ?? 0, orderAmount),
            DiscountType.PercentageAmount => CalculatePercentageDiscount(orderAmount),
            DiscountType.FreeShipping => 0, // Handled separately in shipping calculation
            _ => 0
        };
    }
    
    private decimal CalculatePercentageDiscount(decimal orderAmount)
    {
        var discountAmount = orderAmount * (PercentageAmount ?? 0);
        if (MaxDiscountAmount.HasValue)
            discountAmount = Math.Min(discountAmount, MaxDiscountAmount.Value);
        return discountAmount;
    }
    
    /// <summary>
    /// Check if discount applies to a specific product.
    /// </summary>
    public bool AppliesToProduct(string productId, string? categoryId = null)
    {
        if (string.IsNullOrEmpty(ApplicableProductIds) && string.IsNullOrEmpty(ApplicableCategories))
            return true; // Applies to all
        
        // Check product ID match
        if (!string.IsNullOrEmpty(ApplicableProductIds))
        {
            var productIds = ApplicableProductIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();
            if (productIds.Contains(productId))
                return true;
        }
        
        // Check category match
        if (!string.IsNullOrEmpty(ApplicableCategories) && !string.IsNullOrEmpty(categoryId))
        {
            var categories = ApplicableCategories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToList();
            if (categories.Contains(categoryId))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Activate this discount.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate this discount.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
    
    private static void ValidateDateRange(DateTime validFromUtc, DateTime validToUtc)
    {
        if (validToUtc <= validFromUtc)
            throw new ArgumentException("Valid to date must be after valid from date");
    }
}
