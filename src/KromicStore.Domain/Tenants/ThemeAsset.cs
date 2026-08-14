using KromicStore.Domain.Common;

namespace KromicStore.Domain.Tenants;

/// <summary>
/// Asset file associated with a theme (logos, hero banners, images, etc.)
/// Supports multipart file upload for theme customization.
/// </summary>
public sealed class ThemeAsset : AuditableEntity
{
    private ThemeAsset()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        StoragePath = string.Empty;
        AssetType = ThemeAssetType.Logo;
    }

    private ThemeAsset(Guid id, Guid themeId, string fileName, string contentType, string storagePath, ThemeAssetType assetType)
        : base(id)
    {
        ThemeId = themeId;
        FileName = fileName;
        ContentType = contentType;
        StoragePath = storagePath;
        AssetType = assetType;
        IsActive = true;
        Size = 0;
    }

    // Identity
    public Guid ThemeId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }

    // Storage
    public string StoragePath { get; private set; } // Full path to file in blob storage or file system
    public long Size { get; private set; } // File size in bytes

    // Classification
    public ThemeAssetType AssetType { get; private set; }
    public string? Description { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public string? PublicUrl { get; private set; } // CDN or public URL for the asset

    public static ThemeAsset Create(
        Guid themeId,
        string fileName,
        string contentType,
        string storagePath,
        ThemeAssetType assetType,
        long fileSize,
        string? description = null)
    {
        if (themeId == Guid.Empty)
            throw new ArgumentException("ThemeId is required.", nameof(themeId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName is required.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("ContentType is required.", nameof(contentType));
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("StoragePath is required.", nameof(storagePath));
        if (fileSize <= 0)
            throw new ArgumentException("FileSize must be greater than 0.", nameof(fileSize));

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        if (fileSize > maxFileSize)
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxFileSize} bytes.");

        var asset = new ThemeAsset(
            Guid.NewGuid(),
            themeId,
            fileName.Trim(),
            contentType.Trim(),
            storagePath.Trim(),
            assetType)
        {
            Size = fileSize,
            Description = description?.Trim()
        };

        return asset;
    }

    public void SetPublicUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Public URL cannot be empty.", nameof(url));
        PublicUrl = url.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}

/// <summary>
/// Type of asset stored for theme customization.
/// </summary>
public enum ThemeAssetType
{
    /// <summary>Store logo for header/navigation</summary>
    Logo = 0,

    /// <summary>Hero banner image for homepage</summary>
    HeroBanner = 1,

    /// <summary>Favicon for browser tab</summary>
    Favicon = 2,

    /// <summary>General image or graphic</summary>
    Image = 3,

    /// <summary>Icon or small graphic element</summary>
    Icon = 4,

    /// <summary>Background image for sections</summary>
    Background = 5,

    /// <summary>Other asset type</summary>
    Other = 6
}
