using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KromicStore.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.API.Controllers;

/// <summary>
/// Admin/Testing endpoints for development and testing purposes.
/// These endpoints should be disabled in production.
/// </summary>
[ApiController]
[Route("api/v1/admin-test")]
[Authorize(Roles = "SuperAdmin,TenantAdmin")]
public class AdminTestingController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<AdminTestingController> _logger;

    public AdminTestingController(IApplicationDbContext db, ILogger<AdminTestingController> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Marks a user's email as verified (for development/testing only).
    /// </summary>
    /// <param name="email">The user's email to verify.</param>
    /// <returns>Success or error message.</returns>
    /// <response code="200">Email marked as verified.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> VerifyUserEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        if (user == null)
            return NotFound(new { message = "User not found." });

        if (user.IsEmailVerified)
            return Ok(new { message = "Email is already verified." });

        user.MarkEmailVerified();
        await _db.SaveChangesAsync();

        _logger.LogInformation("Email verified for user {Email} (TESTING ONLY)", email);

        return Ok(new { message = "Email marked as verified successfully.", email = user.Email, isEmailVerified = true });
    }

    /// <summary>
    /// Gets the current user's email verification status.
    /// </summary>
    /// <returns>User email and verification status.</returns>
    /// <response code="200">Returns user verification status.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("email-verification-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetEmailVerificationStatus()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            email = user.Email,
            isEmailVerified = user.IsEmailVerified,
            userId = user.Id
        });
    }
}
