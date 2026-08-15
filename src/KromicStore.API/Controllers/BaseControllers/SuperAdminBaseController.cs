using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers.BaseControllers;

/// <summary>
/// STRICT: SuperAdmin only endpoints.
/// 
/// Access Control:
/// - Only users with SuperAdmin role can access
/// - No TenantId allowed (SuperAdmin are platform admins)
/// - Accessing from TenantAdmin role → 403 Forbidden
/// 
/// Routes: /api/v1/super/*
/// Example: /api/v1/super/tenants, /api/v1/super/settings
/// </summary>
[ApiController]
[Route("api/v1/super")]
[Authorize(Roles = "SuperAdmin")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(void))]
public abstract class SuperAdminBaseController : ControllerBase
{
    protected readonly ILogger<SuperAdminBaseController> _logger;

    protected SuperAdminBaseController(ILogger<SuperAdminBaseController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
