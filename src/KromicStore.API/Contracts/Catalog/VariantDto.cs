namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for product variant information.
/// </summary>
public sealed record VariantDto
{
    /// <summary>Gets or sets the variant ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the variant SKU.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets or sets the variant name/description.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the variant price (if different from base).</summary>
    public decimal? Price { get; init; }

    /// <summary>Gets or sets the variant cost price.</summary>
    public decimal? CostPrice { get; init; }

    /// <summary>Gets or sets the variant attributes (e.g., Size: M, Color: Red).</summary>
    public Dictionary<string, string> Attributes { get; init; } = [];

    /// <summary>Gets or sets the quantity on hand for this variant.</summary>
    public int QuantityOnHand { get; init; }

    /// <summary>Gets or sets whether the variant is available.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }
}
