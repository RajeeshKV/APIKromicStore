namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for category information.
/// </summary>
public sealed record CategoryDto
{
    /// <summary>Gets or sets the category ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the category description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the parent category ID.</summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; init; }

    /// <summary>Gets or sets whether the category is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets or sets the SEO slug.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Gets or sets the product count in this category.</summary>
    public int ProductCount { get; init; }

    /// <summary>Gets or sets the creation timestamp (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Gets or sets the last modification timestamp (UTC).</summary>
    public DateTime? ModifiedAtUtc { get; init; }
}
