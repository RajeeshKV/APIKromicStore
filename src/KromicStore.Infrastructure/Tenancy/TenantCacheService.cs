using KromicStore.Domain.Tenants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Tenancy;

/// <summary>
/// Caches tenant lookups with TTL and invalidation support.
/// Reduces database hits for tenant resolution operations.
/// </summary>
public sealed class TenantCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantCacheService> _logger;
    private const string SubdomainKeyPrefix = "tenant:subdomain:";
    private const string CustomDomainKeyPrefix = "tenant:domain:";
    private const int CacheDurationMinutes = 5;

    public TenantCacheService(IMemoryCache cache, ILogger<TenantCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a tenant from cache, or stores it if not found.
    /// </summary>
    public Tenant? GetOrAdd(string cacheKey, Func<Tenant?> factory)
    {
        if (_cache.TryGetValue(cacheKey, out Tenant? cached))
        {
            _logger.LogDebug("Tenant cache hit: {CacheKey}", cacheKey);
            return cached;
        }

        var tenant = factory();
        if (tenant != null)
        {
            _cache.Set(cacheKey, tenant, TimeSpan.FromMinutes(CacheDurationMinutes));
            _logger.LogDebug("Tenant cached: {CacheKey} Expiry={Minutes}m", cacheKey, CacheDurationMinutes);
        }

        return tenant;
    }

    /// <summary>
    /// Gets a tenant from cache, or stores it if not found (async version).
    /// </summary>
    public async Task<Tenant?> GetOrAddAsync(string cacheKey, Func<Task<Tenant?>> factory)
    {
        if (_cache.TryGetValue(cacheKey, out Tenant? cached))
        {
            _logger.LogDebug("Tenant cache hit: {CacheKey}", cacheKey);
            return cached;
        }

        var tenant = await factory();
        if (tenant != null)
        {
            _cache.Set(cacheKey, tenant, TimeSpan.FromMinutes(CacheDurationMinutes));
            _logger.LogDebug("Tenant cached: {CacheKey} Expiry={Minutes}m", cacheKey, CacheDurationMinutes);
        }

        return tenant;
    }

    /// <summary>
    /// Invalidates subdomain cache entry.
    /// </summary>
    public void InvalidateSubdomain(string subdomain)
    {
        var key = SubdomainKeyPrefix + subdomain.ToLowerInvariant();
        _cache.Remove(key);
        _logger.LogDebug("Tenant subdomain cache invalidated: {Subdomain}", subdomain);
    }

    /// <summary>
    /// Invalidates custom domain cache entry.
    /// </summary>
    public void InvalidateCustomDomain(string customDomain)
    {
        var key = CustomDomainKeyPrefix + customDomain.ToLowerInvariant().TrimEnd('.');
        _cache.Remove(key);
        _logger.LogDebug("Tenant custom domain cache invalidated: {Domain}", customDomain);
    }

    /// <summary>
    /// Clears all tenant cache entries.
    /// </summary>
    public void Clear()
    {
        // Memory cache doesn't provide enumeration, so this is a no-op
        // In production, consider using a named cache strategy or external cache
        _logger.LogInformation("Tenant cache clear requested (no-op for memory cache)");
    }

    public string GetSubdomainCacheKey(string subdomain) => SubdomainKeyPrefix + subdomain.ToLowerInvariant();
    public string GetCustomDomainCacheKey(string customDomain) => CustomDomainKeyPrefix + customDomain.ToLowerInvariant().TrimEnd('.');
}
