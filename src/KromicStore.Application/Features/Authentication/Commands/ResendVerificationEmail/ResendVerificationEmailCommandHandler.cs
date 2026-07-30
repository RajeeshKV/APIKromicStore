using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Authentication.Commands.ResendVerificationEmail;

/// <summary>
/// Resends the verification email.
///
/// Security note (doc 98): always return success even if the email is not found
/// to prevent account enumeration. The audit log captures the real outcome.
/// </summary>
public sealed class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

    public ResendVerificationEmailCommandHandler(
        IApplicationDbContext db,
        ITokenService         tokenService,
        ILogger<ResendVerificationEmailCommandHandler> logger)
    {
        _db           = db;
        _tokenService  = tokenService;
        _logger        = logger;
    }

    public async Task Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Silent success — prevent enumeration
        if (user is null)
        {
            _logger.LogWarning("Resend verification: email not found {Email}", normalizedEmail);
            return;
        }

        if (user.IsEmailVerified)
        {
            _logger.LogInformation("Resend verification: email already verified UserId={UserId}", user.Id);
            return;
        }

        // Consume all existing unused tokens for this user
        var existingTokens = await _db.EmailVerificationTokens
            .Where(t => t.UserId == user.Id && t.ConsumedOnUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var t in existingTokens)
            t.Consume(DateTime.UtcNow);

        // Issue fresh token
        var rawToken    = _tokenService.GenerateRefreshToken();
        var hashedToken = _tokenService.HashToken(rawToken);
        var expiry      = DateTime.UtcNow.AddHours(24);

        var verificationToken = EmailVerificationToken.Create(user.Id, hashedToken, expiry);
        _db.AddEntity(verificationToken);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification email token regenerated. UserId={UserId}", user.Id);
        _logger.LogDebug("Verification token (DEV ONLY): {Token}", rawToken);
    }
}
