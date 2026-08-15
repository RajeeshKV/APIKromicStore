using KromicStore.Application.Features.Authentication.Commands.ChangePassword;
using KromicStore.Application.Features.Authentication.Commands.ForgotPassword;
using KromicStore.Application.Features.Authentication.Commands.Login;
using KromicStore.Application.Features.Authentication.Commands.Logout;
using KromicStore.Application.Features.Authentication.Commands.Register;
using KromicStore.Application.Features.Authentication.Commands.ResendVerificationEmail;
using KromicStore.Application.Features.Authentication.Commands.ResetPassword;
using KromicStore.Application.Features.Authentication.Commands.VerifyEmail;
using KromicStore.Application.Features.Authentication.Commands.RefreshToken;
using KromicStore.Application.Features.Authentication.DTOs;
using KromicStore.Application.Features.Authentication.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers;

/// <summary>
/// Authentication endpoints — register, login, token refresh, logout,
/// email verification, and password management.
/// All endpoints are versioned under /api/v1/auth.
/// Controllers contain routing only; all logic lives in CQRS handlers.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    // ── Registration ──────────────────────────────────────────────────────────

    /// <summary>
    /// Register a new user account.
    /// Returns JWT + refresh token on success.
    /// An email verification message is queued automatically.
    /// </summary>
    /// <response code="201">Account created and tokens issued.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            FirstName:  request.FirstName,
            LastName:   request.LastName,
            Email:      request.Email,
            Password:   request.Password,
            Subdomain:  request.Subdomain,
            StoreName:  request.StoreName,
            DeviceName: request.DeviceName,
            IpAddress:  HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── Subdomain availability ────────────────────────────────────────────────

    /// <summary>
    /// Check if a subdomain is available for registration.
    /// Debounce calls from the UI — call after 300-500ms of no typing.
    /// </summary>
    /// <response code="200">Returns availability and preview URL.</response>
    /// <response code="400">Subdomain is missing or has invalid format.</response>
    [HttpGet("check-subdomain")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckSubdomain(
        [FromQuery] string subdomain,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return BadRequest(new { message = "Subdomain is required." });

        var normalized = subdomain.Trim().ToLowerInvariant();

        // Quick client-side-mirror format check before hitting the DB
        if (normalized.Length < 3 || normalized.Length > 63)
            return Ok(new { available = false, subdomain = normalized, reason = "Subdomain must be 3–63 characters." });

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^[a-z0-9][a-z0-9-]*[a-z0-9]$"))
            return Ok(new { available = false, subdomain = normalized, reason = "Use only lowercase letters, numbers, and hyphens. Cannot start or end with a hyphen." });

        var result = await _sender.Send(
            new Application.Features.Authentication.Queries.CheckSubdomain.CheckSubdomainQuery(normalized),
            cancellationToken);

        return Ok(new
        {
            available  = result.IsAvailable,
            subdomain  = result.Subdomain,
            reason     = result.Reason,
            previewUrl = result.IsAvailable ? $"https://{result.Subdomain}.kromic.in" : (string?)null
        });
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Authenticate with email and password.
    /// Returns JWT + refresh token on success.
    /// Note: Users can login with unverified email, but frontend should show 
    /// a verification banner when IsEmailVerified = false.
    /// </summary>
    /// <response code="200">Authenticated successfully (check IsEmailVerified for banner).</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="423">Account locked.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            Email:      request.Email,
            Password:   request.Password,
            DeviceName: request.DeviceName,
            IpAddress:  HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── Token refresh ─────────────────────────────────────────────────────────

    /// <summary>
    /// Exchange a valid refresh token for a new access + refresh token pair.
    /// The old refresh token is revoked immediately (rotation).
    /// </summary>
    /// <response code="200">Tokens rotated successfully.</response>
    /// <response code="401">Token invalid, expired, or revoked.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(
            Token:      request.RefreshToken,
            DeviceName: request.DeviceName,
            IpAddress:  HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Revoke the supplied refresh token.
    /// Idempotent — safe to call even if already logged out.
    /// </summary>
    /// <response code="204">Token revoked (or was already revoked).</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }

    // ── Email verification ────────────────────────────────────────────────────

    /// <summary>
    /// Verify email address using the token sent by email.
    /// The token is provided as a query parameter in the verification link.
    /// </summary>
    /// <response code="204">Email verified.</response>
    /// <response code="400">Token missing.</response>
    /// <response code="401">Token invalid, expired, or already used.</response>
    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new VerifyEmailCommand(token), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Resend the email verification link.
    /// Always returns 204 to prevent account enumeration.
    /// </summary>
    /// <response code="204">Request accepted (email sent if account exists).</response>
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ResendVerificationEmailCommand(request.Email), cancellationToken);
        return NoContent();
    }

    // ── Password management ───────────────────────────────────────────────────

    /// <summary>
    /// Request a password reset email.
    /// Always returns 204 to prevent account enumeration.
    /// </summary>
    /// <response code="204">Request accepted (email sent if account exists).</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Complete password reset using the token from the reset email.
    /// All active sessions are revoked on success.
    /// </summary>
    /// <response code="204">Password reset successfully.</response>
    /// <response code="400">Validation failure or invalid/expired token.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmPassword),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Change the authenticated user's password.
    /// Requires the current password for re-authentication.
    /// All other sessions are revoked on success.
    /// </summary>
    /// <response code="204">Password changed successfully.</response>
    /// <response code="400">Validation failure or wrong current password.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.ConfirmPassword),
            cancellationToken);

        return NoContent();
    }

    // ── Current user ──────────────────────────────────────────────────────────

    /// <summary>
    /// Get the profile of the currently authenticated user.
    /// </summary>
    /// <response code="200">User profile returned.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(result);
    }
}

// ── Request models ────────────────────────────────────────────────────────────
// Kept in the same file intentionally — these are thin HTTP contracts only.
// They translate HTTP input into CQRS commands. No business logic here.

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Subdomain,
    string? StoreName = null,
    string? DeviceName = null);

public sealed record LoginRequest(
    string Email,
    string Password,
    string? DeviceName = null);

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string? DeviceName = null);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ResendVerificationRequest(string Email);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
