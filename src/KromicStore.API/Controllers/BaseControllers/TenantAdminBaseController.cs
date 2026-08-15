using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers.BaseControllers;

/// <summary>
/// STRICT: TenantAdmin + StoreManager endpoints.
/// 
/// Access Control:
/// - Only users with TenantAdmin or StoreManager role can access
/// - SuperAdmin role gets 403 (no access to tenant endpoints)
/// - User must have TenantId in JWT
/// - Trying to access another tenant's data → blocked by tenant context
/// 
/// Routes: /api/v1/tenant/*
/// Example: /api/v1/tenant/products, /api/v1/tenant/orders, /api/v1/tenant/customers
/// </summary>
[ApiController]
[Authorize(Roles = "TenantAdmin,StoreManager")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(void))]
public abstract class TenantAdminBaseController : ControllerBase
{
    protected readonly ILogger<TenantAdminBaseController> _logger;

    protected TenantAdminBaseController(ILogger<TenantAdminBaseController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
