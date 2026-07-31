namespace KromicStore.API.Contracts.Shipping;

/// <summary>
/// Request to update an existing shipping zone.
/// </summary>
public class UpdateShippingZoneRequest
{
    /// <summary>
    /// Zone name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zone description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether zone is active.
    /// </summary>
    public bool IsActive { get; set; }
}
