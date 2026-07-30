namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for product search result.
/// </summary>
public sealed record ProductSearchResultDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the product description excerpt.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the SKU.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets or sets the base price.</summary>
    public decimal BasePrice { get; init; }

    /// <summary>Gets or sets the currency code.</summary>
    public string CurrencyCode { get; init; } = "USD";

    /// <summary>Gets or sets the category name.</summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>Gets or sets the product tags.</summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>Gets or sets whether the product is available.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Gets or sets the search relevance score (0-1).</summary>
    public float RelevanceScore { get; init; }
}
