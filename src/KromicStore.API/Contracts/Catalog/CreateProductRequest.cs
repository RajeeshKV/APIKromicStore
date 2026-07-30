namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to create a new product.
/// </summary>
public sealed record CreateProductRequest
{
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the product description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the category ID.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Gets or sets the SKU (stock keeping unit).</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets or sets the base price in the store's currency.</summary>
    public decimal BasePrice { get; init; }

    /// <summary>Gets or sets the currency code (e.g., USD, EUR).</summary>
    public string CurrencyCode { get; init; } = "USD";

    /// <summary>Gets or sets the cost price for accounting.</summary>
    public decimal? CostPrice { get; init; }

    /// <summary>Gets or sets the quantity on hand.</summary>
    public int QuantityOnHand { get; init; }

    /// <summary>Gets or sets the reorder level.</summary>
    public int ReorderLevel { get; init; }

    /// <summary>Gets or sets whether the product is available for purchase.</summary>
    public bool IsAvailable { get; init; } = true;

    /// <summary>Gets or sets the product attributes (key-value pairs).</summary>
    public Dictionary<string, string>? Attributes { get; init; }

    /// <summary>Gets or sets the product tags.</summary>
    public List<string>? Tags { get; init; }

    /// <summary>Gets or sets the SEO slug.</summary>
    public string? Slug { get; init; }

    /// <summary>Gets or sets the meta description for SEO.</summary>
    public string? MetaDescription { get; init; }
}
