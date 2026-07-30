namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to update an existing product variant.
/// </summary>
public sealed record UpdateVariantRequest
{
    /// <summary>Gets or sets the variant name/description.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the variant price.</summary>
    public decimal? Price { get; init; }

    /// <summary>Gets or sets the variant cost price.</summary>
    public decimal? CostPrice { get; init; }

    /// <summary>Gets or sets the variant attributes.</summary>
    public Dictionary<string, string>? Attributes { get; init; }

    /// <summary>Gets or sets whether the variant is available.</summary>
    public bool IsAvailable { get; init; }
}
