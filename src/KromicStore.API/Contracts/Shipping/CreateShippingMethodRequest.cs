namespace KromicStore.API.Contracts.Shipping;

/// <summary>
/// Request to create a new shipping method within a zone.
/// </summary>
public class CreateShippingMethodRequest
{
    /// <summary>
    /// Method name (e.g., "Standard", "Express").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Carrier name (e.g., "FedEx", "UPS").
    /// </summary>
    public string Carrier { get; set; } = string.Empty;

    /// <summary>
    /// Base shipping cost.
    /// </summary>
    public decimal BaseRate { get; set; }

    /// <summary>
    /// Estimated delivery days.
    /// </summary>
    public int EstimatedDays { get; set; }

    /// <summary>
    /// Whether method should be active upon creation.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
