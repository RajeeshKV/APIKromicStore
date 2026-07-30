namespace KromicStore.API.Contracts.Catalog;

/// <summary>
/// Data transfer object for product image information.
/// </summary>
public sealed record ProductImageDto
{
    /// <summary>Gets or sets the image ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the image URL.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets or sets the image alt text for accessibility.</summary>
    public string? AltText { get; init; }

    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; init; }

    /// <summary>Gets or sets whether this is the primary image.</summary>
    public bool IsPrimary { get; init; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }
}
