using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.DTOs;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Rotates a refresh token.
///
/// Rotation rules (doc 96):
///   1. Hash the incoming token and look it up.
///   2. Reject if not found, expired, or revoked.
///   3. If the token is already revoked, treat as replay attack — revoke ALL
///      tokens for that user (security incident response).
///   4. Revoke the current token.
///   5. Issue a new refresh token + new access token.
///   6. Persist.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokenResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IApplicationDbContext db,
        ITokenService         tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _db           = db;
        _tokenService  = tokenService;
        _logger        = logger;
    }

    public async Task<AuthTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.Token);

        // Load the token — bypass global query filter (RefreshToken has no soft-delete)
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            _logger.LogWarning("Refresh token not found — possible replay or forged token");
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        // ── Replay attack detection ───────────────────────────────────────────
        if (storedToken.IsRevoked)
        {
            // Grace window: if the token was just revoked in the last 5 seconds,
            // it is likely a duplicate concurrent request from the frontend
            // (e.g. two API calls fire simultaneously on page load, both triggering
            // a refresh). In that case we do NOT nuke all tokens — we simply reject
            // this second request. The frontend should use the new token from the
            // first successful refresh.
            var justRevoked = storedToken.RevokedOnUtc.HasValue &&
                              (DateTime.UtcNow - storedToken.RevokedOnUtc.Value).TotalSeconds <= 10;

            if (justRevoked)
            {
                _logger.LogWarning(
                    "Duplicate refresh attempt within grace window for UserId={UserId}. Rejecting silently.",
                    storedToken.UserId);
                throw new AuthenticationException("Invalid or expired refresh token.");
            }

            _logger.LogWarning(
                "Revoked refresh token reused outside grace window. Revoking ALL tokens for UserId={UserId}. Potential compromise.",
                storedToken.UserId);

            var allTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedOnUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var t in allTokens)
                t.Revoke(DateTime.UtcNow);

            await _db.SaveChangesAsync(cancellationToken);
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        if (storedToken.IsExpired(DateTime.UtcNow))
        {
            _logger.LogWarning("Expired refresh token used. UserId={UserId}", storedToken.UserId);
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        // ── Load user with roles ──────────────────────────────────────────────
        // IgnoreQueryFilters: the refresh endpoint has no tenant context set.
        // Without this, users with a TenantId would be invisible.
        var user = await _db.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId && !u.IsDeleted, cancellationToken);

        if (user is null || !user.IsActive)
            throw new AuthenticationException("Invalid or expired refresh token.");

        var roleIds   = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roleNames = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        // ── Rotate ────────────────────────────────────────────────────────────
        storedToken.Revoke(DateTime.UtcNow);

        var rawNew     = _tokenService.GenerateRefreshToken();
        var hashedNew  = _tokenService.HashToken(rawNew);
        var expiry     = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays);

        var newToken = DomainRefreshToken.Create(
            userId:       user.Id,
            tokenHash:    hashedNew,
            expiresOnUtc: expiry,
            deviceName:   request.DeviceName ?? storedToken.DeviceName,
            ipAddress:    request.IpAddress  ?? storedToken.IPAddress);

        _db.AddEntity(newToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated. UserId={UserId}", user.Id);

        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);

        return new AuthTokenResponse(
            AccessToken:     accessToken,
            RefreshToken:    rawNew,
            ExpiresInSeconds: _tokenService.AccessTokenExpirationSeconds,
            User: new UserProfileResponse(
                Id:              user.Id,
                TenantId:        user.TenantId,
                Email:           user.Email,
                FirstName:       user.FirstName,
                LastName:        user.LastName,
                IsEmailVerified: user.IsEmailVerified,
                Roles:           roleNames));
    }
}
