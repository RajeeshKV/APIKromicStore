using KromicStore.Domain.Identity;

namespace KromicStore.Application.Common.Abstractions;

/// <summary>
/// Abstracts JWT access token generation and refresh token generation/hashing.
/// The application layer calls this; Infrastructure implements it.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate a signed JWT access token for the given user and their roles.
    /// Claims included: sub, email, tenantId, role[], jti, iat, exp.
    /// </summary>
    string GenerateAccessToken(User user, IEnumerable<string> roles);

    /// <summary>
    /// Generate a cryptographically secure plaintext refresh token.
    /// The caller is responsible for hashing it before persisting.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// One-way hash a token value (refresh token, email token, reset token).
    /// Uses SHA-256.
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Returns the configured access token lifetime in seconds.
    /// </summary>
    int AccessTokenExpirationSeconds { get; }

    /// <summary>
    /// Returns the configured refresh token lifetime in days.
    /// </summary>
    int RefreshTokenExpirationDays { get; }
}
