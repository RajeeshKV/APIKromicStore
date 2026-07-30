namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to update an existing product.
/// </summary>
public sealed record UpdateProductRequest
{
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the product description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the category ID.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Gets or sets the base price.</summary>
    public decimal BasePrice { get; init; }

    /// <summary>Gets or sets the cost price.</summary>
    public decimal? CostPrice { get; init; }

    /// <summary>Gets or sets the reorder level.</summary>
    public int ReorderLevel { get; init; }

    /// <summary>Gets or sets whether the product is available for purchase.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Gets or sets the product attributes.</summary>
    public Dictionary<string, string>? Attributes { get; init; }

    /// <summary>Gets or sets the product tags.</summary>
    public List<string>? Tags { get; init; }

    /// <summary>Gets or sets the SEO slug.</summary>
    public string? Slug { get; init; }

    /// <summary>Gets or sets the meta description.</summary>
    public string? MetaDescription { get; init; }
}
