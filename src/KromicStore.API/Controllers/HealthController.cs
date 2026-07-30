using KromicStore.API.Contracts;
using KromicStore.Application.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly ITenantContext _tenantContext;

    public HealthController(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var payload = new { status = "Healthy", tenantResolved = _tenantContext.IsResolved, _tenantContext.TenantId };
        return Ok(ApiResponse<object>.Ok(payload, null, HttpContext.TraceIdentifier));
    }
}
