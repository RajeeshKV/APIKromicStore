namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to update an existing product collection.
/// </summary>
public sealed record UpdateCollectionRequest
{
    /// <summary>Gets or sets the collection name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the collection description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the SEO slug.</summary>
    public string? Slug { get; init; }

    /// <summary>Gets or sets whether the collection is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets or sets the product IDs to replace in the collection.</summary>
    public List<Guid>? ProductIds { get; init; }
}
