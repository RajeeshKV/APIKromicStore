namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for inventory information.
/// </summary>
public sealed record InventoryDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; init; }

    /// <summary>Gets or sets the product SKU.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets or sets the quantity on hand.</summary>
    public int QuantityOnHand { get; init; }

    /// <summary>Gets or sets the reorder level.</summary>
    public int ReorderLevel { get; init; }

    /// <summary>Gets or sets the quantity reserved for orders.</summary>
    public int QuantityReserved { get; init; }

    /// <summary>Gets or sets the available quantity (QuantityOnHand - QuantityReserved).</summary>
    public int AvailableQuantity { get; init; }

    /// <summary>Gets or sets whether the product is in stock.</summary>
    public bool IsInStock { get; init; }

    /// <summary>Gets or sets whether the product is below reorder level.</summary>
    public bool IsBelowReorderLevel { get; init; }

    /// <summary>Gets or sets the last inventory adjustment timestamp.</summary>
    public DateTime? LastAdjustedAtUtc { get; init; }
}
