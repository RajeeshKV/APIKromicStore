using MediatR;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.UploadThemeAsset;

/// <summary>
/// Command to upload an asset file for a theme (logo, banner, etc.)
/// Supports multipart file upload for theme customization.
/// </summary>
public sealed record UploadThemeAssetCommand(
    Guid ThemeId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize,
    ThemeAssetType AssetType,
    string? Description = null
) : IRequest<UploadThemeAssetResult>;

/// <summary>
/// Result containing details of the uploaded theme asset.
/// </summary>
public sealed record UploadThemeAssetResult(
    Guid AssetId,
    string FileName,
    long FileSize,
    string ContentType,
    string? PublicUrl,
    ThemeAssetType AssetType
);
