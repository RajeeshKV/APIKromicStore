using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// ProductTag entity representing marketing tags for products.
/// Examples: New, Trending, Organic, Handmade, Sale, etc.
/// Used for filtering, categorization, and product discovery.
/// </summary>
public sealed class ProductTag : BaseEntity, ISoftDeletable
{
    public Guid ProductId { get; private set; }
    public string Tag { get; private set; } = string.Empty;

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ProductTag()
    {
    }

    private ProductTag(Guid id) : base(id)
    {
    }

    public static ProductTag Create(Guid productId, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty", nameof(tag));

        var normalized = tag.Trim().ToLowerInvariant();

        if (normalized.Length > 50)
            throw new ArgumentException("Tag cannot exceed 50 characters", nameof(tag));

        var productTag = new ProductTag(Guid.NewGuid())
        {
            ProductId = productId,
            Tag = normalized
        };

        return productTag;
    }

    public void SoftDelete(DateTime utcNow, string actor)
    {
        IsDeleted = true;
        DeletedOnUtc = utcNow;
        DeletedBy = actor;
    }
}
