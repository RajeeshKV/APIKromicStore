using KromicStore.Application.Common.Abstractions;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Health check for tenant resolution functionality.
/// Verifies that the tenant context is accessible and database is reachable.
/// </summary>
public sealed class TenantResolutionHealthCheck : IHealthCheck
{
    private readonly ITenantContext _tenantContext;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ILogger<TenantResolutionHealthCheck> _logger;

    public TenantResolutionHealthCheck(
        ITenantContext tenantContext,
        KromicStoreDbContext dbContext,
        ILogger<TenantResolutionHealthCheck> logger)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if tenant context is set (for tenant-scoped requests)
            var tenantId = _tenantContext.TenantId;
            var details = new Dictionary<string, object>
            {
                { "TenantId", tenantId?.ToString() ?? "Not Set" },
                { "IsTenantScoped", tenantId.HasValue }
            };

            // Test database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            details["DatabaseConnected"] = canConnect;

            if (!canConnect)
            {
                _logger.LogWarning("Tenant resolution health check: database not reachable");
                return HealthCheckResult.Unhealthy("Database not reachable", new Exception("Cannot connect to database"), details);
            }

            _logger.LogDebug("Tenant resolution health check: OK");
            return HealthCheckResult.Healthy("Tenant resolution operational", details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant resolution health check failed");
            return HealthCheckResult.Unhealthy("Tenant resolution check failed", ex);
        }
    }
}

/// <summary>
/// Health check for database connectivity and query performance.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly KromicStoreDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(KromicStoreDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            sw.Stop();

            var details = new Dictionary<string, object>
            {
                { "Connected", canConnect },
                { "ResponseTime", $"{sw.ElapsedMilliseconds}ms" }
            };

            if (!canConnect)
            {
                _logger.LogWarning("Database health check: cannot connect");
                return HealthCheckResult.Unhealthy("Database not reachable", null, details);
            }

            if (sw.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Database health check: slow response ({Elapsed}ms)", sw.ElapsedMilliseconds);
                return HealthCheckResult.Degraded("Database responding slowly", null, details);
            }

            _logger.LogDebug("Database health check: OK ({Elapsed}ms)", sw.ElapsedMilliseconds);
            return HealthCheckResult.Healthy("Database operational", details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database check failed", ex);
        }
    }
}
