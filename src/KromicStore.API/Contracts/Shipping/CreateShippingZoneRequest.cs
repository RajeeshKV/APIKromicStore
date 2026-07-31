namespace KromicStore.API.Contracts.Shipping;

/// <summary>
/// Request to create a new shipping zone.
/// </summary>
public class CreateShippingZoneRequest
{
    /// <summary>
    /// Zone name (e.g., "Domestic", "International").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zone description (optional).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether zone should be active upon creation.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
