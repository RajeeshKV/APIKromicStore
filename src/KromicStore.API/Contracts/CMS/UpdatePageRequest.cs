namespace KromicStore.API.Contracts.CMS;

/// <summary>
/// Request to update an existing CMS page.
/// </summary>
public class UpdatePageRequest
{
    /// <summary>
    /// Page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

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
    /// Whether page is published.
    /// </summary>
    public bool IsPublished { get; set; }
}
