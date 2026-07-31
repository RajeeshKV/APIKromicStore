using KromicStore.Domain.Common;

namespace KromicStore.Domain.Media.Entities;

/// <summary>
/// Represents a media asset (image, document) in Cloudinary.
/// Tenant-scoped with full lifecycle management.
/// </summary>
public class MediaAsset : TenantEntity, IAuditable, ISoftDeletable
{
    public string FileName { get; private set; } = string.Empty;
    public string CloudinaryPublicId { get; private set; } = string.Empty;
    public string CloudinaryUrl { get; private set; } = string.Empty;
    public string CloudinarySecureUrl { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public MediaAssetFolder Folder { get; private set; } = MediaAssetFolder.Products;
    public string? Description { get; private set; }
    public Dictionary<string, string>? Metadata { get; private set; }
    public DateTime UploadedOnUtc { get; private set; }
    public string UploadedBy { get; private set; } = string.Empty;
    public string? AccessedBy { get; private set; }
    public DateTime? LastAccessedOnUtc { get; private set; }

    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? ModifiedOnUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private MediaAsset() { }

    private MediaAsset(Guid id, Guid tenantId) : base(id, tenantId) { }

    /// <summary>
    /// Creates a new media asset after successful upload to Cloudinary.
    /// </summary>
    public static MediaAsset Create(
        Guid tenantId,
        string fileName,
        string cloudinaryPublicId,
        string cloudinaryUrl,
        string cloudinarySecureUrl,
        string mimeType,
        long fileSizeBytes,
        int width,
        int height,
        MediaAssetFolder folder,
        string uploadedBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(cloudinaryPublicId))
            throw new ArgumentException("Cloudinary public ID is required.", nameof(cloudinaryPublicId));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type is required.", nameof(mimeType));

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be greater than 0.", nameof(fileSizeBytes));

        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploaded by is required.", nameof(uploadedBy));

        var asset = new MediaAsset(Guid.NewGuid(), tenantId)
        {
            FileName = fileName,
            CloudinaryPublicId = cloudinaryPublicId,
            CloudinaryUrl = cloudinaryUrl,
            CloudinarySecureUrl = cloudinarySecureUrl,
            MimeType = mimeType,
            FileSizeBytes = fileSizeBytes,
            Width = width,
            Height = height,
            Folder = folder,
            Description = description,
            UploadedOnUtc = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            Metadata = new Dictionary<string, string>()
        };

        asset.MarkCreated(DateTime.UtcNow, uploadedBy);
        return asset;
    }

    /// <summary>
    /// Records access to the asset for usage tracking.
    /// </summary>
    public void RecordAccess(string accessedBy)
    {
        LastAccessedOnUtc = DateTime.UtcNow;
        AccessedBy = accessedBy;
        MarkModified(DateTime.UtcNow, accessedBy);
    }

    /// <summary>
    /// Updates asset metadata (description, custom fields).
    /// </summary>
    public void UpdateMetadata(string? description, Dictionary<string, string>? customMetadata, string actor)
    {
        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (customMetadata != null)
            Metadata = customMetadata;

        MarkModified(DateTime.UtcNow, actor);
    }

    /// <summary>
    /// Soft delete the asset (Cloudinary asset remains).
    /// </summary>
    public void SoftDelete(string actor)
    {
        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;
        DeletedBy = actor;
        MarkModified(DateTime.UtcNow, actor);
    }

    /// <summary>
    /// Restore a soft-deleted asset.
    /// </summary>
    public void Restore(string actor)
    {
        IsDeleted = false;
        DeletedOnUtc = null;
        DeletedBy = null;
        MarkModified(DateTime.UtcNow, actor);
    }
}

/// <summary>
/// Media asset folder enumeration for tenant organization.
/// </summary>
public enum MediaAssetFolder
{
    Products = 1,
    Categories = 2,
    Logos = 3,
    Banners = 4,
    Customers = 5,
    Themes = 6,
    Documents = 7,
    Other = 8
}
