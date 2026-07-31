using KromicStore.Domain.Media.Entities;

namespace KromicStore.Infrastructure.Services.Media;

/// <summary>
/// Abstraction for media/file storage service providers (Cloudinary, S3, Azure Blob, etc.).
/// Enables vendor-agnostic media management with tenant isolation.
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Upload a media file to storage.
    /// </summary>
    Task<MediaUploadResult> UploadAsync(
        Guid tenantId,
        Stream fileStream,
        string fileName,
        string mimeType,
        MediaAssetFolder folder,
        string uploadedBy,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replace an existing media file.
    /// </summary>
    Task<MediaUploadResult> ReplaceAsync(
        Guid tenantId,
        string publicId,
        Stream fileStream,
        string fileName,
        string mimeType,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a media file.
    /// </summary>
    Task<MediaDeleteResult> DeleteAsync(
        Guid tenantId,
        string publicId,
        string deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate CDN URL for media with optional transformations.
    /// </summary>
    string GenerateUrl(string publicId, MediaTransformation? transformation = null);

    /// <summary>
    /// Generate secure (HTTPS) CDN URL for media.
    /// </summary>
    string GenerateSecureUrl(string publicId, MediaTransformation? transformation = null);

    /// <summary>
    /// Get media metadata (size, dimensions, format).
    /// </summary>
    Task<MediaMetadata?> GetMetadataAsync(
        Guid tenantId,
        string publicId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Health check - verify service connectivity.
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Media upload result.
/// </summary>
public class MediaUploadResult
{
    public bool Success { get; set; }
    public string? PublicId { get; set; }
    public string? SecureUrl { get; set; }
    public string? Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Format { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Media deletion result.
    /// </summary>
public class MediaDeleteResult
{
    public bool Success { get; set; }
    public string? PublicId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeletedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Media asset metadata.
/// </summary>
public class MediaMetadata
{
    public string PublicId { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
}

/// <summary>
/// Media transformation options for URL generation.
/// </summary>
public class MediaTransformation
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? CropMode { get; set; } = "auto";
    public string? Gravity { get; set; } = "auto";
    public string? Quality { get; set; } = "auto";
    public string? Format { get; set; }
    public int? Radius { get; set; }
    public bool EnableWebp { get; set; } = true;

    /// <summary>
    /// Generate thumbnail transformation (150x150).
    /// </summary>
    public static MediaTransformation Thumbnail => new()
    {
        Width = 150,
        Height = 150,
        CropMode = "fill"
    };

    /// <summary>
    /// Generate preview transformation (400x400).
    /// </summary>
    public static MediaTransformation Preview => new()
    {
        Width = 400,
        Height = 400,
        CropMode = "fit"
    };

    /// <summary>
    /// Generate display transformation (800x600).
    /// </summary>
    public static MediaTransformation Display => new()
    {
        Width = 800,
        Height = 600,
        CropMode = "fit"
    };
}
