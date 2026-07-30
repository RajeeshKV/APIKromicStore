using System.ComponentModel.DataAnnotations;

namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed JWT configuration. Validated on startup.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    [Range(1, 730)]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
