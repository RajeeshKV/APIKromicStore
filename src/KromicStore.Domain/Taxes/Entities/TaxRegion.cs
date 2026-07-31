using KromicStore.Domain.Common;

namespace KromicStore.Domain.Taxes.Entities;

/// <summary>
/// TaxRegion represents a geographic region (country/state) with tax rules.
/// Determines whether tax is inclusive or exclusive.
/// </summary>
public sealed class TaxRegion : TenantEntity, IAuditable, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty; // ISO 3166-1 alpha-2
    public string? StateCode { get; private set; } // For subdivisions (e.g., US states, CA provinces)
    public bool IsTaxInclusive { get; private set; } // true = VAT/GST style, false = Sales tax style
    public bool IsActive { get; private set; }
    
    private readonly List<TaxRule> _rules = [];
    public IReadOnlyList<TaxRule> Rules => _rules.AsReadOnly();
    
    // Auditing and soft delete are inherited from AuditableEntity
    
    private TaxRegion()
    {
    }
    
    private TaxRegion(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new tax region.
    /// </summary>
    public static TaxRegion Create(
        Guid tenantId,
        string name,
        string countryCode,
        bool isTaxInclusive,
        string? stateCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Region name is required", nameof(name));
        
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be 2 characters (ISO 3166-1 alpha-2)", nameof(countryCode));
        
        var region = new TaxRegion(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            CountryCode = countryCode.ToUpperInvariant(),
            StateCode = stateCode?.ToUpperInvariant(),
            IsTaxInclusive = isTaxInclusive,
            IsActive = true
        };
        
        return region;
    }
    
    /// <summary>
    /// Add a tax rule to this region.
    /// </summary>
    public void AddRule(TaxRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        
        if (!_rules.Any(r => r.Id == rule.Id))
            _rules.Add(rule);
    }
    
    /// <summary>
    /// Remove a tax rule from this region.
    /// </summary>
    public void RemoveRule(Guid ruleId)
    {
        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
            _rules.Remove(rule);
    }
    
    /// <summary>
    /// Get applicable tax rate for a product category.
    /// </summary>
    public decimal GetTaxRate(string? productCategory)
    {
        if (string.IsNullOrWhiteSpace(productCategory))
            return 0;
        
        var rule = _rules.FirstOrDefault(r => r.IsActive && r.ProductCategory == productCategory);
        return rule?.TaxRate ?? 0;
    }
    
    /// <summary>
    /// Activate this tax region.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate this tax region.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
