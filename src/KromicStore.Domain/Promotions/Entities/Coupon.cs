using KromicStore.Domain.Common;

namespace KromicStore.Domain.Promotions.Entities;

/// <summary>
/// Coupon represents a promotional code that can be applied to orders.
/// Tracks usage, validity period, and discount details.
/// </summary>
public sealed class Coupon : TenantEntity, IAuditable, ISoftDeletable
{
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid DiscountId { get; private set; } // Links to the discount this coupon provides
    
    // Usage limits
    public int? MaxUsageCount { get; private set; } // null = unlimited
    public int? MaxUsagePerCustomer { get; private set; } // null = unlimited
    public int CurrentUsageCount { get; private set; }
    
    // Validity
    public DateTime ValidFromUtc { get; private set; }
    public DateTime ValidToUtc { get; private set; }
    public bool IsActive { get; private set; }
    
    // Minimum order requirements
    public decimal? MinimumOrderValue { get; private set; } // null = no minimum
    public string? ApplicableCategories { get; private set; } // Comma-separated category IDs, null = all
    
    // Auditing and soft delete are inherited from AuditableEntity
    
    private Coupon()
    {
    }
    
    private Coupon(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new coupon.
    /// </summary>
    public static Coupon Create(
        Guid tenantId,
        string code,
        Guid discountId,
        DateTime validFromUtc,
        DateTime validToUtc,
        string? description = null,
        int? maxUsageCount = null,
        int? maxUsagePerCustomer = null,
        decimal? minimumOrderValue = null,
        string? applicableCategories = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Coupon code is required", nameof(code));
        
        if (validToUtc <= validFromUtc)
            throw new ArgumentException("Valid to date must be after valid from date");
        
        if (maxUsageCount.HasValue && maxUsageCount < 1)
            throw new ArgumentException("Max usage count must be >= 1", nameof(maxUsageCount));
        
        if (maxUsagePerCustomer.HasValue && maxUsagePerCustomer < 1)
            throw new ArgumentException("Max usage per customer must be >= 1", nameof(maxUsagePerCustomer));
        
        if (minimumOrderValue.HasValue && minimumOrderValue < 0)
            throw new ArgumentException("Minimum order value cannot be negative", nameof(minimumOrderValue));
        
        var coupon = new Coupon(Guid.NewGuid(), tenantId)
        {
            Code = code.ToUpperInvariant().Trim(),
            DiscountId = discountId,
            Description = description?.Trim(),
            MaxUsageCount = maxUsageCount,
            MaxUsagePerCustomer = maxUsagePerCustomer,
            CurrentUsageCount = 0,
            ValidFromUtc = validFromUtc,
            ValidToUtc = validToUtc,
            IsActive = true,
            MinimumOrderValue = minimumOrderValue,
            ApplicableCategories = applicableCategories?.Trim()
        };
        
        return coupon;
    }
    
    /// <summary>
    /// Check if the coupon can be used (valid, not expired, usage limits not exceeded).
    /// </summary>
    public bool CanBeUsed(int? currentCustomerUsageCount = null)
    {
        if (!IsActive || IsDeleted)
            return false;
        
        var now = DateTime.UtcNow;
        if (now < ValidFromUtc || now > ValidToUtc)
            return false;
        
        if (MaxUsageCount.HasValue && CurrentUsageCount >= MaxUsageCount)
            return false;
        
        if (MaxUsagePerCustomer.HasValue && currentCustomerUsageCount.HasValue && currentCustomerUsageCount >= MaxUsagePerCustomer)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Increment the usage count.
    /// </summary>
    public void IncrementUsage()
    {
        if (!CanBeUsed())
            throw new InvalidOperationException("Coupon cannot be used");
        
        CurrentUsageCount++;
    }
    
    /// <summary>
    /// Check if coupon applies to a specific category.
    /// </summary>
    public bool AppliesToCategory(string? categoryId)
    {
        if (string.IsNullOrEmpty(ApplicableCategories))
            return true; // Applies to all categories
        
        if (string.IsNullOrEmpty(categoryId))
            return false;
        
        var categories = ApplicableCategories.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .ToList();
        
        return categories.Contains(categoryId);
    }
    
    /// <summary>
    /// Activate the coupon.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate the coupon.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
