using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Resets a user's password.
///
/// Rules (doc 98):
///   - Token must be valid, unconsumed, and not expired.
///   - Password complexity enforced by validator.
///   - ChangePasswordHash increments TokenVersion — this implicitly invalidates
///     all issued JWTs that carry the old tokenVersion claim.
///   - All active refresh tokens are explicitly revoked.
///   - Reset token is consumed (single-use).
/// </summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher       _passwordHasher;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher       passwordHasher,
        ITokenService         tokenService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _db             = db;
        _passwordHasher  = passwordHasher;
        _tokenService    = tokenService;
        _logger          = logger;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.Token);

        var resetToken = await _db.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (resetToken is null || resetToken.IsConsumed)
        {
            _logger.LogWarning("Password reset: invalid or consumed token used.");
            throw new AuthenticationException("Invalid or expired reset token.");
        }

        if (resetToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset: expired token used. TokenId={TokenId}", resetToken.Id);
            throw new AuthenticationException("This password reset link has expired. Please request a new one.");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == resetToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            throw new AuthenticationException("Invalid or expired reset token.");

        // Update password — increments TokenVersion (invalidates all JWTs)
        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePasswordHash(newHash);

        // Consume the reset token
        resetToken.Consume(DateTime.UtcNow);

        // Revoke all active refresh tokens
        var activeRefreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedOnUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var rt in activeRefreshTokens)
            rt.Revoke(DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Password reset complete. UserId={UserId}. {Count} refresh tokens revoked.",
            user.Id, activeRefreshTokens.Count);
    }
}
