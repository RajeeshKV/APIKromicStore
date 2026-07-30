using KromicStore.Application.Common.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace KromicStore.Infrastructure.Services;

/// <summary>
/// Service for retrieving information about the currently authenticated user.
/// Implemented as scoped service to work with HTTP context.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated
    {
        get
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true;
        }
    }

    public Guid UserId
    {
        get
        {
            if (!IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var userId = _httpContextAccessor?.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("UserId claim not found.");

            return Guid.Parse(userId);
        }
    }

    public string? Email
    {
        get
        {
            return _httpContextAccessor?.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }
    }

    public IEnumerable<string> Roles
    {
        get
        {
            var roleClaims = _httpContextAccessor?.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role) ?? [];
            return roleClaims.Select(c => c.Value);
        }
    }

    public string? GetClaim(string claimType)
    {
        return _httpContextAccessor?.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
