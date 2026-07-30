namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for collection information.
/// </summary>
public sealed record CollectionDto
{
    /// <summary>Gets or sets the collection ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the collection name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the collection description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the SEO slug.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Gets or sets whether the collection is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets or sets the number of products in this collection.</summary>
    public int ProductCount { get; init; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }
}
