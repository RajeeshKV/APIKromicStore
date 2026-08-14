using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Tenants;
using KromicStore.Infrastructure.Persistence;
using KromicStore.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KromicStore.API.Middleware;

/// <summary>
/// Resolves tenant from request with production-grade security.
/// Priority: Custom Domain → Subdomain → JWT Token (with DB validation) → Development Header
/// 
/// Security Notes:
/// - JWT tenantId claim is ONLY trusted if explicitly flagged with "allowTenantIdBypass" claim
/// - All JWT-based resolution includes database verification of user-tenant relationship
/// - Prevents token scope creep and unauthorized tenant access
/// - Development mode supports X-Kromic-TenantId header for testing without DNS setup
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private const string DevelopmentTenantHeader = "X-Kromic-TenantId";
    private const string AllowTenantBypassClaim = "allowTenantIdBypass";
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
        // Try to resolve tenant from host header (custom domain or subdomain) - most secure, preferred method
        if (!await ResolveTenantFromHostAsync(httpContext, tenantContext, dbContext))
        {
            // Fallback: Try to resolve from JWT token with database validation (production-safe)
            if (!await ResolveTenantFromJwtWithValidationAsync(httpContext, tenantContext, dbContext))
            {
                // Development fallback: X-Kromic-TenantId header (for local testing without DNS)
                if (_environment.IsDevelopment() && httpContext.Request.Headers.TryGetValue(DevelopmentTenantHeader, out var value))
                {
                    if (Guid.TryParse(value, out var tenantId))
                    {
                        tenantContext.Set(tenantId);
                        _logger.LogInformation("Tenant resolved by development header for {TenantId}", tenantId);
                    }
                }
            }
        }

        await _next(httpContext);
    }

    /// <summary>
    /// Resolves tenant from JWT with security validation.
    /// Only trusts JWT tenantId claim if:
    /// 1. The user is authenticated (has "sub" claim)
    /// 2. The "allowTenantIdBypass" claim is explicitly set to "true"
    /// 3. The user actually belongs to the tenant (verified in database)
    /// 4. The tenant is active
    /// </summary>
    private async Task<bool> ResolveTenantFromJwtWithValidationAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        KromicStoreDbContext dbContext)
    {
        // Only process if user is authenticated
        if (!httpContext.User.Identity?.IsAuthenticated ?? false)
            return false;

        // Check if bypass is explicitly allowed in the token
        var bypassClaim = httpContext.User.FindFirst(AllowTenantBypassClaim)?.Value;
        if (bypassClaim != "true")
            return false;

        // Get tenantId from JWT
        if (httpContext.User.FindFirst("tenantId")?.Value is not { Length: > 0 } tenantIdClaim)
            return false;

        if (!Guid.TryParse(tenantIdClaim, out var tenantIdFromJwt))
            return false;

        // Get user ID from JWT
        if (httpContext.User.FindFirst("sub")?.Value is not { Length: > 0 } userIdClaim)
            return false;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return false;

        // *** CRITICAL SECURITY CHECK ***
        // Verify the user actually belongs to this tenant in the database
        var user = await dbContext.Users
            .Where(u => u.Id == userId && u.TenantId == tenantIdFromJwt)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            _logger.LogWarning(
                "Security: User {UserId} attempted to access tenant {TenantId} but has no relationship in database. Possible token tampering.",
                userId, tenantIdFromJwt);
            return false;
        }

        // Verify tenant is active
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantIdFromJwt);
        if (tenant is null || !tenant.Status.IsActive())
        {
            _logger.LogWarning("Tenant {TenantId} not found or inactive for user {UserId}", tenantIdFromJwt, userId);
            return false;
        }

        tenantContext.Set(tenantIdFromJwt, storeName: tenant.StoreName);
        _logger.LogInformation(
            "Tenant resolved from JWT with database validation for UserId={UserId}, TenantId={TenantId}",
            userId, tenantIdFromJwt);
        return true;
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

