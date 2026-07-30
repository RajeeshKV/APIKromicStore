namespace KromicStore.Application.Features.Catalog.Abstractions;

public interface IMediaService
{
    Task<MediaUploadResult> UploadImageAsync(Stream fileStream, string fileName, Guid tenantId, string folder = "products", CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
    string GetImageUrl(string publicId, int? width = null, int? height = null, string? crop = null);
}

public sealed record MediaUploadResult(
    string Url,
    string PublicId,
    int Width,
    int Height,
    long FileSize);
