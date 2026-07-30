using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Generates a password reset token (45-minute expiry per doc 98).
/// Consumes any outstanding unused reset tokens first.
/// Always responds with HTTP 200 — never reveals whether the email exists.
/// </summary>
public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext db,
        ITokenService         tokenService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _db           = db;
        _tokenService  = tokenService;
        _logger        = logger;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("ForgotPassword: email not found {Email}", normalizedEmail);
            return; // Silent success — anti-enumeration
        }

        // Consume all existing unused reset tokens
        var existing = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.ConsumedOnUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var t in existing)
            t.Consume(DateTime.UtcNow);

        // Issue new token — 45 min expiry
        var rawToken    = _tokenService.GenerateRefreshToken();
        var hashedToken = _tokenService.HashToken(rawToken);
        var expiry      = DateTime.UtcNow.AddMinutes(45);

        var resetToken = PasswordResetToken.Create(user.Id, hashedToken, expiry);
        _db.AddEntity(resetToken);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset token issued. UserId={UserId}", user.Id);
        _logger.LogDebug("Reset token (DEV ONLY): {Token}", rawToken);
        // TODO: dispatch email via outbox once Brevo integration is complete
    }
}
