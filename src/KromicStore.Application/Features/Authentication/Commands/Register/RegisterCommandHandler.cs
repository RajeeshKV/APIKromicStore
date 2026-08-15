using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Authentication.DTOs;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Identity;
using KromicStore.Domain.Tenants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainRefreshToken = KromicStore.Domain.Identity.RefreshToken;

namespace KromicStore.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Handles tenant user registration.
///
/// Flow:
///   1. Reject duplicate email (globally unique — one account per email).
///   2. Hash password.
///   3. Create a Tenant for this user (slug derived from email local-part).
///   4. Create User as TenantAdmin, linked to the new Tenant.
///   5. Assign TenantAdmin role.
///   6. Activate the tenant immediately (no manual approval for now).
///   7. Issue refresh token (hashed, persisted).
///   8. Generate email verification token.
///   9. Persist everything in one transaction.
///  10. Return tokens with tenantId in response.
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthTokenResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantRepository     _tenantRepository;
    private readonly IPasswordHasher       _passwordHasher;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IApplicationDbContext db,
        ITenantRepository     tenantRepository,
        IPasswordHasher       passwordHasher,
        ITokenService         tokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _db               = db;
        _tenantRepository = tenantRepository;
        _passwordHasher   = passwordHasher;
        _tokenService     = tokenService;
        _logger           = logger;
    }

    public async Task<AuthTokenResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // ── 1. Check duplicate email (global) ────────────────────────────────
        var emailExists = await _db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (emailExists)
            throw new ConflictException($"An account with email '{normalizedEmail}' already exists.");

        // ── 2. Hash password ─────────────────────────────────────────────────
        var passwordHash = _passwordHasher.Hash(request.Password);

        // ── 3. Create Tenant ─────────────────────────────────────────────────
        // Derive a unique slug from the email local-part (e.g. "john.doe@gmail.com" → "johndoe")
        var emailLocal   = normalizedEmail.Split('@')[0];
        var baseSlug     = new string(emailLocal.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "store";

        // Ensure slug is unique — append random suffix if taken
        var slug = baseSlug;
        var attempt = 0;
        while (await _tenantRepository.SubdomainExistsAsync(slug, cancellationToken: cancellationToken))
        {
            attempt++;
            slug = $"{baseSlug}{attempt}";
        }

        var firstName = request.FirstName.Trim();
        var lastName  = request.LastName.Trim();
        var storeName = $"{firstName}'s Store";

        var tenant = Tenant.Create(name: storeName, slug: slug, storeName: storeName);
        tenant.AddPlatformDomain(slug, isPrimary: true);
        tenant.Activate(); // activate immediately on registration

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        // Don't SaveChanges yet — do it all in one transaction at the end

        // ── 4. Create User as TenantAdmin ─────────────────────────────────────
        var user = User.CreateTenantUser(tenant.Id, normalizedEmail, passwordHash, firstName, lastName);
        _db.AddEntity(user);

        // Assign owner on tenant now that we have the user id
        tenant.AssignOwner(user.Id);

        // ── 5. Assign TenantAdmin role ────────────────────────────────────────
        var tenantAdminRole = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == Roles.TenantAdmin, cancellationToken);

        if (tenantAdminRole is not null)
            _db.AddEntity(UserRole.Create(user.Id, tenantAdminRole.Id));

        var roleNames = tenantAdminRole is not null
            ? new List<string> { Roles.TenantAdmin }
            : new List<string>();

        // ── 6. Refresh token ──────────────────────────────────────────────────
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var hashedRefresh   = _tokenService.HashToken(rawRefreshToken);
        var refreshExpiry   = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays);

        _db.AddEntity(DomainRefreshToken.Create(
            userId:       user.Id,
            tokenHash:    hashedRefresh,
            expiresOnUtc: refreshExpiry,
            deviceName:   request.DeviceName,
            ipAddress:    request.IpAddress));

        // ── 7. Email verification token (24-hour expiry) ──────────────────────
        var rawVerifyToken = _tokenService.GenerateRefreshToken();
        var hashedVerify   = _tokenService.HashToken(rawVerifyToken);

        _db.AddEntity(EmailVerificationToken.Create(user.Id, hashedVerify, DateTime.UtcNow.AddHours(24)));

        // ── 8. Persist everything ─────────────────────────────────────────────
        await _tenantRepository.SaveChangesAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User registered. UserId={UserId} Email={Email} TenantId={TenantId} Slug={Slug}",
            user.Id, normalizedEmail, tenant.Id, slug);

        _logger.LogDebug(
            "Email verification token (DEV ONLY): {Token}", rawVerifyToken);

        // ── 9. Issue access token (contains tenantId + role claims) ──────────
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);

        return new AuthTokenResponse(
            AccessToken:      accessToken,
            RefreshToken:     rawRefreshToken,
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
