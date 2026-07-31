using KromicStore.Domain.Common;

namespace KromicStore.Domain.Media.Entities;

/// <summary>
/// Archive for deleted product images enabling restore capability.
/// Implements soft-delete pattern for media assets.
/// </summary>
public class ProductImageArchive : AuditableEntity, ITenantEntity
{
    private ProductImageArchive() { }

    private ProductImageArchive(Guid id) : base(id) { }

    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string PublicId { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string SecureUrl { get; private set; } = string.Empty;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string? Format { get; private set; }
    public long FileSizeBytes { get; private set; }
    public DateTime? RestoredOnUtc { get; private set; }

    public static ProductImageArchive CreateFromImage(
        Guid tenantId,
        Guid productId,
        string publicId,
        string url,
        string secureUrl,
        int width,
        int height,
        string? format,
        long fileSizeBytes,
        string deletedBy)
    {
        var archive = new ProductImageArchive(Guid.NewGuid())
        {
            TenantId = tenantId,
            ProductId = productId,
            PublicId = publicId,
            Url = url,
            SecureUrl = secureUrl,
            Width = width,
            Height = height,
            Format = format,
            FileSizeBytes = fileSizeBytes
        };
        
        archive.MarkCreated(DateTime.UtcNow, deletedBy);
        return archive;
    }

    public void MarkRestored()
    {
        RestoredOnUtc = DateTime.UtcNow;
        MarkModified(DateTime.UtcNow, "system");
    }
}
