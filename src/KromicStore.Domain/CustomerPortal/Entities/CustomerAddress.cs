using KromicStore.Domain.Common;

namespace KromicStore.Domain.CustomerPortal.Entities;

/// <summary>
/// CustomerAddress represents a saved address for a customer.
/// Can be used for shipping or billing purposes.
/// </summary>
public sealed class CustomerAddress : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = string.Empty; // e.g., "Home", "Office"
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string StateCode { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty; // ISO 3166-1 alpha-2
    public string? PhoneNumber { get; private set; }
    
    // Address type flags
    public bool IsShippingAddress { get; private set; }
    public bool IsBillingAddress { get; private set; }
    public bool IsDefaultShipping { get; private set; }
    public bool IsDefaultBilling { get; private set; }
    
    public bool IsActive { get; private set; }
    
    // Auditing and soft delete are inherited from AuditableEntity
    
    private CustomerAddress()
    {
    }
    
    private CustomerAddress(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new customer address.
    /// </summary>
    public static CustomerAddress Create(
        Guid tenantId,
        Guid customerId,
        string label,
        string street,
        string city,
        string stateCode,
        string postalCode,
        string countryCode,
        string? phoneNumber = null,
        bool isShipping = true,
        bool isBilling = false)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label is required", nameof(label));
        
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required", nameof(stateCode));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required", nameof(postalCode));
        
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be 2 characters (ISO 3166-1 alpha-2)", nameof(countryCode));
        
        if (!isShipping && !isBilling)
            throw new ArgumentException("Address must be marked as shipping or billing", nameof(isShipping));
        
        var address = new CustomerAddress(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId,
            Label = label.Trim(),
            Street = street.Trim(),
            City = city.Trim(),
            StateCode = stateCode.ToUpperInvariant().Trim(),
            PostalCode = postalCode.Trim(),
            CountryCode = countryCode.ToUpperInvariant(),
            PhoneNumber = phoneNumber?.Trim(),
            IsShippingAddress = isShipping,
            IsBillingAddress = isBilling,
            IsActive = true
        };
        
        return address;
    }
    
    /// <summary>
    /// Get formatted full address.
    /// </summary>
    public string GetFormattedAddress() =>
        $"{Street}, {City}, {StateCode} {PostalCode}, {CountryCode}";
    
    /// <summary>
    /// Update address details.
    /// </summary>
    public void UpdateAddress(
        string street,
        string city,
        string stateCode,
        string postalCode,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required", nameof(stateCode));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required", nameof(postalCode));
        
        Street = street.Trim();
        City = city.Trim();
        StateCode = stateCode.ToUpperInvariant().Trim();
        PostalCode = postalCode.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }
    
    /// <summary>
    /// Mark as default shipping address.
    /// </summary>
    public void SetAsDefaultShipping()
    {
        if (!IsShippingAddress)
            throw new InvalidOperationException("Address must be marked as shipping address");
        
        IsDefaultShipping = true;
    }
    
    /// <summary>
    /// Unmark as default shipping address.
    /// </summary>
    public void UnsetAsDefaultShipping()
    {
        IsDefaultShipping = false;
    }
    
    /// <summary>
    /// Mark as default billing address.
    /// </summary>
    public void SetAsDefaultBilling()
    {
        if (!IsBillingAddress)
            throw new InvalidOperationException("Address must be marked as billing address");
        
        IsDefaultBilling = true;
    }
    
    /// <summary>
    /// Unmark as default billing address.
    /// </summary>
    public void UnsetAsDefaultBilling()
    {
        IsDefaultBilling = false;
    }
    
    /// <summary>
    /// Activate the address.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate the address.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
