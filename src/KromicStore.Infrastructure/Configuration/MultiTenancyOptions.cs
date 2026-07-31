namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Platform-level multi-tenancy configuration.
/// These settings are managed by the Platform Administrator during deployment
/// and are never configurable by tenants.
/// </summary>
public sealed class MultiTenancyOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "MultiTenancy";

    /// <summary>
    /// Comma-separated list of reserved subdomains that cannot be registered by tenants.
    /// These subdomains are owned by the platform.
    /// 
    /// Example: "store,storeapi,admin,api,auth,docs,health,status,cdn,assets"
    /// </summary>
    public string ReservedSubdomains { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of platform-owned subdomains that should bypass tenant resolution.
    /// Requests to these hosts should never attempt tenant resolution.
    /// 
    /// Example: "store,storeapi,admin"
    /// </summary>
    public string ExcludedSubdomains { get; set; } = string.Empty;

    /// <summary>
    /// Gets the parsed reserved subdomains as a collection.
    /// </summary>
    public IReadOnlyCollection<string> ParsedReservedSubdomains =>
        ReservedSubdomains
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .ToList();

    /// <summary>
    /// Gets the parsed excluded subdomains as a collection.
    /// </summary>
    public IReadOnlyCollection<string> ParsedExcludedSubdomains =>
        ExcludedSubdomains
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .ToList();

    /// <summary>
    /// Validates the multi-tenancy configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        // Check for duplicate reserved subdomains
        var reserved = ParsedReservedSubdomains.ToList();
        if (reserved.Count != reserved.Distinct().Count())
        {
            return (false, "ReservedSubdomains contains duplicate values");
        }

        // Check for duplicate excluded subdomains
        var excluded = ParsedExcludedSubdomains.ToList();
        if (excluded.Count != excluded.Distinct().Count())
        {
            return (false, "ExcludedSubdomains contains duplicate values");
        }

        // Validate subdomain format (alphanumeric and hyphens only, no leading/trailing hyphens)
        var invalidReserved = reserved.FirstOrDefault(s => !IsValidSubdomain(s));
        if (invalidReserved != null)
        {
            return (false, $"Invalid reserved subdomain format: '{invalidReserved}'");
        }

        var invalidExcluded = excluded.FirstOrDefault(s => !IsValidSubdomain(s));
        if (invalidExcluded != null)
        {
            return (false, $"Invalid excluded subdomain format: '{invalidExcluded}'");
        }

        return (true, null);
    }

    /// <summary>
    /// Checks if a subdomain is valid (alphanumeric and hyphens, no leading/trailing hyphens).
    /// </summary>
    private static bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        if (subdomain.StartsWith("-") || subdomain.EndsWith("-"))
            return false;

        return subdomain.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Checks if a subdomain is reserved.
    /// </summary>
    public bool IsReservedSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        return ParsedReservedSubdomains.Contains(subdomain.ToLowerInvariant());
    }

    /// <summary>
    /// Checks if a subdomain is excluded from tenant resolution.
    /// </summary>
    public bool IsExcludedSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        return ParsedExcludedSubdomains.Contains(subdomain.ToLowerInvariant());
    }
}
