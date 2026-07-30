using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// ProductImage entity representing a product image stored in Cloudinary.
/// Multiple images per product supported, exactly one primary image.
/// </summary>
public sealed class ProductImage : BaseEntity, ISoftDeletable
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string PublicId { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ProductImage()
    {
    }

    private ProductImage(Guid id) : base(id)
    {
    }

    public static ProductImage Create(
        Guid productId,
        string url,
        string? altText = null,
        int displayOrder = 0,
        bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        var image = new ProductImage(Guid.NewGuid())
        {
            ProductId = productId,
            Url = url.Trim(),
            PublicId = ExtractPublicId(url),
            AltText = altText?.Trim(),
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary
        };

        return image;
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    public void UpdateMetadata(string? altText = null, int? displayOrder = null, bool? isPrimary = null)
    {
        if (altText is not null)
            AltText = altText.Trim();

        if (displayOrder.HasValue && displayOrder >= 0)
            DisplayOrder = displayOrder.Value;

        if (isPrimary.HasValue)
            IsPrimary = isPrimary.Value;
    }

    public void SoftDelete(DateTime utcNow, string actor)
    {
        IsDeleted = true;
        DeletedOnUtc = utcNow;
        DeletedBy = actor;
    }

    private static string ExtractPublicId(string url)
    {
        // Extract public_id from Cloudinary URL
        // Example: https://res.cloudinary.com/account/image/upload/v1234567890/tenant-id/products/image.jpg
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Return the last part (filename with extension)
            return parts.Length > 0 ? parts[^1] : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
