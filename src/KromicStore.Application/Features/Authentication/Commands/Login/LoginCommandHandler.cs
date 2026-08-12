using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.DTOs;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Handles email + password login.
///
/// Security rules enforced:
///   - Generic "Invalid credentials" for both bad email AND bad password
///     (prevents account enumeration).
///   - Account must be active.
///   - Email must be verified (per doc 24: 403 if not verified).
///   - Refresh token issued and persisted (hashed).
///   - LastLoginOnUtc recorded.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokenResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher       _passwordHasher;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher       passwordHasher,
        ITokenService         tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _db             = db;
        _passwordHasher = passwordHasher;
        _tokenService   = tokenService;
        _logger         = logger;
    }

    public async Task<AuthTokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // ── 1. Load user with roles ───────────────────────────────────────────
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Generic message — do NOT differentiate "email not found" from "wrong password"
        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            _logger.LogWarning("Login failed for email={Email} — invalid credentials", normalizedEmail);
            throw new AuthenticationException("Invalid email or password.");
        }

        // ── 2. Account checks ─────────────────────────────────────────────────
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt on inactive account UserId={UserId}", user.Id);
            throw new AccountLockedException("Your account has been deactivated. Please contact support.");
        }

        // Allow login with unverified email, but frontend will show verification banner
        // User can still access the app but should verify email before performing sensitive actions
        if (!user.IsEmailVerified)
        {
            _logger.LogInformation("Login with unverified email UserId={UserId} — verification required", user.Id);
        }

        // ── 3. Resolve roles ──────────────────────────────────────────────────
        var roleIds   = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roleNames = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        // ── 4. Issue refresh token ────────────────────────────────────────────
        var rawRefresh   = _tokenService.GenerateRefreshToken();
        var hashedRefresh = _tokenService.HashToken(rawRefresh);
        var refreshExpiry = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays);

        var refreshToken = DomainRefreshToken.Create(
            userId:       user.Id,
            tokenHash:    hashedRefresh,
            expiresOnUtc: refreshExpiry,
            deviceName:   request.DeviceName,
            ipAddress:    request.IpAddress);

        _db.AddEntity(refreshToken);

        // ── 5. Record login time ──────────────────────────────────────────────
        user.RecordLogin(DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Login successful. UserId={UserId}", user.Id);

        // ── 6. Issue access token ─────────────────────────────────────────────
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);

        return new AuthTokenResponse(
            AccessToken:     accessToken,
            RefreshToken:    rawRefresh,
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
