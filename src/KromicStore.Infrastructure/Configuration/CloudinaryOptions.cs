using System.ComponentModel.DataAnnotations;

namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Cloudinary media service configuration.
/// Strongly typed configuration with validation.
/// </summary>
public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    [Required(ErrorMessage = "Cloudinary cloud name is required")]
    public string CloudName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cloudinary API key is required")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cloudinary API secret is required")]
    public string ApiSecret { get; set; } = string.Empty;

    public string? ApiEnvironmentVariable { get; set; } = null;

    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024; // 50 MB

    public int RequestTimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    public int InitialRetryDelayMilliseconds { get; set; } = 1000;

    public double RetryBackoffMultiplier { get; set; } = 2.0;

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Allowed MIME types for upload.
    /// </summary>
    public List<string> AllowedMimeTypes { get; set; } = new()
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml",
        "application/pdf"
    };

    /// <summary>
    /// Allowed file extensions.
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new()
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".svg",
        ".pdf"
    };

    /// <summary>
    /// Base path for all tenant uploads in Cloudinary.
    /// </summary>
    public string TenantBasePath { get; set; } = "tenant";

    /// <summary>
    /// Enable image transformation (resize, compress, etc).
    /// </summary>
    public bool EnableTransformations { get; set; } = true;

    /// <summary>
    /// CDN delivery settings.
    /// </summary>
    public bool UseSecureUrl { get; set; } = true;

    public bool EnableWebp { get; set; } = true;

    public bool EnableAutoQuality { get; set; } = true;

    /// <summary>
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(CloudName))
            return (false, "Cloudinary cloud name is required");

        if (CloudName.Length < 3)
            return (false, "Cloudinary cloud name appears to be invalid (too short)");

        if (string.IsNullOrWhiteSpace(ApiKey))
            return (false, "Cloudinary API key is required");

        if (ApiKey.Length < 10)
            return (false, "Cloudinary API key appears to be invalid (too short)");

        if (string.IsNullOrWhiteSpace(ApiSecret))
            return (false, "Cloudinary API secret is required");

        if (ApiSecret.Length < 20)
            return (false, "Cloudinary API secret appears to be invalid (too short)");

        if (MaxFileSizeBytes <= 0)
            return (false, "Max file size must be greater than 0");

        if (RequestTimeoutSeconds <= 0)
            return (false, "Request timeout must be greater than 0");

        if (MaxRetries < 0)
            return (false, "Max retries cannot be negative");

        if (InitialRetryDelayMilliseconds <= 0)
            return (false, "Initial retry delay must be greater than 0");

        if (RetryBackoffMultiplier <= 1.0)
            return (false, "Retry backoff multiplier must be greater than 1.0");

        if (!AllowedMimeTypes.Any())
            return (false, "At least one MIME type must be allowed");

        if (!AllowedExtensions.Any())
            return (false, "At least one file extension must be allowed");

        if (string.IsNullOrWhiteSpace(TenantBasePath))
            return (false, "Tenant base path is required");

        return (true, null);
    }

    /// <summary>
    /// Build tenant-specific upload folder path.
    /// </summary>
    public string GetTenantUploadPath(Guid tenantId, string folder)
    {
        return $"{TenantBasePath}/{tenantId}/{folder}";
    }

    /// <summary>
    /// Check if file extension is allowed.
    /// </summary>
    public bool IsExtensionAllowed(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    /// <summary>
    /// Check if MIME type is allowed.
    /// </summary>
    public bool IsMimeTypeAllowed(string mimeType)
    {
        return AllowedMimeTypes.Contains(mimeType.ToLowerInvariant());
    }
}
