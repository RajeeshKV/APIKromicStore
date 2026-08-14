using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Tenants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.UploadThemeAsset;

/// <summary>
/// Handles upload of asset files for themes.
/// Validates file, stores it, and records the asset in the database.
/// </summary>
public sealed class UploadThemeAssetCommandHandler
    : IRequestHandler<UploadThemeAssetCommand, UploadThemeAssetResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<UploadThemeAssetCommandHandler> _logger;

    // Allowed content types for theme assets
    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml",
        "image/x-icon"
    };

    public UploadThemeAssetCommandHandler(
        IApplicationDbContext db,
        ILogger<UploadThemeAssetCommandHandler> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UploadThemeAssetResult> Handle(
        UploadThemeAssetCommand request,
        CancellationToken cancellationToken)
    {
        // Validate theme exists
        var theme = await _db.Themes
            .FirstOrDefaultAsync(t => t.Id == request.ThemeId, cancellationToken);

        if (theme == null)
            throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");

        // Validate file
        if (request.FileStream == null || request.FileSize == 0)
            throw new ArgumentException("File cannot be empty.", nameof(request.FileStream));

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new InvalidOperationException($"File type '{request.ContentType}' is not allowed. " +
                $"Allowed types: {string.Join(", ", AllowedContentTypes)}");

        const long maxFileSize = 10 * 1024 * 1024; // 10 MB
        if (request.FileSize > maxFileSize)
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxFileSize} bytes.");

        // Generate storage path (typically to blob storage or file system)
        // Format: themes/{themeId}/{assetType}/{fileName}
        var fileExtension = Path.GetExtension(request.FileName);
        var storagePath = $"themes/{request.ThemeId}/{request.AssetType}/{Guid.NewGuid()}{fileExtension}";

        _logger.LogInformation("Uploading theme asset: {FileName}, Type: {AssetType}, Size: {Size} bytes",
            request.FileName, request.AssetType, request.FileSize);

        // Create asset entity
        var asset = ThemeAsset.Create(
            themeId: request.ThemeId,
            fileName: request.FileName,
            contentType: request.ContentType,
            storagePath: storagePath,
            assetType: request.AssetType,
            fileSize: request.FileSize,
            description: request.Description
        );

        // In a real implementation, upload file to blob storage (e.g., AWS S3, Azure Blob Storage)
        // For now, set a placeholder public URL
        var publicUrl = $"/api/v1/themes/assets/{asset.Id}/download";
        asset.SetPublicUrl(publicUrl);

        // Save to database
        _db.AddEntity(asset);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Theme asset uploaded successfully: {AssetId}, URL: {PublicUrl}",
            asset.Id, publicUrl);

        return new UploadThemeAssetResult(
            AssetId: asset.Id,
            FileName: request.FileName,
            FileSize: request.FileSize,
            ContentType: request.ContentType,
            PublicUrl: publicUrl,
            AssetType: request.AssetType
        );
    }
}
