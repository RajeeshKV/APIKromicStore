namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to duplicate an existing product.
/// </summary>
public sealed record DuplicateProductRequest
{
    /// <summary>Gets or sets the new product name (optional, uses original if not provided).</summary>
    public string? NewName { get; init; }

    /// <summary>Gets or sets the new product SKU (optional, generates new if not provided).</summary>
    public string? NewSku { get; init; }

    /// <summary>Gets or sets whether to duplicate variants.</summary>
    public bool IncludeVariants { get; init; } = true;

    /// <summary>Gets or sets whether to duplicate images.</summary>
    public bool IncludeImages { get; init; } = true;

    /// <summary>Gets or sets whether to duplicate attributes.</summary>
    public bool IncludeAttributes { get; init; } = true;
}
