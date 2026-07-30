namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to create a new product collection.
/// </summary>
public sealed record CreateCollectionRequest
{
    /// <summary>Gets or sets the collection name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the collection description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the SEO slug (optional, auto-generated if not provided).</summary>
    public string? Slug { get; init; }

    /// <summary>Gets or sets whether the collection is active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Gets or sets the product IDs to add to the collection.</summary>
    public List<Guid>? ProductIds { get; init; }
}
