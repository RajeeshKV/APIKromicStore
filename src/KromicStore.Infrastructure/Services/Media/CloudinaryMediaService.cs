using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;
using KromicStore.Domain.Media.Entities;
using KromicStore.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KromicStore.Infrastructure.Services.Media;

/// <summary>
/// Cloudinary media service implementation.
/// Provides image upload, management, and CDN URL generation with tenant isolation.
/// </summary>
public class CloudinaryMediaService : IMediaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CloudinaryOptions _options;
    private readonly ILogger<CloudinaryMediaService> _logger;

    public CloudinaryMediaService(
        IHttpClientFactory httpClientFactory,
        IOptions<CloudinaryOptions> options,
        ILogger<CloudinaryMediaService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaUploadResult> UploadAsync(
        Guid tenantId,
        Stream fileStream,
        string fileName,
        string mimeType,
        MediaAssetFolder folder,
        string uploadedBy,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type is required.", nameof(mimeType));

        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploaded by is required.", nameof(uploadedBy));

        try
        {
            // Validate file
            if (fileStream.Length > _options.MaxFileSizeBytes)
            {
                _logger.LogWarning(
                    "File {FileName} exceeds max size limit. Size: {FileSize}, Limit: {MaxSize}",
                    fileName, fileStream.Length, _options.MaxFileSizeBytes);

                return new MediaUploadResult
                {
                    Success = false,
                    ErrorCode = "FILE_TOO_LARGE",
                    ErrorMessage = $"File size exceeds limit of {_options.MaxFileSizeBytes} bytes"
                };
            }

            if (!_options.IsMimeTypeAllowed(mimeType))
            {
                _logger.LogWarning(
                    "MIME type {MimeType} is not allowed for upload",
                    mimeType);

                return new MediaUploadResult
                {
                    Success = false,
                    ErrorCode = "INVALID_MIME_TYPE",
                    ErrorMessage = $"MIME type {mimeType} is not allowed"
                };
            }

            if (!_options.IsExtensionAllowed(fileName))
            {
                _logger.LogWarning(
                    "File extension for {FileName} is not allowed",
                    fileName);

                return new MediaUploadResult
                {
                    Success = false,
                    ErrorCode = "INVALID_EXTENSION",
                    ErrorMessage = $"File extension is not allowed"
                };
            }

            _logger.LogInformation(
                "Uploading file {FileName} to Cloudinary for tenant {TenantId} in folder {Folder}",
                fileName, tenantId, folder);

            var folderPath = _options.GetTenantUploadPath(tenantId, folder.ToString().ToLowerInvariant());

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);
            content.Add(new StringContent(folderPath), "folder");
            content.Add(new StringContent("auto"), "resource_type");
            
            if (metadata != null)
            {
                foreach (var (key, value) in metadata)
                {
                    content.Add(new StringContent(value), $"metadata[{key}]");
                }
            }

            var client = _httpClientFactory.CreateClient("Cloudinary");
            var response = await client.PostAsync(
                $"https://api.cloudinary.com/v1_1/{_options.CloudName}/auto/upload",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Cloudinary upload failed for {FileName}. Status: {StatusCode}, Error: {Error}",
                    fileName, response.StatusCode, errorContent);

                return new MediaUploadResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to upload file to Cloudinary"
                };
            }

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var uploadResponse = JsonSerializer.Deserialize<CloudinaryUploadResponse>(responseString);

            _logger.LogInformation(
                "File {FileName} uploaded successfully to Cloudinary. PublicId: {PublicId}",
                fileName, uploadResponse?.PublicId);

            return new MediaUploadResult
            {
                Success = true,
                PublicId = uploadResponse?.PublicId,
                Url = uploadResponse?.Url,
                SecureUrl = uploadResponse?.SecureUrl,
                Width = uploadResponse?.Width ?? 0,
                Height = uploadResponse?.Height ?? 0,
                FileSizeBytes = fileStream.Length,
                Format = uploadResponse?.Format
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while uploading {FileName} to Cloudinary",
                fileName);

            return new MediaUploadResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<MediaUploadResult> ReplaceAsync(
        Guid tenantId,
        string publicId,
        Stream fileStream,
        string fileName,
        string mimeType,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("Public ID is required.", nameof(publicId));

        _logger.LogInformation(
            "Replacing media {PublicId} for tenant {TenantId}",
            publicId, tenantId);

        // Delete old file
        await DeleteAsync(tenantId, publicId, updatedBy, cancellationToken);

        // Upload new file
        var folder = ExtractFolderFromPublicId(publicId);
        return await UploadAsync(
            tenantId,
            fileStream,
            fileName,
            mimeType,
            Enum.Parse<MediaAssetFolder>(folder, ignoreCase: true),
            updatedBy,
            cancellationToken: cancellationToken);
    }

    public async Task<MediaDeleteResult> DeleteAsync(
        Guid tenantId,
        string publicId,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("Public ID is required.", nameof(publicId));

        try
        {
            _logger.LogInformation(
                "Deleting media {PublicId} from Cloudinary for tenant {TenantId}",
                publicId, tenantId);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(publicId), "public_id");

            var client = _httpClientFactory.CreateClient("Cloudinary");
            var response = await client.PostAsync(
                $"https://api.cloudinary.com/v1_1/{_options.CloudName}/destroy",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Cloudinary delete failed for {PublicId}. Status: {StatusCode}",
                    publicId, response.StatusCode);

                return new MediaDeleteResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to delete file from Cloudinary"
                };
            }

            _logger.LogInformation("Media {PublicId} deleted successfully from Cloudinary", publicId);

            return new MediaDeleteResult
            {
                Success = true,
                PublicId = publicId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while deleting {PublicId} from Cloudinary",
                publicId);

            return new MediaDeleteResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public string GenerateUrl(string publicId, MediaTransformation? transformation = null)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("Public ID is required.", nameof(publicId));

        var baseUrl = $"https://res.cloudinary.com/{_options.CloudName}/image/upload";

        if (transformation == null)
            return $"{baseUrl}/{publicId}";

        var transformationString = BuildTransformationString(transformation);
        return $"{baseUrl}/{transformationString}/{publicId}";
    }

    public string GenerateSecureUrl(string publicId, MediaTransformation? transformation = null)
    {
        var url = GenerateUrl(publicId, transformation);
        return url.Replace("http://", "https://");
    }

    public async Task<MediaMetadata?> GetMetadataAsync(
        Guid tenantId,
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("Public ID is required.", nameof(publicId));

        try
        {
            var client = _httpClientFactory.CreateClient("Cloudinary");
            var response = await client.GetAsync(
                $"https://api.cloudinary.com/v1_1/{_options.CloudName}/resources/image/{publicId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var metadataString = await response.Content.ReadAsStringAsync(cancellationToken);
            var metadata = JsonSerializer.Deserialize<CloudinaryMetadataResponse>(metadataString);

            return new MediaMetadata
            {
                PublicId = metadata?.PublicId ?? publicId,
                Format = metadata?.Format ?? string.Empty,
                FileSizeBytes = metadata?.Bytes ?? 0,
                Width = metadata?.Width ?? 0,
                Height = metadata?.Height ?? 0,
                MimeType = GetMimeTypeFromFormat(metadata?.Format),
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while getting metadata for {PublicId}",
                publicId);
            return null;
        }
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Cloudinary");
            var response = await client.GetAsync(
                $"https://api.cloudinary.com/v1_1/{_options.CloudName}/resources/image",
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary health check failed");
            return false;
        }
    }

    private string BuildTransformationString(MediaTransformation transformation)
    {
        var parts = new List<string>();

        if (transformation.Width.HasValue || transformation.Height.HasValue)
        {
            var w = transformation.Width?.ToString() ?? "auto";
            var h = transformation.Height?.ToString() ?? "auto";
            var c = transformation.CropMode ?? "auto";
            var g = transformation.Gravity ?? "auto";
            parts.Add($"w_{w},h_{h},c_{c},g_{g}");
        }

        if (!string.IsNullOrWhiteSpace(transformation.Quality))
            parts.Add($"q_{transformation.Quality}");

        if (transformation.Radius.HasValue)
            parts.Add($"r_{transformation.Radius}");

        if (!string.IsNullOrWhiteSpace(transformation.Format))
            parts.Add($"f_{transformation.Format}");

        if (transformation.EnableWebp)
            parts.Add("f_auto");

        return string.Join("/", parts);
    }

    private string ExtractFolderFromPublicId(string publicId)
    {
        var parts = publicId.Split('/');
        return parts.Length > 2 ? parts[2] : MediaAssetFolder.Products.ToString();
    }

    private string GetMimeTypeFromFormat(string? format) => format?.ToLowerInvariant() switch
    {
        "jpg" or "jpeg" => "image/jpeg",
        "png" => "image/png",
        "gif" => "image/gif",
        "webp" => "image/webp",
        "svg" => "image/svg+xml",
        "pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}

/// <summary>
/// Cloudinary upload API response.
/// </summary>
internal class CloudinaryUploadResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("public_id")]
    public string? PublicId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("url")]
    public string? Url { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("secure_url")]
    public string? SecureUrl { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("width")]
    public int Width { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("height")]
    public int Height { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("format")]
    public string? Format { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("bytes")]
    public long Bytes { get; set; }
}

/// <summary>
/// Cloudinary metadata API response.
/// </summary>
internal class CloudinaryMetadataResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("public_id")]
    public string? PublicId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("format")]
    public string? Format { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("width")]
    public int Width { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("height")]
    public int Height { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
