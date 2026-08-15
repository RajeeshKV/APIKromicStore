using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Tenants;
using KromicStore.Infrastructure.Persistence;
using KromicStore.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KromicStore.API.Middleware;

/// <summary>
/// Resolves tenant from request with production-grade security.
/// 
/// Resolution priority:
///   1. Custom Domain / Subdomain  → verified against DB
///   2. JWT tenantId claim          → verified against DB (user must belong to tenant)
///   3. X-Kromic-TenantId header    → development only
///
/// For admin panel (admin.kromic.in) the host is NOT a tenant subdomain.
/// Tenant context is resolved from the JWT tenantId claim for authenticated users.
///
/// Security Notes:
/// - JWT tenantId claim is ONLY trusted if user actually belongs to that tenant in DB
/// - Prevents token scope creep and cross-tenant access
/// - SuperAdmin has no tenantId — they access /api/v1/super/* endpoints only
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private const string DevelopmentTenantHeader = "X-Kromic-TenantId";
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, IWebHostEnvironment environment, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, TenantContext tenantContext, KromicStoreDbContext dbContext)
    {
        // 1. Try subdomain / custom domain first (storefront and storefront-api calls)
        if (!await ResolveTenantFromHostAsync(httpContext, tenantContext, dbContext))
        {
            // 2. Try JWT tenantId claim — covers admin panel calls (admin.kromic.in)
            //    where host is not a tenant subdomain
            if (!await ResolveTenantFromJwtAsync(httpContext, tenantContext, dbContext))
            {
                // 3. Development fallback: explicit header for local testing without DNS
                if (_environment.IsDevelopment() &&
                    httpContext.Request.Headers.TryGetValue(DevelopmentTenantHeader, out var value) &&
                    Guid.TryParse(value, out var devTenantId))
                {
                    tenantContext.Set(devTenantId);
                    _logger.LogInformation("Tenant resolved via dev header: {TenantId}", devTenantId);
                }
            }
        }

        await _next(httpContext);
    }

    // ── Resolution strategies ─────────────────────────────────────────────────

    /// <summary>
    /// Resolves tenant from the JWT tenantId claim with database ownership check.
    /// Used primarily for requests from the admin panel (admin.kromic.in).
    /// </summary>
    private async Task<bool> ResolveTenantFromJwtAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        KromicStoreDbContext dbContext)
    {
        // Must be authenticated
        if (httpContext.User.Identity?.IsAuthenticated != true)
            return false;

        // Extract tenantId from JWT
        var tenantIdClaim = httpContext.User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            return false;

        // Extract user id
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        // *** Security check: user must actually belong to this tenant ***
        // Use IgnoreQueryFilters() here because TenantContext is not yet set,
        // so the global filter would exclude everything.
        var userBelongsToTenant = await dbContext.UserSet
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id == userId && u.TenantId == tenantId && !u.IsDeleted);

        if (!userBelongsToTenant)
        {
            _logger.LogWarning(
                "Security: User {UserId} claimed tenant {TenantId} but has no DB relationship. Possible token tampering.",
                userId, tenantId);
            return false;
        }

        // Tenant must exist and be active
        var tenant = await dbContext.TenantSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted);

        if (tenant is null || !tenant.Status.IsActive())
        {
            _logger.LogWarning("Tenant {TenantId} not found or inactive for user {UserId}", tenantId, userId);
            return false;
        }

        tenantContext.Set(tenantId, storeName: tenant.StoreName);
        _logger.LogDebug("Tenant resolved from JWT for UserId={UserId}, TenantId={TenantId}", userId, tenantId);
        return true;
    }

    /// <summary>
    /// Resolves tenant from the Host header: custom domain or subdomain.
    /// </summary>
    private async Task<bool> ResolveTenantFromHostAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        KromicStoreDbContext dbContext)
    {
        var host = httpContext.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
            return false;

        var normalizedHost = host.ToLowerInvariant().TrimEnd('.');

        // Try exact custom domain match
        var tenantByDomain = await dbContext.TenantSet
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.Domains.Any(d => d.CustomDomain == normalizedHost && d.IsVerified))
            .FirstOrDefaultAsync();

        if (tenantByDomain is not null && tenantByDomain.Status.IsActive())
        {
            tenantContext.Set(tenantByDomain.Id, storeName: tenantByDomain.StoreName);
            _logger.LogDebug("Tenant resolved from custom domain: {Host} → {TenantId}", normalizedHost, tenantByDomain.Id);
            return true;
        }

        // Try subdomain (e.g. "mystore" from "mystore.kromic.in")
        var subdomain = ExtractSubdomain(normalizedHost);
        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenantBySubdomain = await dbContext.TenantSet
                .IgnoreQueryFilters()
                .Where(t => !t.IsDeleted && t.Domains.Any(d => d.Subdomain == subdomain))
                .FirstOrDefaultAsync();

            if (tenantBySubdomain is not null)
            {
                if (!tenantBySubdomain.Status.IsActive())
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await httpContext.Response.WriteAsJsonAsync(new { error = "Tenant is inactive or suspended." });
                    // Return true to stop further resolution — we already wrote the response
                    return true;
                }

                tenantContext.Set(tenantBySubdomain.Id, storeName: tenantBySubdomain.StoreName);
                _logger.LogDebug("Tenant resolved from subdomain: {Subdomain} → {TenantId}", subdomain, tenantBySubdomain.Id);
                return true;
            }
        }

        return false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the subdomain from "subdomain.kromic.in".
    /// Returns null for the base domain or unrecognised patterns.
    /// </summary>
    private static string? ExtractSubdomain(string host)
    {
        const string platformDomain = "kromic.in";

        if (!host.EndsWith(platformDomain, StringComparison.OrdinalIgnoreCase))
            return null;

        var parts = host.Split('.');
        // Expect exactly "subdomain.kromic.in" (3 parts)
        if (parts.Length != 3)
            return null;

        var subdomain = parts[0];
        return string.IsNullOrWhiteSpace(subdomain) ? null : subdomain;
    }
}
