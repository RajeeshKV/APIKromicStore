using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Authentication.Commands.ChangePassword;

/// <summary>
/// Changes the password for the currently authenticated user.
///
/// Rules:
///   - Requires valid current password (re-authentication).
///   - ChangePasswordHash increments TokenVersion — invalidates all existing JWTs.
///   - All other refresh tokens revoked; the caller must re-login.
/// </summary>
public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher       _passwordHasher;
    private readonly ICurrentUserService   _currentUser;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher       passwordHasher,
        ICurrentUserService   currentUser,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _db             = db;
        _passwordHasher  = passwordHasher;
        _currentUser     = currentUser;
        _logger          = logger;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new AuthenticationException("User not found.");

        // Re-verify current password
        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            _logger.LogWarning("ChangePassword failed — wrong current password. UserId={UserId}", userId);
            throw new AuthenticationException("Current password is incorrect.");
        }

        // Update — bumps TokenVersion, invalidating all issued JWTs
        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePasswordHash(newHash);

        // Revoke all refresh tokens (force re-login everywhere)
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedOnUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var rt in tokens)
            rt.Revoke(DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Password changed. UserId={UserId}. {Count} sessions revoked.",
            userId, tokens.Count);
    }
}
