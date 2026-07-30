using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Tenants;
using KromicStore.Infrastructure.Persistence;
using KromicStore.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.API.Middleware;

/// <summary>
/// Resolves tenant from request Host header.
/// Priority: Custom Domain → Subdomain → Development Header
/// Rejects requests with invalid/inactive tenants.
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
        // Try to resolve tenant from host header
        if (!await ResolveTenantFromHostAsync(httpContext, tenantContext, dbContext))
        {
            // Development fallback: X-Kromic-TenantId header
            if (_environment.IsDevelopment() && httpContext.Request.Headers.TryGetValue(DevelopmentTenantHeader, out var value))
            {
                if (Guid.TryParse(value, out var tenantId))
                {
                    tenantContext.Set(tenantId);
                    _logger.LogInformation("Tenant resolved by development header for {TenantId}", tenantId);
                }
            }
        }

        await _next(httpContext);
    }

    private static async Task<bool> ResolveTenantFromHostAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        KromicStoreDbContext dbContext)
    {
        var host = httpContext.Request.Host.Host;

        if (string.IsNullOrWhiteSpace(host))
            return false;

        var normalizedHost = NormalizeHost(host);

        // Try custom domain first
        var tenantByCustomDomain = await dbContext.Tenants
            .Where(t => t.Domains.Any(d => d.CustomDomain == normalizedHost && d.IsVerified))
            .FirstOrDefaultAsync();

        if (tenantByCustomDomain is not null && tenantByCustomDomain.Status.IsActive())
        {
            tenantContext.Set(tenantByCustomDomain.Id, storeName: tenantByCustomDomain.StoreName);
            return true;
        }

        // Try subdomain (extract from "subdomain.kromic.in")
        var subdomain = ExtractSubdomain(normalizedHost);
        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenantBySubdomain = await dbContext.Tenants
                .Where(t => t.Domains.Any(d => d.Subdomain == subdomain))
                .FirstOrDefaultAsync();

            if (tenantBySubdomain is not null && tenantBySubdomain.Status.IsActive())
            {
                tenantContext.Set(tenantBySubdomain.Id, storeName: tenantBySubdomain.StoreName);
                return true;
            }

            if (tenantBySubdomain is not null)
            {
                // Tenant found but inactive
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(new { error = "Tenant is inactive" });
                return true;
            }
        }

        return false;
    }

    private static string NormalizeHost(string host)
        => host.ToLowerInvariant().TrimEnd('.');

    /// <summary>
    /// Extracts subdomain from "subdomain.kromic.in" format.
    /// Returns null for invalid formats or base domain.
    /// </summary>
    private static string? ExtractSubdomain(string host)
    {
        const string platformDomain = "kromic.in";

        if (!host.EndsWith(platformDomain, StringComparison.OrdinalIgnoreCase))
            return null;

        var parts = host.Split('.');
        if (parts.Length != 3)
            return null; // Not in "subdomain.kromic.in" format

        var subdomain = parts[0];
        return string.IsNullOrWhiteSpace(subdomain) ? null : subdomain;
    }
}
