namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for product summary (used in lists).
/// </summary>
public sealed record ProductCardDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the product description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the SKU.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets or sets the base price.</summary>
    public decimal BasePrice { get; init; }

    /// <summary>Gets or sets the currency code.</summary>
    public string CurrencyCode { get; init; } = "USD";

    /// <summary>Gets or sets whether the product is available.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Gets or sets the quantity on hand.</summary>
    public int QuantityOnHand { get; init; }

    /// <summary>Gets or sets the category ID.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Gets or sets the category name.</summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>Gets or sets the product tags.</summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }
}
