using System.Text.RegularExpressions;

namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// CORS (Cross-Origin Resource Sharing) configuration.
/// Controls which origins are allowed to make requests to the API.
/// Supports both explicit origins and wildcard patterns.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Comma-separated list of allowed origins.
    /// Supports explicit origins and wildcard patterns.
    /// 
    /// Examples:
    /// - Explicit: "https://store.kromic.in,https://admin.kromic.in,http://localhost:3000"
    /// - Wildcard: "https://*.kromic.in" (matches any subdomain)
    /// - Mixed: "https://*.kromic.in,http://localhost:3000"
    /// </summary>
    public string AllowedOrigins { get; set; } = string.Empty;

    /// <summary>
    /// Gets the parsed allowed origins as a collection.
    /// </summary>
    public IReadOnlyCollection<string> ParsedAllowedOrigins =>
        AllowedOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

    /// <summary>
    /// Validates the CORS configuration.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        var origins = ParsedAllowedOrigins.ToList();

        if (origins.Count == 0)
        {
            return (true, null); // Empty origins allowed - may be set via environment
        }

        // Validate each origin (either as absolute URI or wildcard pattern)
        foreach (var origin in origins)
        {
            if (!IsValidOrigin(origin))
            {
                return (false, $"Invalid origin format: '{origin}' (must be valid URI or wildcard pattern like 'https://*.example.com')");
            }
        }

        // Check for duplicates
        if (origins.Count != origins.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            return (false, "AllowedOrigins contains duplicate values");
        }

        return (true, null);
    }

    /// <summary>
    /// Checks if an origin is allowed.
    /// Supports both exact matching and wildcard pattern matching.
    /// </summary>
    public bool IsOriginAllowed(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return false;

        return ParsedAllowedOrigins.Any(allowedOrigin => 
            MatchesOriginPattern(origin, allowedOrigin));
    }

    /// <summary>
    /// Checks if a given origin is a valid origin or wildcard pattern.
    /// </summary>
    private static bool IsValidOrigin(string origin)
    {
        // Check if it's a wildcard pattern
        if (origin.Contains('*'))
        {
            return IsValidWildcardPattern(origin);
        }

        // Otherwise, validate as absolute URI
        return Uri.TryCreate(origin, UriKind.Absolute, out var _);
    }

    /// <summary>
    /// Validates that a wildcard pattern is well-formed.
    /// 
    /// Valid patterns:
    /// - "https://*.example.com"
    /// - "https://*.example.com:443"
    /// - "http://localhost:*"
    /// - "*://example.com"
    /// </summary>
    private static bool IsValidWildcardPattern(string pattern)
    {
        // Must contain ://
        if (!pattern.Contains("://"))
            return false;

        // Pattern must have at least scheme and host
        var parts = pattern.Split("://");
        if (parts.Length != 2)
            return false;

        var scheme = parts[0];
        var hostPart = parts[1];

        // Scheme must be non-empty and contain only valid characters
        if (string.IsNullOrWhiteSpace(scheme) || !Regex.IsMatch(scheme, @"^[a-zA-Z][a-zA-Z0-9+.-]*$|^\*$"))
            return false;

        // Host part must be non-empty
        if (string.IsNullOrWhiteSpace(hostPart))
            return false;

        // Host part can contain * but must have other valid characters
        // Examples: *.example.com, localhost:*, etc.
        if (!Regex.IsMatch(hostPart, @"^[\w\-.*:]+$"))
            return false;

        return true;
    }

    /// <summary>
    /// Checks if an origin matches a pattern (exact or wildcard).
    /// </summary>
    private static bool MatchesOriginPattern(string origin, string pattern)
    {
        // Exact match
        if (origin.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            return true;

        // Wildcard matching
        if (pattern.Contains('*'))
        {
            return WildcardMatch(origin, pattern);
        }

        return false;
    }

    /// <summary>
    /// Performs wildcard pattern matching for origins.
    /// 
    /// Patterns like "https://*.example.com" match "https://store.example.com", "https://admin.example.com", etc.
    /// Patterns like "http://localhost:*" match "http://localhost:3000", "http://localhost:5173", etc.
    /// </summary>
    private static bool WildcardMatch(string origin, string pattern)
    {
        try
        {
            // Convert wildcard pattern to regex
            // Escape regex special characters except *
            var regexPattern = Regex.Escape(pattern)
                .Replace("\\*", ".*");

            // Add anchors to match the entire string
            regexPattern = $"^{regexPattern}$";

            return Regex.IsMatch(origin, regexPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            // If regex fails, fall back to exact match
            return origin.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
