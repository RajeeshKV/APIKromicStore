using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shipping.Entities;

/// <summary>
/// ShippingMethod represents a delivery method (e.g., Standard, Express, Overnight)
/// with associated rates per shipping zone.
/// </summary>
public sealed class ShippingMethod : TenantEntity, IAuditable
{
    public Guid ShippingZoneId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int EstimatedDaysMin { get; private set; }
    public int EstimatedDaysMax { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    
    // Rates for different weight/value ranges
    private readonly List<ShippingRate> _rates = [];
    public IReadOnlyList<ShippingRate> Rates => _rates.AsReadOnly();
    
    // Auditing is inherited from AuditableEntity
    
    private ShippingMethod()
    {
    }
    
    private ShippingMethod(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new shipping method.
    /// </summary>
    public static ShippingMethod Create(
        Guid tenantId,
        Guid shippingZoneId,
        string name,
        int estimatedDaysMin,
        int estimatedDaysMax,
        string? description = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Method name is required", nameof(name));
        
        if (estimatedDaysMin < 0 || estimatedDaysMax < 0)
            throw new ArgumentException("Estimated days must be non-negative");
        
        if (estimatedDaysMax < estimatedDaysMin)
            throw new ArgumentException("Max days must be >= min days");
        
        var method = new ShippingMethod(Guid.NewGuid(), tenantId)
        {
            ShippingZoneId = shippingZoneId,
            Name = name.Trim(),
            Description = description?.Trim(),
            EstimatedDaysMin = estimatedDaysMin,
            EstimatedDaysMax = estimatedDaysMax,
            IsActive = true,
            DisplayOrder = displayOrder
        };
        
        return method;
    }
    
    /// <summary>
    /// Add a rate to this shipping method.
    /// </summary>
    public void AddRate(ShippingRate rate)
    {
        if (rate == null)
            throw new ArgumentNullException(nameof(rate));
        
        // Validate no overlap with existing rates
        if (_rates.Any(r => r.IsWeightBased == rate.IsWeightBased &&
                           ((rate.IsWeightBased && RateRangesOverlap(r, rate)) ||
                            (!rate.IsWeightBased && r.MinOrderValue <= rate.MaxOrderValue && r.MaxOrderValue >= rate.MinOrderValue))))
        {
            throw new InvalidOperationException("Rate range overlaps with existing rates");
        }
        
        _rates.Add(rate);
    }
    
    /// <summary>
    /// Remove a rate from this shipping method.
    /// </summary>
    public void RemoveRate(Guid rateId)
    {
        var rate = _rates.FirstOrDefault(r => r.Id == rateId);
        if (rate != null)
            _rates.Remove(rate);
    }
    
    /// <summary>
    /// Calculate shipping cost based on weight or order value.
    /// </summary>
    public decimal? CalculateShippingCost(decimal weight, decimal orderValue)
    {
        if (weight < 0 || orderValue < 0)
            return null;
        
        // Try weight-based rate first if available
        var weightRate = _rates.FirstOrDefault(r => r.IsWeightBased && r.MinWeight <= weight && weight <= r.MaxWeight);
        if (weightRate != null)
            return weightRate.Cost;
        
        // Try value-based rate
        var valueRate = _rates.FirstOrDefault(r => !r.IsWeightBased && r.MinOrderValue <= orderValue && orderValue <= r.MaxOrderValue);
        if (valueRate != null)
            return valueRate.Cost;
        
        return null;
    }
    
    /// <summary>
    /// Activate the shipping method.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate the shipping method.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
    
    private static bool RateRangesOverlap(ShippingRate rate1, ShippingRate rate2)
    {
        return rate1.MinWeight <= rate2.MaxWeight && rate1.MaxWeight >= rate2.MinWeight;
    }
}
