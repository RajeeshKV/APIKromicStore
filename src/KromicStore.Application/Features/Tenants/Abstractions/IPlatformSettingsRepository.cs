using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for PlatformSettings (singleton).
/// </summary>
public interface IPlatformSettingsRepository
{
    /// <summary>
    /// Get the singleton platform settings instance.
    /// </summary>
    Task<PlatformSettings?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create platform settings with defaults.
    /// </summary>
    Task<PlatformSettings> GetOrCreateAsync(
        string platformName,
        string supportEmail,
        CancellationToken cancellationToken = default);

    void Update(PlatformSettings settings);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
