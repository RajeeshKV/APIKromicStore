namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Request to create a new product category.
/// </summary>
public sealed record CreateCategoryRequest
{
    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the category description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the parent category ID (optional for root categories).</summary>
    public Guid? ParentCategoryId { get; init; }

    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; init; }

    /// <summary>Gets or sets whether the category is active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Gets or sets the SEO metadata slug.</summary>
    public string? Slug { get; init; }
}
