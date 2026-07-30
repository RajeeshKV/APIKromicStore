using KromicStore.Application.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Authentication.Commands.Logout;

/// <summary>
/// Revokes the supplied refresh token.
/// Silently succeeds if the token is already revoked or not found
/// (idempotent — safe for client retry).
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService         _tokenService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IApplicationDbContext db,
        ITokenService         tokenService,
        ILogger<LogoutCommandHandler> logger)
    {
        _db           = db;
        _tokenService  = tokenService;
        _logger        = logger;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (token is null || token.IsRevoked)
        {
            _logger.LogDebug("Logout: token already revoked or not found — no action needed.");
            return;
        }

        token.Revoke(DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Logout: refresh token revoked. UserId={UserId}", token.UserId);
    }
}
