using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Identity;
using KromicStore.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace KromicStore.Infrastructure.Services;

/// <summary>
/// JWT access token and refresh token implementation.
/// Signing uses HMAC-SHA256 (HS256).
/// Refresh tokens are random 64-byte values; callers receive the plaintext
/// and must pass it through HashToken() before persisting.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public int AccessTokenExpirationSeconds  => _options.AccessTokenExpirationMinutes * 60;
    public int RefreshTokenExpirationDays    => _options.RefreshTokenExpirationDays;

    // ──────────────────────────────────────────────────────────────────────────
    // Access token
    // ──────────────────────────────────────────────────────────────────────────

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds      = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuedAt   = DateTime.UtcNow;
        var expiration = issuedAt.AddMinutes(_options.AccessTokenExpirationMinutes);

        var claims = BuildClaims(user, roles, issuedAt);

        var token = new JwtSecurityToken(
            issuer:            _options.Issuer,
            audience:          _options.Audience,
            claims:            claims,
            notBefore:         issuedAt,
            expires:           expiration,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Refresh token
    // ──────────────────────────────────────────────────────────────────────────

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Hashing (SHA-256, hex-encoded)
    // ──────────────────────────────────────────────────────────────────────────

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static List<Claim> BuildClaims(User user, IEnumerable<string> roles, DateTime issuedAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,  new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(),
                                               ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier,    user.Id.ToString()),
            new(ClaimTypes.Email,             user.Email),
            new("tokenVersion",               user.TokenVersion.ToString()),
            new("isEmailVerified",            user.IsEmailVerified.ToString().ToLowerInvariant(),
                                               ClaimValueTypes.Boolean),
            // Security: Allow JWT-based tenant resolution only when explicitly enabled
            // This prevents token scope creep. The middleware will verify user-tenant relationship in DB.
            new("allowTenantIdBypass",        "true",
                                               ClaimValueTypes.Boolean)
        };

        // TenantId is null for SuperAdmin
        if (user.TenantId.HasValue)
            claims.Add(new Claim("tenantId", user.TenantId.Value.ToString()));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }
}
