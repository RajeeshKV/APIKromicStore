namespace KromicStore.API.Contracts.Shipping;

/// <summary>
/// DTO representing a shipping method.
/// </summary>
public class ShippingMethodDto
{
    /// <summary>
    /// Unique identifier for the shipping method.
    /// </summary>
    public Guid MethodId { get; set; }

    /// <summary>
    /// Shipping zone this method belongs to.
    /// </summary>
    public Guid ShippingZoneId { get; set; }

    /// <summary>
    /// Method name (e.g., "Standard", "Express", "Overnight").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Carrier name (e.g., "FedEx", "UPS", "DHL").
    /// </summary>
    public string Carrier { get; set; } = string.Empty;

    /// <summary>
    /// Base shipping cost for this method.
    /// </summary>
    public decimal BaseRate { get; set; }

    /// <summary>
    /// Estimated delivery days.
    /// </summary>
    public int EstimatedDays { get; set; }

    /// <summary>
    /// Whether method is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When method was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When method was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
