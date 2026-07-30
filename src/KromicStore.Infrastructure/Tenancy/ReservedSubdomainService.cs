using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Infrastructure.Tenancy;

/// <summary>
/// Validates subdomains against a reserved/blocked list.
/// Prevents platform endpoints from being claimed as tenant subdomains.
/// </summary>
public sealed class ReservedSubdomainService : IReservedSubdomainService
{
    // Reserved subdomains that cannot be used as tenant identifiers
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Platform infrastructure
        "admin",
        "api",
        "app",
        "dashboard",
        "docs",
        "help",
        "support",
        "mail",
        "smtp",
        "ftp",
        "ssh",
        "git",

        // Common web conventions
        "www",
        "mail",
        "ftp",
        "localhost",
        "webmail",

        // Root/base names
        "kromic",
        "root",
        "main",
        "base",

        // Authentication/security
        "login",
        "logout",
        "auth",
        "auth0",
        "signin",
        "signup",
        "forgot-password",
        "reset-password",

        // Platform features
        "shop",
        "store",
        "marketplace",
        "admin-panel",

        // Common services
        "cdn",
        "static",
        "assets",
        "images",
        "media",
        "files",
        "downloads",
        "uploads",

        // Status/monitoring
        "status",
        "health",
        "monitoring",
        "metrics",
        "analytics",
        "logs",

        // Development
        "staging",
        "qa",
        "test",
        "dev",
        "development",
        "sandbox",
        "demo",

        // Generic
        "example",
        "sample",
        "temp",
        "temporary",
        "backup",
        "archive",

        // Social media / standard handles
        "twitter",
        "facebook",
        "instagram",
        "linkedin",
        "youtube",
        "github",
        "blog",
    };

    bool IReservedSubdomainService.IsReserved(string subdomain) => IsReserved(subdomain);

    public static bool IsReserved(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return true;

        var normalized = subdomain.Trim().ToLowerInvariant();
        return ReservedNames.Contains(normalized);
    }

    public static IEnumerable<string> GetReservedSubdomains() => ReservedNames.OrderBy(x => x);
}
