namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to adjust product inventory.
/// </summary>
public sealed record AdjustInventoryRequest
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; init; }

    /// <summary>Gets or sets the adjustment quantity (positive for increase, negative for decrease).</summary>
    public int AdjustmentQuantity { get; init; }

    /// <summary>Gets or sets the reason for adjustment.</summary>
    public string? Reason { get; init; }

    /// <summary>Gets or sets optional notes about the adjustment.</summary>
    public string? Notes { get; init; }
}
