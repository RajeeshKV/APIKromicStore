using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Platform settings repository for global configuration management.
/// Stores platform-wide settings used by all tenants.
/// </summary>
public sealed class PlatformSettingsRepository : IPlatformSettingsRepository
{
    private readonly IApplicationDbContext _dbContext;

    public PlatformSettingsRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PlatformSettings?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PlatformSettings
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PlatformSettings> GetOrCreateAsync(
        string platformName,
        string supportEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(platformName, nameof(platformName));
        ArgumentException.ThrowIfNullOrWhiteSpace(supportEmail, nameof(supportEmail));

        var existing = await GetAsync(cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var newSettings = PlatformSettings.Create(platformName, supportEmail);
        _dbContext.AddEntity(newSettings);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newSettings;
    }

    public void Update(PlatformSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        // Update is handled by EF Core tracking
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
