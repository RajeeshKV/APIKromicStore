namespace KromicStore.API.Contracts.Shipping;

/// <summary>
/// Request to update an existing shipping method.
/// </summary>
public class UpdateShippingMethodRequest
{
    /// <summary>
    /// Method name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Carrier name.
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
    /// Whether method is active.
    /// </summary>
    public bool IsActive { get; set; }
}
