using KromicStore.Domain.Common;

namespace KromicStore.Domain.CMS.Entities;

/// <summary>
/// Represents a content management system page.
/// Enables tenants to create and manage informational pages.
/// </summary>
public sealed class CMSPage : TenantEntity, IAuditable, ISoftDeletable
{
    private CMSPage() { }

    private CMSPage(Guid id, Guid tenantId) : base(id, tenantId) { }

    /// <summary>
    /// Gets the page title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the URL slug for the page.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the page content (HTML or rich text).
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the SEO meta description.
    /// </summary>
    public string? MetaDescription { get; private set; }

    /// <summary>
    /// Gets the SEO meta keywords.
    /// </summary>
    public string? MetaKeywords { get; private set; }

    /// <summary>
    /// Gets the page status.
    /// </summary>
    public CMSPageStatus Status { get; private set; } = CMSPageStatus.Draft;

    /// <summary>
    /// Gets the publication date.
    /// </summary>
    public DateTime? PublishedDateUtc { get; private set; }

    /// <summary>
    /// Gets the scheduled publish date (if any).
    /// </summary>
    public DateTime? ScheduledPublishDateUtc { get; private set; }

    /// <summary>
    /// Creates a new CMS page.
    /// </summary>
    public static CMSPage Create(
        Guid tenantId,
        string title,
        string slug,
        string content,
        string? metaDescription = null,
        string? metaKeywords = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        return new CMSPage(Guid.NewGuid(), tenantId)
        {
            Title = title,
            Slug = NormalizeSlug(slug),
            Content = content,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            Status = CMSPageStatus.Draft,
            PublishedDateUtc = null
        };
    }

    /// <summary>
    /// Updates the page content.
    /// </summary>
    public void Update(
        string title,
        string slug,
        string content,
        string? metaDescription = null,
        string? metaKeywords = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        Title = title;
        Slug = NormalizeSlug(slug);
        Content = content;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
    }

    /// <summary>
    /// Publishes the page immediately.
    /// </summary>
    public void Publish()
    {
        if (Status == CMSPageStatus.Deleted)
            throw new InvalidOperationException("Cannot publish a deleted page");

        Status = CMSPageStatus.Published;
        PublishedDateUtc = DateTime.UtcNow;
        ScheduledPublishDateUtc = null;
    }

    /// <summary>
    /// Unpublishes the page (returns to draft).
    /// </summary>
    public void Unpublish()
    {
        if (Status == CMSPageStatus.Deleted)
            throw new InvalidOperationException("Cannot unpublish a deleted page");

        Status = CMSPageStatus.Draft;
        PublishedDateUtc = null;
        ScheduledPublishDateUtc = null;
    }

    /// <summary>
    /// Schedules the page to be published at a future date.
    /// </summary>
    public void Schedule(DateTime publishDateUtc)
    {
        if (publishDateUtc <= DateTime.UtcNow)
            throw new ArgumentException("Publish date must be in the future", nameof(publishDateUtc));

        if (Status == CMSPageStatus.Deleted)
            throw new InvalidOperationException("Cannot schedule a deleted page");

        Status = CMSPageStatus.Scheduled;
        ScheduledPublishDateUtc = publishDateUtc;
        PublishedDateUtc = null;
    }

    /// <summary>
    /// Soft-deletes the page.
    /// </summary>
    public void SoftDelete()
    {
        Status = CMSPageStatus.Deleted;
    }

    /// <summary>
    /// Restores a soft-deleted page.
    /// </summary>
    public void Restore()
    {
        Status = CMSPageStatus.Draft;
    }

    /// <summary>
    /// Normalizes a URL slug.
    /// </summary>
    private static string NormalizeSlug(string slug)
    {
        return slug.ToLower().Trim().Replace(" ", "-");
    }
}

/// <summary>
/// CMS page status enumeration.
/// </summary>
public enum CMSPageStatus
{
    /// <summary>Draft - not yet published.</summary>
    Draft = 0,

    /// <summary>Published - currently visible.</summary>
    Published = 1,

    /// <summary>Scheduled - will be published at a future date.</summary>
    Scheduled = 2,

    /// <summary>Deleted - soft-deleted.</summary>
    Deleted = 3
}
