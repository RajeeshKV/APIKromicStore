namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// CORS (Cross-Origin Resource Sharing) configuration.
/// Controls which origins are allowed to make requests to the API.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Comma-separated list of allowed origins.
    /// 
    /// Example: "https://store.kromic.in,https://admin.kromic.in,http://localhost:3000,http://localhost:5173"
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
            return (false, "AllowedOrigins must contain at least one origin");
        }

        // Validate each origin as a valid URI
        foreach (var origin in origins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var _))
            {
                return (false, $"Invalid origin URL format: '{origin}'");
            }
        }

        // Check for duplicates
        if (origins.Count != origins.Distinct().Count())
        {
            return (false, "AllowedOrigins contains duplicate values");
        }

        return (true, null);
    }

    /// <summary>
    /// Checks if an origin is allowed.
    /// </summary>
    public bool IsOriginAllowed(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return false;

        return ParsedAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
