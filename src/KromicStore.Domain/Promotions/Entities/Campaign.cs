using KromicStore.Domain.Common;

namespace KromicStore.Domain.Promotions.Entities;

/// <summary>
/// Campaign represents a promotional campaign that groups discounts together.
/// Used for coordinated promotions like "Summer Sale" or "Flash Sale".
/// </summary>
public sealed class Campaign : TenantEntity, IAuditable, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    
    // Validity
    public DateTime ValidFromUtc { get; private set; }
    public DateTime ValidToUtc { get; private set; }
    public bool IsActive { get; private set; }
    
    // Associated discounts
    private readonly List<Guid> _discountIds = [];
    public IReadOnlyList<Guid> DiscountIds => _discountIds.AsReadOnly();
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    
    private Campaign()
    {
    }
    
    private Campaign(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new campaign.
    /// </summary>
    public static Campaign Create(
        Guid tenantId,
        string name,
        DateTime validFromUtc,
        DateTime validToUtc,
        string? description = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Campaign name is required", nameof(name));
        
        if (validToUtc <= validFromUtc)
            throw new ArgumentException("Valid to date must be after valid from date");
        
        var campaign = new Campaign(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            ValidFromUtc = validFromUtc,
            ValidToUtc = validToUtc,
            IsActive = true,
            DisplayOrder = displayOrder
        };
        
        return campaign;
    }
    
    /// <summary>
    /// Add a discount to this campaign.
    /// </summary>
    public void AddDiscount(Guid discountId)
    {
        if (discountId == Guid.Empty)
            throw new ArgumentException("Discount ID cannot be empty", nameof(discountId));
        
        if (!_discountIds.Contains(discountId))
            _discountIds.Add(discountId);
    }
    
    /// <summary>
    /// Remove a discount from this campaign.
    /// </summary>
    public void RemoveDiscount(Guid discountId)
    {
        _discountIds.Remove(discountId);
    }
    
    /// <summary>
    /// Check if the campaign is currently active and valid.
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive || IsDeleted)
            return false;
        
        var now = DateTime.UtcNow;
        return now >= ValidFromUtc && now <= ValidToUtc;
    }
    
    /// <summary>
    /// Activate the campaign.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Deactivate the campaign.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
