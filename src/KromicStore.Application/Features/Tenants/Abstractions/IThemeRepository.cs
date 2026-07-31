using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for Theme aggregate.
/// </summary>
public interface IThemeRepository
{
    Task<Theme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Theme?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Theme>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Theme>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<(List<Theme> Themes, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        ThemeStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<List<Theme>> GetMostUsedAsync(int limit = 10, CancellationToken cancellationToken = default);
    void Add(Theme theme);
    void Update(Theme theme);
    void Remove(Theme theme);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
