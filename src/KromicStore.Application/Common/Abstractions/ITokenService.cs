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
    /// Claims included: sub, email, tenantId, isEmailVerified, allowTenantIdBypass, role[], jti, iat, exp.
    /// 
    /// Security Notes:
    /// - allowTenantIdBypass: When true, allows middleware to resolve tenant from JWT claims.
    ///   The middleware still validates the user-tenant relationship in the database.
    /// - tenantId: Only included for non-SuperAdmin users. Middleware trusts this claim
    ///   ONLY when allowTenantIdBypass=true AND user-tenant relationship is verified in DB.
    /// - isEmailVerified: Indicates if user's email has been verified. Frontend should use
    ///   this to show verification banners or block sensitive actions.
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
