using KromicStore.Domain.Common;

namespace KromicStore.Domain.Taxes.Entities;

/// <summary>
/// TaxRule represents a tax rate for a specific product category in a tax region.
/// Can have effective date ranges for seasonal or temporary tax changes.
/// </summary>
public sealed class TaxRule : TenantEntity, IAuditable
{
    public Guid TaxRegionId { get; private set; }
    public string ProductCategory { get; private set; } = string.Empty;
    public decimal TaxRate { get; private set; } // e.g., 0.15 for 15%
    public string? Description { get; private set; }
    public DateTime? EffectiveFromUtc { get; private set; }
    public DateTime? EffectiveToUtc { get; private set; }
    public bool IsActive { get; private set; }
    
    // Auditing is inherited from AuditableEntity
    
    private TaxRule()
    {
    }
    
    private TaxRule(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new tax rule.
    /// </summary>
    public static TaxRule Create(
        Guid tenantId,
        Guid taxRegionId,
        string productCategory,
        decimal taxRate,
        string? description = null,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null)
    {
        if (string.IsNullOrWhiteSpace(productCategory))
            throw new ArgumentException("Product category is required", nameof(productCategory));
        
        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1 (0-100%)", nameof(taxRate));
        
        if (effectiveFromUtc.HasValue && effectiveToUtc.HasValue && effectiveToUtc < effectiveFromUtc)
            throw new ArgumentException("Effective to date must be >= effective from date");
        
        var rule = new TaxRule(Guid.NewGuid(), tenantId)
        {
            TaxRegionId = taxRegionId,
            ProductCategory = productCategory.Trim(),
            TaxRate = taxRate,
            Description = description?.Trim(),
            EffectiveFromUtc = effectiveFromUtc,
            EffectiveToUtc = effectiveToUtc,
            IsActive = true
        };
        
        return rule;
    }
    
    /// <summary>
    /// Update the tax rate.
    /// </summary>
    public void UpdateRate(decimal newRate)
    {
        if (newRate < 0 || newRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1 (0-100%)", nameof(newRate));
        
        TaxRate = newRate;
    }
    
    /// <summary>
    /// Set effective date range.
    /// </summary>
    public void SetEffectiveDateRange(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc.HasValue && toUtc.HasValue && toUtc < fromUtc)
            throw new ArgumentException("To date must be >= from date");
        
        EffectiveFromUtc = fromUtc;
        EffectiveToUtc = toUtc;
    }
    
    /// <summary>
    /// Check if this tax rule is currently effective.
    /// </summary>
    public bool IsEffectiveNow(DateTime? asOfUtc = null)
    {
        var now = asOfUtc ?? DateTime.UtcNow;
        
        if (!IsActive)
            return false;
        
        if (EffectiveFromUtc.HasValue && now < EffectiveFromUtc)
            return false;
        
        if (EffectiveToUtc.HasValue && now > EffectiveToUtc)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Activate this tax rule.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate this tax rule.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
