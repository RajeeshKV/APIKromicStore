using KromicStore.Domain.Common;

namespace KromicStore.Domain.CustomerPortal.Entities;

/// <summary>
/// CustomerProfile represents a customer's profile information within a tenant.
/// Stores name, contact, preferences, and account settings.
/// </summary>
public sealed class CustomerProfile : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid CustomerId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    
    // Preferences
    public bool NewsletterOptIn { get; private set; }
    public string? NotificationPreferences { get; private set; } // JSON serialized
    
    // Account metadata
    public DateTime? LastLoginUtc { get; private set; }
    public int LoginCount { get; private set; }
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    
    private CustomerProfile()
    {
    }
    
    private CustomerProfile(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new customer profile.
    /// </summary>
    public static CustomerProfile Create(
        Guid tenantId,
        Guid customerId,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        DateTime? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));
        
        if (dateOfBirth.HasValue && dateOfBirth > DateTime.UtcNow)
            throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));
        
        var profile = new CustomerProfile(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            DateOfBirth = dateOfBirth,
            NewsletterOptIn = false,
            LoginCount = 0
        };
        
        return profile;
    }
    
    /// <summary>
    /// Get full name (FirstName LastName).
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();
    
    /// <summary>
    /// Update basic profile information.
    /// </summary>
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));
        
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }
    
    /// <summary>
    /// Update newsletter preference.
    /// </summary>
    public void SetNewsletterOptIn(bool optIn)
    {
        NewsletterOptIn = optIn;
    }
    
    /// <summary>
    /// Update notification preferences (stored as JSON).
    /// </summary>
    public void UpdateNotificationPreferences(string? preferencesJson)
    {
        NotificationPreferences = preferencesJson?.Trim();
    }
    
    /// <summary>
    /// Record a login event.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginUtc = DateTime.UtcNow;
        LoginCount++;
    }
    
    /// <summary>
    /// Get age based on date of birth.
    /// </summary>
    public int? GetAge()
    {
        if (!DateOfBirth.HasValue)
            return null;
        
        var today = DateTime.UtcNow;
        var age = today.Year - DateOfBirth.Value.Year;
        
        if (DateOfBirth.Value.Date > today.AddYears(-age))
            age--;
        
        return age;
    }
}
