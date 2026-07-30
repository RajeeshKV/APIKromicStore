using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.DTOs;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Handles tenant user registration.
///
/// Flow:
///   1. Reject duplicate email within the same tenant scope.
///   2. Hash password.
///   3. Create User aggregate.
///   4. Assign default role (TenantAdmin for first registration, Customer thereafter).
///   5. Issue refresh token (hashed, persisted).
///   6. Issue JWT access token.
///   7. Generate email verification token — caller dispatches the email.
///   8. Persist everything in one transaction.
///   9. Return tokens.
///
/// Note: email sending is intentionally NOT done in this handler.
/// The domain event EmailVerificationRequested will be dispatched post-commit
/// once the outbox / notification pipeline is wired up.
/// For now we log the token so development can proceed without Brevo.
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthTokenResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher       _passwordHasher;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher       passwordHasher,
        ITokenService         tokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _db             = db;
        _passwordHasher = passwordHasher;
        _tokenService   = tokenService;
        _logger         = logger;
    }

    public async Task<AuthTokenResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Check duplicate email (tenant-scoped) ──────────────────────────
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            throw new ConflictException($"An account with email '{normalizedEmail}' already exists.");

        // ── 2. Hash password ──────────────────────────────────────────────────
        var passwordHash = _passwordHasher.Hash(request.Password);

        // ── 3. Resolve TenantId from context ──────────────────────────────────
        // Registration at this endpoint always creates a TenantAdmin.
        // TenantId is resolved from the current tenant context (injected via
        // ITenantContext global query filter). Since this is a public endpoint
        // the tenant must already be provisioned and resolved by middleware.
        // For now we create users without TenantId (SuperUser style) until
        // tenant provisioning is wired. A separate RegisterTenantUserCommand
        // will enforce the TenantId properly.
        var user = User.CreateSuperUser(normalizedEmail, passwordHash, request.FirstName, request.LastName);

        _db.AddEntity(user);

        // ── 4. Assign role ────────────────────────────────────────────────────
        var tenantAdminRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.Name == Roles.TenantAdmin, cancellationToken);

        if (tenantAdminRole is not null)
        {
            var userRole = UserRole.Create(user.Id, tenantAdminRole.Id);
            _db.AddEntity(userRole);
        }

        var roleNames = tenantAdminRole is not null
            ? new List<string> { Roles.TenantAdmin }
            : new List<string>();

        // ── 5. Refresh token ──────────────────────────────────────────────────
        var rawRefreshToken  = _tokenService.GenerateRefreshToken();
        var hashedRefresh    = _tokenService.HashToken(rawRefreshToken);
        var refreshExpiry    = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays);

        var refreshToken = DomainRefreshToken.Create(
            userId:     user.Id,
            tokenHash:  hashedRefresh,
            expiresOnUtc: refreshExpiry,
            deviceName: request.DeviceName,
            ipAddress:  request.IpAddress);

        _db.AddEntity(refreshToken);

        // ── 6. Email verification token (24-hour expiry) ──────────────────────
        var rawVerifyToken  = _tokenService.GenerateRefreshToken(); // same CSPRNG
        var hashedVerify    = _tokenService.HashToken(rawVerifyToken);
        var verifyExpiry    = DateTime.UtcNow.AddHours(24);

        var verificationToken = EmailVerificationToken.Create(user.Id, hashedVerify, verifyExpiry);
        _db.AddEntity(verificationToken);

        // ── 7. Persist ────────────────────────────────────────────────────────
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User registered. UserId={UserId} Email={Email}",
            user.Id, normalizedEmail);

        // Development convenience — log raw token so /verify-email can be tested
        // without email delivery. Remove once Brevo is wired.
        _logger.LogDebug(
            "Email verification token (DEV ONLY — never log in production): {Token}",
            rawVerifyToken);

        // ── 8. Access token ───────────────────────────────────────────────────
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);

        return new AuthTokenResponse(
            AccessToken:     accessToken,
            RefreshToken:    rawRefreshToken,
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
