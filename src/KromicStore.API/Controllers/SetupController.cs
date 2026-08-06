using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Identity;

namespace KromicStore.API.Controllers;

/// <summary>
/// Bootstrap endpoint for initial system setup (superuser creation).
/// This controller should be disabled after initial setup in production.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public sealed class SetupController : ControllerBase
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SetupController> _logger;

    public SetupController(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILogger<SetupController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a superuser account. Only works if no users exist in the system.
    /// Should only be called once during initial setup.
    /// </summary>
    [HttpPost("create-superuser")]
    public async Task<IActionResult> CreateSuperuser([FromBody] CreateSuperuserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (request == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                return BadRequest("First name is required");

            if (string.IsNullOrWhiteSpace(request.LastName))
                return BadRequest("Last name is required");

            // Security: Only allow if no users exist
            var userCount = await _dbContext.Users.CountAsync(cancellationToken);
            if (userCount > 0)
            {
                _logger.LogWarning("Setup: Attempted to create superuser when users already exist");
                return BadRequest("System already initialized. Superuser already exists.");
            }

            // Validate password strength
            if (request.Password.Length < 8)
                return BadRequest("Password must be at least 8 characters long");

            if (!request.Password.Any(char.IsUpper))
                return BadRequest("Password must contain at least one uppercase letter");

            if (!request.Password.Any(char.IsLower))
                return BadRequest("Password must contain at least one lowercase letter");

            if (!request.Password.Any(char.IsDigit))
                return BadRequest("Password must contain at least one digit");

            if (!request.Password.Any(c => !char.IsLetterOrDigit(c)))
                return BadRequest("Password must contain at least one special character");

            // Hash password
            var hashedPassword = _passwordHasher.Hash(request.Password);

            // Create superuser
            var superuser = KromicStore.Domain.Identity.User.CreateSuperUser(
                email: request.Email,
                passwordHash: hashedPassword,
                firstName: request.FirstName.Trim(),
                lastName: request.LastName.Trim());

            // Mark email as verified for superuser (skip verification step)
            superuser.MarkEmailVerified();

            // Add to database
            _dbContext.AddEntity(superuser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Superuser created successfully: {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = "Superuser created successfully",
                email = superuser.Email,
                id = superuser.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating superuser");
            return StatusCode(500, new { success = false, message = "An error occurred while creating superuser", error = ex.Message });
        }
    }

    /// <summary>
    /// Check if the system has been initialized (has users).
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetSetupStatus(CancellationToken cancellationToken)
    {
        try
        {
            var userCount = await _dbContext.Users.CountAsync(cancellationToken);
            var initialized = userCount > 0;

            return Ok(new
            {
                initialized,
                userCount,
                message = initialized ? "System is initialized" : "System needs initialization - create superuser"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking setup status");
            return StatusCode(500, new { success = false, message = "An error occurred", error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for creating a superuser.
/// </summary>
public sealed record CreateSuperuserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);
