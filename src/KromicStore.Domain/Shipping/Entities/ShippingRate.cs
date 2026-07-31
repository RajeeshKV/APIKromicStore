using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shipping.Entities;

/// <summary>
/// ShippingRate represents a cost tier within a shipping method.
/// Can be based on either weight (kg) or order value.
/// </summary>
public sealed class ShippingRate : TenantEntity, IAuditable
{
    public Guid ShippingMethodId { get; private set; }
    
    // Weight-based rate (kg)
    public decimal MinWeight { get; private set; }
    public decimal MaxWeight { get; private set; }
    
    // Value-based rate (currency)
    public decimal MinOrderValue { get; private set; }
    public decimal MaxOrderValue { get; private set; }
    
    public bool IsWeightBased { get; private set; }
    public decimal Cost { get; private set; }
    public bool IsActive { get; private set; }
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    private ShippingRate()
    {
    }
    
    private ShippingRate(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a weight-based shipping rate.
    /// </summary>
    public static ShippingRate CreateWeightBased(
        Guid tenantId,
        Guid shippingMethodId,
        decimal minWeight,
        decimal maxWeight,
        decimal cost)
    {
        if (minWeight < 0 || maxWeight < 0)
            throw new ArgumentException("Weights must be non-negative");
        
        if (maxWeight < minWeight)
            throw new ArgumentException("Max weight must be >= min weight");
        
        if (cost < 0)
            throw new ArgumentException("Cost must be non-negative");
        
        var rate = new ShippingRate(Guid.NewGuid(), tenantId)
        {
            ShippingMethodId = shippingMethodId,
            MinWeight = minWeight,
            MaxWeight = maxWeight,
            MinOrderValue = 0,
            MaxOrderValue = 0,
            IsWeightBased = true,
            Cost = cost,
            IsActive = true
        };
        
        return rate;
    }
    
    /// <summary>
    /// Create a value-based shipping rate.
    /// </summary>
    public static ShippingRate CreateValueBased(
        Guid tenantId,
        Guid shippingMethodId,
        decimal minOrderValue,
        decimal maxOrderValue,
        decimal cost)
    {
        if (minOrderValue < 0 || maxOrderValue < 0)
            throw new ArgumentException("Order values must be non-negative");
        
        if (maxOrderValue < minOrderValue)
            throw new ArgumentException("Max value must be >= min value");
        
        if (cost < 0)
            throw new ArgumentException("Cost must be non-negative");
        
        var rate = new ShippingRate(Guid.NewGuid(), tenantId)
        {
            ShippingMethodId = shippingMethodId,
            MinWeight = 0,
            MaxWeight = 0,
            MinOrderValue = minOrderValue,
            MaxOrderValue = maxOrderValue,
            IsWeightBased = false,
            Cost = cost,
            IsActive = true
        };
        
        return rate;
    }
    
    /// <summary>
    /// Update the cost of this rate.
    /// </summary>
    public void UpdateCost(decimal newCost)
    {
        if (newCost < 0)
            throw new ArgumentException("Cost must be non-negative", nameof(newCost));
        
        Cost = newCost;
    }
    
    /// <summary>
    /// Activate this rate.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate this rate.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
