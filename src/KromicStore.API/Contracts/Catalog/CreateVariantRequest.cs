namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to create a product variant.
/// </summary>
public sealed record CreateVariantRequest
{
    /// <summary>Gets or sets the variant SKU.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets or sets the variant name/description.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the variant price (optional, uses base price if not provided).</summary>
    public decimal? Price { get; init; }

    /// <summary>Gets or sets the variant cost price.</summary>
    public decimal? CostPrice { get; init; }

    /// <summary>Gets or sets the variant attributes (e.g., Size: M, Color: Red).</summary>
    public Dictionary<string, string>? Attributes { get; init; }

    /// <summary>Gets or sets the quantity on hand for this variant.</summary>
    public int QuantityOnHand { get; init; }

    /// <summary>Gets or sets whether the variant is available.</summary>
    public bool IsAvailable { get; init; } = true;
}
