namespace KromicStore.API.Contracts.CMS;

/// <summary>
/// DTO representing a CMS page.
/// </summary>
public class PageDto
{
    /// <summary>
    /// Unique page identifier.
    /// </summary>
    public Guid PageId { get; set; }

    /// <summary>
    /// Page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Page slug for URL (e.g., "about-us").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Page content (HTML or rich text).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Meta description for SEO.
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Meta keywords for SEO.
    /// </summary>
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Whether page is published and visible.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Publish date (nullable for draft pages).
    /// </summary>
    public DateTime? PublishDate { get; set; }

    /// <summary>
    /// When page was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When page was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
