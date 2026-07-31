namespace KromicStore.API.Contracts.CMS;

/// <summary>
/// Request to create a new CMS page.
/// </summary>
public class CreatePageRequest
{
    /// <summary>
    /// Page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Page slug for URL.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Page content.
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
    /// Whether to publish immediately.
    /// </summary>
    public bool Publish { get; set; } = false;
}
