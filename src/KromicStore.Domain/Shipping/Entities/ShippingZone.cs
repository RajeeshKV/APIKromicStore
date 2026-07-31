using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shipping.Entities;

/// <summary>
/// ShippingZone aggregate root representing a geographic zone for shipping.
/// Contains countries, regions, and associated shipping methods.
/// </summary>
public sealed class ShippingZone : TenantEntity, IAuditable, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    
    // Geographic coverage
    private readonly List<string> _countries = []; // ISO country codes
    public IReadOnlyList<string> Countries => _countries.AsReadOnly();
    
    private readonly List<ShippingMethod> _methods = [];
    public IReadOnlyList<ShippingMethod> Methods => _methods.AsReadOnly();
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    
    private ShippingZone()
    {
    }
    
    private ShippingZone(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new shipping zone.
    /// </summary>
    public static ShippingZone Create(Guid tenantId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Zone name is required", nameof(name));
        
        var zone = new ShippingZone(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
        
        return zone;
    }
    
    /// <summary>
    /// Add a country to this shipping zone.
    /// </summary>
    public void AddCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be 2 characters (ISO 3166-1 alpha-2)", nameof(countryCode));
        
        var code = countryCode.ToUpperInvariant();
        if (!_countries.Contains(code))
            _countries.Add(code);
    }
    
    /// <summary>
    /// Remove a country from this shipping zone.
    /// </summary>
    public void RemoveCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return;
        
        _countries.Remove(countryCode.ToUpperInvariant());
    }
    
    /// <summary>
    /// Add a shipping method to this zone.
    /// </summary>
    public void AddMethod(ShippingMethod method)
    {
        if (method == null)
            throw new ArgumentNullException(nameof(method));
        
        if (!_methods.Any(m => m.Id == method.Id))
            _methods.Add(method);
    }
    
    /// <summary>
    /// Remove a shipping method from this zone.
    /// </summary>
    public void RemoveMethod(Guid methodId)
    {
        var method = _methods.FirstOrDefault(m => m.Id == methodId);
        if (method != null)
            _methods.Remove(method);
    }
    
    /// <summary>
    /// Activate the shipping zone.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate the shipping zone.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
    
    /// <summary>
    /// Check if this zone covers a specific country.
    /// </summary>
    public bool CoversCountry(string countryCode)
    {
        return _countries.Contains(countryCode.ToUpperInvariant());
    }
}
