namespace KromicStore.API.Contracts.Shipping;

/// <summary>
/// DTO representing a shipping zone.
/// </summary>
public class ShippingZoneDto
{
    /// <summary>
    /// Unique identifier for the shipping zone.
    /// </summary>
    public Guid ZoneId { get; set; }

    /// <summary>
    /// Zone name (e.g., "Domestic", "International", "Europe").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zone description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether zone is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When zone was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When zone was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
