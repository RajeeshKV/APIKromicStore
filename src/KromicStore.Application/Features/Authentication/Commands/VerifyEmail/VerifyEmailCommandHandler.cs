using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Authentication.Commands.VerifyEmail;

/// <summary>
/// Validates the email verification token and activates the user's email.
///
/// Rules (doc 98):
///   - Token must exist, not be consumed, and not be expired.
///   - Single-use: consumed immediately after validation.
///   - If already verified, succeed silently (idempotent).
/// </summary>
public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IApplicationDbContext db,
        ITokenService         tokenService,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _db           = db;
        _tokenService  = tokenService;
        _logger        = logger;
    }

    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.Token);

        var verificationToken = await _db.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (verificationToken is null)
            throw new AuthenticationException("Invalid or expired verification token.");

        if (verificationToken.IsConsumed)
        {
            _logger.LogWarning("Email verification token already consumed. TokenId={TokenId}", verificationToken.Id);
            throw new AuthenticationException("This verification link has already been used.");
        }

        if (verificationToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired email verification token. TokenId={TokenId}", verificationToken.Id);
            throw new AuthenticationException("This verification link has expired. Please request a new one.");
        }

        // Load user
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == verificationToken.UserId, cancellationToken);

        if (user is null)
            throw new AuthenticationException("Invalid or expired verification token.");

        // Idempotent — already verified is fine
        if (!user.IsEmailVerified)
            user.MarkEmailVerified();

        // Consume token (single-use)
        verificationToken.Consume(DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email verified. UserId={UserId}", user.Id);
    }
}
